using UnityEngine;

namespace SavePeps.Monetization
{
    /// <summary>
    /// Store-specific public SDK keys live outside the generated scene so a
    /// scene rebuild cannot erase release configuration. RevenueCat public
    /// keys identify an app; they are not secret server credentials.
    /// </summary>
    [CreateAssetMenu(menuName = "Peps/RevenueCat Settings", fileName = "RevenueCatSettings")]
    public sealed class RevenueCatSettings : ScriptableObject
    {
        [Tooltip("RevenueCat Test Store public SDK key. Used only in Editor or Development Builds.")]
        [SerializeField] private string _testStoreApiKey = "test_KgZRyWyGqqRofWFaYkfaefscJpS";

        [Tooltip("RevenueCat Google Play public SDK key (typically starts with goog_). Required for release builds.")]
        [SerializeField] private string _googlePlayApiKey;

        public string TestStoreApiKey => _testStoreApiKey?.Trim();
        public string GooglePlayApiKey => _googlePlayApiKey?.Trim();

        public string ApiKeyFor(bool developmentBuild) =>
            developmentBuild ? TestStoreApiKey : GooglePlayApiKey;
    }
}
