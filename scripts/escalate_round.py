#!/usr/bin/env python3
"""
Single-Round Escalation Tooling CLI for Save Peps.

Provides a unified workflow for auditing, authoring, reseeding, validating,
and testing escalation on a single round at a time (R1-R12).

Usage:
  python3 scripts/escalate_round.py audit <round_num>
  python3 scripts/escalate_round.py reseed <round_num>
  python3 scripts/escalate_round.py validate <round_num>
  python3 scripts/escalate_round.py capture-sheet <round_num> [output_dir]
  python3 scripts/escalate_round.py device-prep <round_num>
  python3 scripts/escalate_round.py device-test <round_num>
  python3 scripts/escalate_round.py status
"""

import sys
import os
import subprocess
import json
import time
import xml.etree.ElementTree as ET

UNITY_BIN = "/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity"
PROJ_PATH = "/Users/michi/save-peps/unity/SavePeps"
ADB_BIN = "/Applications/Unity/Hub/Editor/6000.3.21f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb"
PKG_NAME = "fan.sound.savepeps"

def run_unity(execute_method=None, test_platform=None, env=None, log_file="/tmp/unity_cmd.log"):
    cmd = [UNITY_BIN, "-batchmode", "-projectPath", PROJ_PATH, "-logFile", log_file]
    
    if test_platform:
        cmd.extend(["-runTests", "-testPlatform", test_platform, "-testResults", "/tmp/edit.xml"])
    else:
        cmd.extend(["-quit", "-nographics"])
        if execute_method:
            cmd.extend(["-executeMethod", execute_method])

    cmd_env = os.environ.copy()
    if env:
        cmd_env.update(env)

    # Check for Unity lock
    lockfile = os.path.join(PROJ_PATH, "Temp", "UnityLockfile")
    if os.path.exists(lockfile):
        print("[WARN] Temp/UnityLockfile exists. Checking for active Unity processes...")
        try:
            out = subprocess.check_output(["pgrep", "-lf", "Unity.app/Contents/MacOS/Unity"]).decode()
            if out.strip():
                print(f"[ERROR] Unity is currently running:\n{out.strip()}\nWait for it to finish.")
                return False
        except subprocess.CalledProcessError:
            print("[INFO] Stale lockfile detected, proceeding.")

    print(f"[RUN] {' '.join(cmd)}")
    p = subprocess.run(cmd, env=cmd_env, capture_output=True)
    
    if os.path.exists(log_file):
        with open(log_file, "r") as f:
            content = f.read()
            if "error CS" in content or "Scripts have compiler errors" in content:
                print("[ERROR] Compilation errors detected:")
                for line in content.splitlines():
                    if "error CS" in line:
                        print("  " + line)
                return False
    return True

def cmd_audit(round_num):
    print(f"\n--- AUDITING ESCALATION: ROUND {round_num} ---")
    log_file = f"/tmp/audit_r{round_num}.log"
    ok = run_unity(
        execute_method="SavePeps.EditorTools.EscalationWorkflow.AuditFromCli",
        env={"ROUND_NUM": str(round_num)},
        log_file=log_file
    )
    if not ok:
        print("[FAIL] Audit failed to execute.")
        return 1

    if os.path.exists(log_file):
        with open(log_file, "r") as f:
            lines = f.readlines()
            capture = False
            for line in lines:
                if "ESCALATION AUDIT" in line:
                    capture = True
                if capture:
                    if "UnityEngine.Debug" in line:
                        break
                    print(line.rstrip())
    return 0

def cmd_reseed(round_num):
    print(f"\n--- RESEEDING ROUND {round_num} (ISOLATED) ---")
    log_file = f"/tmp/reseed_r{round_num}.log"
    ok = run_unity(
        execute_method="SavePeps.EditorTools.EscalationWorkflow.ReseedFromCli",
        env={"ROUND_NUM": str(round_num)},
        log_file=log_file
    )
    if not ok:
        print(f"[FAIL] Reseeding round {round_num} failed.")
        return 1
    print(f"[OK] Round {round_num} reseeded successfully.")
    return 0

def cmd_validate(round_num):
    print(f"\n--- VALIDATING CATALOG & ROUND {round_num} ---")
    log_file = f"/tmp/validate_r{round_num}.log"
    ok = run_unity(
        test_platform="EditMode",
        log_file=log_file
    )
    if not ok:
        print("[FAIL] Test run failed.")
        return 1

    xml_path = "/tmp/edit.xml"
    if os.path.exists(xml_path):
        root = ET.parse(xml_path).getroot()
        total = root.get('total')
        passed = root.get('passed')
        failed = root.get('failed')
        print(f"[TESTS] Total: {total}, Passed: {passed}, Failed: {failed}")
        if failed and int(failed) > 0:
            for tc in root.iter('test-case'):
                if tc.get('result') != 'Passed':
                    print("  FAIL:", tc.get('fullname'))
                    msg = tc.find('.//message')
                    if msg is not None and msg.text:
                        print("   ", msg.text.strip()[:400])
            return 1
        print("[OK] All EditMode validation tests passed.")
        return 0
    return 1

def cmd_capture_sheet(round_num, out_dir=None):
    if not out_dir:
        out_dir = f"/tmp/round_{round_num}_stages"
    os.makedirs(out_dir, exist_ok=True)
    print(f"\n--- CAPTURING STAGE PREVIEWS: ROUND {round_num} -> {out_dir} ---")
    log_file = f"/tmp/capture_r{round_num}.log"
    ok = run_unity(
        execute_method="SavePeps.EditorTools.EscalationWorkflow.CaptureFromCli",
        env={"ROUND_NUM": str(round_num), "OUTPUT_DIR": out_dir},
        log_file=log_file
    )
    if not ok:
        print(f"[FAIL] Capturing stages for round {round_num} failed.")
        return 1
    print(f"[OK] Stages captured in {out_dir}:")
    for f in sorted(os.listdir(out_dir)):
        if f.endswith(".png"):
            print("  -", f)
    return 0

def cmd_device_prep(round_num):
    print(f"\n--- PREPARING PIXEL 4 FOR ROUND {round_num} ---")
    completed_rounds = list(range(1, round_num))
    completed_rescues = []
    for r in completed_rounds:
        completed_rescues.extend([f"r{(r-1)*3 + 1:02d}", f"r{(r-1)*3 + 2:02d}", f"r{(r-1)*3 + 3:02d}"])

    save_data = {
        "HighestUnlockedRound": round_num,
        "CurrentRound": round_num,
        "CompletedRounds": completed_rounds,
        "CompletedRescues": completed_rescues,
        "MasteredRescues": completed_rescues,
        "SoundEnabled": True,
        "HapticsEnabled": True,
        "CreatedTimestamp": 1787429100,
        "LastPlayedTimestamp": int(time.time()),
        "PlayCount": 10
    }

    local_save = f"/tmp/save_r{round_num}.json"
    with open(local_save, "w") as f:
        json.dump(save_data, f, indent=2)

    device_path = f"/storage/emulated/0/Android/data/{PKG_NAME}/files/save.json"
    
    subprocess.run([ADB_BIN, "shell", f"mkdir -p /storage/emulated/0/Android/data/{PKG_NAME}/files"])
    subprocess.run([ADB_BIN, "push", local_save, device_path])
    print(f"[OK] Pixel 4 save file configured for Round {round_num}.")
    return 0

def cmd_device_test(round_num, out_screenshots_dir=None):
    if not out_screenshots_dir:
        out_screenshots_dir = f"/tmp/round_{round_num}_device_shots"
    os.makedirs(out_screenshots_dir, exist_ok=True)

    cmd_device_prep(round_num)

    print(f"\n--- LAUNCHING & RUNNING ROUND {round_num} ON PIXEL 4 ---")
    subprocess.run([ADB_BIN, "logcat", "-c"])
    subprocess.run([ADB_BIN, "shell", "am", "force-stop", PKG_NAME])
    subprocess.run([ADB_BIN, "shell", "am", "start", "-n", f"{PKG_NAME}/com.unity3d.player.UnityPlayerActivity"])
    
    print("[WAIT] Waiting for game to load...")
    time.sleep(3.5)

    # Tap 'Play' on home screen (x=540, y=1730)
    subprocess.run([ADB_BIN, "shell", "input", "tap", "540", "1730"])
    time.sleep(2.5)

    # Capture R{N}.1 stage
    shot1 = os.path.join(out_screenshots_dir, f"r{round_num}_1_stage.png")
    subprocess.run(f"{ADB_BIN} exec-out screencap -p > {shot1}", shell=True)
    print(f"[SHOT] Captured {shot1}")

    # Check logcat for active rescue
    out = subprocess.check_output(f"{ADB_BIN} logcat -d | grep -oE '\\[SavePeps\\].*'", shell=True).decode()
    print("[LOGS]\n" + "\n".join("  " + l for l in out.splitlines()[-6:]))
    return 0

def cmd_status():
    print("\n=== SAVE PEPS: 12-ROUND ESCALATION STATUS ===")
    run_unity(
        execute_method="SavePeps.EditorTools.EscalationWorkflow.AuditFromCli",
        env={"ROUND_NUM": "all"},
        log_file="/tmp/audit_all.log"
    )
    if os.path.exists("/tmp/audit_all.log"):
        with open("/tmp/audit_all.log", "r") as f:
            for line in f:
                if "ESCALATION AUDIT" in line or "Progression Check" in line or "[R" in line:
                    print(line.rstrip())
    return 0

def main():
    if len(sys.argv) < 2:
        print(__doc__)
        sys.exit(1)

    cmd = sys.argv[1].lower()
    
    if cmd == "status":
        sys.exit(cmd_status())
    
    if len(sys.argv) < 3:
        print(f"Error: Command '{cmd}' requires <round_num> argument (1-12).")
        sys.exit(1)

    try:
        round_num = int(sys.argv[2])
        if round_num < 1 or round_num > 12:
            raise ValueError()
    except ValueError:
        print(f"Error: Invalid round number '{sys.argv[2]}'. Must be 1-12.")
        sys.exit(1)

    if cmd == "audit":
        sys.exit(cmd_audit(round_num))
    elif cmd == "reseed":
        sys.exit(cmd_reseed(round_num))
    elif cmd == "validate":
        sys.exit(cmd_validate(round_num))
    elif cmd == "capture-sheet":
        out_dir = sys.argv[3] if len(sys.argv) > 3 else None
        sys.exit(cmd_capture_sheet(round_num, out_dir))
    elif cmd == "device-prep":
        sys.exit(cmd_device_prep(round_num))
    elif cmd == "device-test":
        out_dir = sys.argv[3] if len(sys.argv) > 3 else None
        sys.exit(cmd_device_test(round_num, out_dir))
    else:
        print(f"Unknown command: {cmd}\n")
        print(__doc__)
        sys.exit(1)

if __name__ == "__main__":
    main()
