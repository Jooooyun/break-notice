using UnityEngine;

namespace BreakNotice.Presentation
{
    /// <summary>
    /// Explicit positive allow-list root for structural camera blockers.
    /// Dynamic actors, carried objects, hitboxes, and interaction triggers must
    /// never be placed below this marker.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ShowcaseCameraWorldBlockerRoot : MonoBehaviour
    {
    }
}
