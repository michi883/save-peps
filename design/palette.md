# Save Peps — Art Direction Lock

Decided once, in P0. Changing any of this after the content sprint starts means re-touching every diorama, so it is a lock rather than a preference.

Target feeling: **cute, warm, playful, minimal, polished, readable at arm's length on a phone.** Small toy dioramas, not a world.

---

## 1. The palette

Inherited from Save Pip, which arrived at a warm, soft, high-contrast palette across 106 hand-drawn scenes. It is proven to read at small sizes and to keep 75 distinct objects visually distinguishable — both properties we need, and neither is cheap to rediscover.

Shipped as **`palette_atlas.png`** — a 4×9 texture, 36 swatches, 9 ramps of 4 steps (shade → base → light → highlight). Every mesh in the game samples this one texture. That is the entire material policy.

| Row | Ramp | Shade | Base | Light | Highlight |
|---|---|---|---|---|---|
| 0 | **Ink & shadow** | `#221D33` | `#3D3354` | `#57406B` | `#8E8BA7` |
| 1 | **Foliage** | `#5E8F51` | `#7FB069` | `#95C77E` | `#A9D488` |
| 2 | **Earth** | `#57402D` | `#6B4A34` | `#9C6748` | `#B27A58` |
| 3 | **Wood & sand** | `#8E6D50` | `#B08F6C` | `#C9A87F` | `#E8DCC8` |
| 4 | **Water** | `#5FB7D4` | `#6FC0E3` | `#8FD6F9` | `#CDEBF7` |
| 5 | **Sky & cream** | `#B8E6F5` | `#F7F3E8` | `#FFF3CE` | `#FFFFFF` |
| 6 | **Warm accent** | `#E8B62D` | `#FFB53E` | `#FFCF56` | `#FFDE8A` |
| 7 | **Pep A — coral** | `#E05A45` | `#FF7660` | `#FFB3A0` | `#F49FBC` |
| 8 | **Pep B — mint** | `#1FA396` | `#2EC4B6` | `#65D6CA` | `#B7A8E0` |

### Rules

- **`#3D3354` is the only dark.** Never pure black — not in shadows, not in outlines, not in eyes. This single choice is most of why Save Pip reads as warm rather than harsh.
- **Row 6 (warm accent) is reserved for attention.** Goal markers, glows, the shimmer on tappable objects, celebration. If everything glows, nothing does — do not use row 6 as a surface colour on scenery.
- **Rows 7 and 8 belong to the Peps.** No prop, no environment, no UI element uses coral or mint as a primary. The two characters must be the only things on screen wearing those hues, so the eye finds them instantly and the separation reads at a glance.
- **Import settings:** point (nearest) filter, **mipmaps off**, no compression (it is 185 bytes), sRGB on, wrap clamp. Bilinear filtering or mips would bleed adjacent swatches into each other.
- **UVs land in texel centres.** For a 4×9 atlas, column *c* row *r* → `u = (c + 0.5) / 4`, `v = (r + 0.5) / 9`.

---

## 2. Materials

**One material, `M_Palette`, for essentially all geometry.** URP/Lit, base map = `palette_atlas.png`, smoothness 0, metallic 0. Meshes carry colour in their UVs.

This is what makes CC0 kits from different sources look like one game: re-UV on import so every vertex points at a swatch, and the source pack's own textures are discarded entirely. It also collapses the scene into a handful of draw calls, which is most of the mobile performance budget solved for free.

Exceptions, and they are the complete list:
- `M_PalUnlit` — same texture, URP/Unlit, for anything that must ignore lighting (fx bits, the glow accents).
- `M_Face` — the Peps' face atlas, unlit, alpha-clipped.
- `M_Water` — one scrolling-UV transparent shader for water surfaces.

Anything that wants a fifth material needs a reason stated out loud.

---

## 3. Lighting and camera

- **One directional light** (warm, ~15° elevation off-axis) + a gradient ambient (sky `#B8E6F5` → ground `#E8DCC8`). No realtime shadows anywhere.
- **Blob shadows** — a soft dark ellipse decal under the Peps and every prop. Cheaper than shadow maps and reads better at this scale; also keeps objects visually planted, which matters for judging where a thing is in a 3D scene from a fixed angle.
- **Camera: perspective, FOV ~28°**, elevated and tilted down ~25°. The low FOV plus the tilt is what produces the tilt-shift toy-diorama read. Fixed per rescue, authored into each diorama prefab. No player control, ever.
- **Post-processing:** one global Volume — colour grading (LUT), gentle vignette, subtle bloom so row-6 accents glow. Nothing else. No depth of field (expensive on mobile and it fights readability), no motion blur, no SSAO.
- **Framing:** portrait only, locked. Compose to a 4:3 safe box; taller phones reveal more sky, never more mechanism. Nothing gameplay-relevant may sit outside the safe box.

---

## 4. Readability rules for tappable objects

These are pass/fail, not taste. A rescue that breaks one is a broken rescue.

1. Every tappable has a **soft rim highlight** (row 6) and a slow idle bob or shimmer. Static props do not move; tappables always do.
2. A tappable must **contrast in value**, not just hue, against whatever is behind it. Squint at the screen — if it disappears, re-place it.
3. **Collider ≫ mesh.** The tap target is a separate invisible collider, generously oversized. Save Pip's tap circles were consistently ~25% larger than the art, and that is the right ratio.
4. The three tappables must be **spatially separated** — never overlapping in screen space, never clustered in one corner.
5. **The arm's-length test:** hold a real phone at arm's length. If the predicament and the three choices are not all legible in three seconds, the scene is not finished. This is checked on device, not in the editor.

---

## 5. The diorama

Each rescue is a small toy world against its authored sky and atmosphere.

- Each round owns a distinct spatial silhouette. Grounded worlds expose a deliberate toy-base edge; suspended and orbital worlds may replace the base with rails, machinery, or open space.
- Compose for the portrait gameplay safe area. The predicament, both Peps, and all three choices must read together without flattening every world into the same platform.
- **One transition, used everywhere:** the solved diorama tilts and slides away, and the next drops in with a small settle bounce. The shared transition carries the whole game's sense of craft.

---

## 6. Character look

Character behavior is frozen in [`docs/core-ux.md`](../docs/core-ux.md). Visually:

- **Distinct silhouettes, not just colours.** Pep A taller and rounder (coral), Pep B smaller and squarer (mint). Both must be tellable apart in a 1024² icon and in a thumbnail.
- **Faces are a texture atlas** on a forward-facing quad — `neutral, worried, hopeful, panic, happy, love`. No blend shapes, no facial rig. Directly inherited from Save Pip's face-stack approach, which got a full emotional range out of swapping one drawing for another.
- Eyes and mouth use ink `#3D3354`. Blush uses `#FFB3A0` on both characters, regardless of body colour — it is the shared warmth that makes them read as a pair.
