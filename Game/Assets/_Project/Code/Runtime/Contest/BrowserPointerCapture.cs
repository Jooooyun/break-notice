using UnityEngine;
using UnityEngine.InputSystem;

namespace BreakNotice.Contest
{
    [DisallowMultipleComponent]
    public sealed class BrowserPointerCapture : MonoBehaviour
    {
        private void Update()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ReleasePointer();
                return;
            }

            if (Mouse.current != null &&
                (Mouse.current.leftButton.wasPressedThisFrame ||
                 Mouse.current.rightButton.wasPressedThisFrame))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
#endif
        }

        private void OnDisable()
        {
            ReleasePointer();
        }

        private static void ReleasePointer()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
