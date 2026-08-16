using UnityEngine;

namespace BreakNotice.Presentation
{
    /// <summary>
    /// Explicit presentation-only camera occluder root. Colliders below this
    /// marker continue to block gameplay actors, but must not constrain the
    /// camera position. Any visibility treatment is renderer-only.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ShowcaseCameraSoftOccluderRoot : MonoBehaviour
    {
    }
}
