namespace SavePeps.Progression
{
    /// <summary>
    /// A display-ready summary derived from the existing per-rescue marks.
    /// It deliberately is not saved separately: one source of truth prevents
    /// a round badge disagreeing with the three marks it represents.
    /// </summary>
    public readonly struct RoundProgress
    {
        public int Solved { get; }
        public int Stars { get; }
        public int Total { get; }

        public bool IsUnplayed => Solved == 0;
        public bool IsComplete => Total > 0 && Solved == Total;
        public bool IsPerfect => Total > 0 && Stars == Total;

        private RoundProgress(int solved, int stars, int total)
        {
            Solved = solved;
            Stars = stars;
            Total = total;
        }

        public static RoundProgress Read(RoundDefinition round, SaveData save)
        {
            var total = round?.Rescues?.Length ?? 0;
            var solved = 0;
            var stars = 0;

            for (var i = 0; i < total; i++)
            {
                var mark = save?.MarkFor(round.RescueAt(i)?.Id) ?? Mark.None;
                if (mark != Mark.None) solved++;
                if (mark == Mark.Star) stars++;
            }

            return new RoundProgress(solved, stars, total);
        }
    }
}
