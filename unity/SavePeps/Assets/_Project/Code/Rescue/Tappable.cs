using UnityEngine;

namespace SavePeps.Rescue
{
    /// <summary>
    /// Marks a prop as tappable and carries its id back to the runner.
    ///
    /// The collider is deliberately separate from and larger than the visible
    /// mesh — Save Pip's tap circles ran about 25% wider than the art, and
    /// that generosity is most of why it felt good on a phone. A player
    /// aiming at a small plank should not have to be accurate.
    ///
    /// This lives in its own file because Unity resolves a MonoBehaviour's
    /// script by filename: declared inside TapRouter.cs it deserialised as a
    /// missing script on every prop, and no tap could ever register.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class Tappable : MonoBehaviour
    {
        [Tooltip("Matches RescueObject.Id.")]
        public string ObjectId;

        /// <summary>The transform choreography drives for this prop.</summary>
        public AnimTarget Target { get; private set; }

        private void Awake() => Target = GetComponentInChildren<AnimTarget>();
    }
}
