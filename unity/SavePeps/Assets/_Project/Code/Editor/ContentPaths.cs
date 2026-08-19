namespace SavePeps.EditorTools
{
    /// <summary>
    /// Where authored content lives. One definition, because these strings are
    /// referenced from the seeder, the validator, the playback driver and the
    /// scene builder, and a path that drifts between them fails as "no
    /// catalogue found" rather than as anything that points at the cause.
    /// </summary>
    public static class ContentPaths
    {
        public const string Root = "Assets/_Project";

        public const string RescueDir = Root + "/Content/Rescues";
        public const string RoundDir = Root + "/Content/Rounds";
        public const string CatalogPath = Root + "/Content/Catalog.asset";

        public const string GameScenePath = Root + "/Scenes/Game.unity";

        public const string EnvironmentDir = Root + "/Art/Environments";
        public const string CharacterDir = Root + "/Art/Characters";
        public const string PropDir = Root + "/Art/Props";
    }
}
