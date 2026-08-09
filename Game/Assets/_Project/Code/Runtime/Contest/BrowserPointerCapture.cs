using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BreakNotice.Contest
{
    [DisallowMultipleComponent]
    public sealed class BrowserPointerCapture : MonoBehaviour
    {
        private bool isSupported;
        private bool isLocked;
        private bool captureFailed;
        private GUIStyle statusStyle;

        public bool IsSupported => isSupported;

        public bool IsLocked => isLocked;

        private void OnEnable()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.stickyCursorLock = false;
            isSupported = BNPointerLock_Install() != 0;
            BNPointerLock_SetEnabled(1);
            RefreshPointerState();
#endif
        }

        private void Update()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ReleasePointer();
                return;
            }

            RefreshPointerState();
#endif
        }

        private void OnApplicationFocus(bool hasFocus)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (!hasFocus)
            {
                ReleasePointer();
            }
#endif
        }

        private void OnDisable()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            BNPointerLock_SetEnabled(0);
#endif
            ReleasePointer();
        }

        public static string BuildStatusText(bool supported, bool locked, bool failed = false)
        {
            if (!supported)
            {
                return "POINTER LOCK UNAVAILABLE";
            }

            if (failed && !locked)
            {
                return "MOUSE CAPTURE BLOCKED - CLICK GAME VIEW TO RETRY";
            }

            return locked
                ? "MOUSE CAPTURED - ESC TO RELEASE"
                : "CLICK GAME VIEW TO CAPTURE MOUSE";
        }

        private void OnGUI()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            EnsureStyle();
            GUI.Label(
                new Rect(12f, 12f, Mathf.Min(430f, Screen.width - 24f), 34f),
                BuildStatusText(isSupported, isLocked, captureFailed),
                statusStyle);
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private void RefreshPointerState()
        {
            isSupported = BNPointerLock_IsSupported() != 0;
            captureFailed = BNPointerLock_HasError() != 0;
            ApplyPointerState(isSupported && BNPointerLock_IsLocked() != 0);
        }
#endif

        private void ReleasePointer()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            BNPointerLock_Exit();
#endif
            ApplyPointerState(false);
        }

        private void ApplyPointerState(bool locked)
        {
            isLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private void EnsureStyle()
        {
            if (statusStyle != null)
            {
                return;
            }

            statusStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern int BNPointerLock_Install();

        [DllImport("__Internal")]
        private static extern void BNPointerLock_SetEnabled(int enabled);

        [DllImport("__Internal")]
        private static extern int BNPointerLock_IsSupported();

        [DllImport("__Internal")]
        private static extern int BNPointerLock_IsLocked();

        [DllImport("__Internal")]
        private static extern int BNPointerLock_HasError();

        [DllImport("__Internal")]
        private static extern void BNPointerLock_Exit();
#endif
    }
}
