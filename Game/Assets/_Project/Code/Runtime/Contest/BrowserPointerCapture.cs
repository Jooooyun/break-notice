using System.Runtime.InteropServices;
using BreakNotice.Gameplay.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BreakNotice.Contest
{
    [DefaultExecutionOrder(-400)]
    [DisallowMultipleComponent]
    public sealed class BrowserPointerCapture : MonoBehaviour
    {
        [SerializeField]
        private InputSystemLogicalInputAdapter logicalInput;

        private bool isSupported;
        private bool isLocked;
        private bool isPending;
        private bool isFallbackActive;
        private bool captureFailed;
        private GUIStyle statusStyle;

        public bool IsSupported => isSupported;

        public bool IsLocked => isLocked;

        public bool IsPending => isPending;

        public bool IsFallbackActive => isFallbackActive;

        public bool CaptureFailed => captureFailed;

        public InputSystemLogicalInputAdapter LogicalInput => logicalInput;

        private void Awake()
        {
            if (logicalInput == null)
            {
                logicalInput = FindFirstObjectByType<InputSystemLogicalInputAdapter>();
            }
            logicalInput?.SetPointerCaptureRequired(true);
        }

        public void Configure(InputSystemLogicalInputAdapter input)
        {
            logicalInput?.SetPointerCaptureRequired(false);
            logicalInput = input;
            logicalInput?.SetPointerCaptureRequired(isActiveAndEnabled);
        }

        private void OnEnable()
        {
            logicalInput?.SetPointerCaptureRequired(true);
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.stickyCursorLock = false;
            isSupported = BNPointerLock_Install() != 0;
            BNPointerLock_SetEnabled(1);
            RefreshPointerState();
#else
            isSupported = true;
            RefreshDesktopPointerState();
#endif
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ReleasePointer();
                return;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            RefreshPointerState();
#else
            RefreshDesktopPointerState();
            if (!isLocked &&
                Application.isFocused &&
                Mouse.current != null &&
                Mouse.current.leftButton.wasPressedThisFrame)
            {
                RequestPointerCapture();
            }
#endif
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                ReleasePointer();
            }
        }

        private void OnDisable()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            BNPointerLock_SetEnabled(0);
#endif
            ReleasePointer();
            logicalInput?.SetPointerCaptureRequired(false);
        }

        public void RequestPointerCapture()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // A browser pointer-lock request must originate from the trusted
            // canvas click handled by the WebGL bridge.
            RefreshPointerState();
#else
            isSupported = true;
            isPending = false;
            isFallbackActive = false;
            captureFailed = false;
            Cursor.lockState = CursorLockMode.Locked;
            ApplyPointerState(
                Cursor.lockState == CursorLockMode.Locked,
                false,
                false,
                Vector2.zero);
#endif
        }

        public void ReleasePointerCapture()
        {
            ReleasePointer();
        }

        public static string BuildStatusText(
            bool supported,
            bool locked,
            bool failed = false,
            bool pending = false,
            bool fallbackActive = false)
        {
            if (!supported)
            {
                return "POINTER LOCK UNAVAILABLE";
            }

            if (locked)
            {
                return "MOUSE CAPTURED - ESC TO RELEASE";
            }

            if (fallbackActive)
            {
                return "MOUSE LOOK ACTIVE - ESC TO RELEASE";
            }

            if (pending)
            {
                return "CAPTURING MOUSE...";
            }

            if (failed)
            {
                return "MOUSE CAPTURE BLOCKED - CLICK GAME VIEW TO RETRY";
            }

            return "CLICK GAME VIEW TO CAPTURE MOUSE";
        }

        private void OnGUI()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            EnsureStyle();
            GUI.Label(
                new Rect(12f, 12f, Mathf.Min(430f, Screen.width - 24f), 34f),
                BuildStatusText(
                    isSupported,
                    isLocked,
                    captureFailed,
                    isPending,
                    isFallbackActive),
                statusStyle);
#endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private void RefreshPointerState()
        {
            isSupported = BNPointerLock_IsSupported() != 0;
            isPending = BNPointerLock_IsPending() != 0;
            captureFailed = BNPointerLock_HasError() != 0;
            bool fallbackActive = BNPointerLock_IsFallbackActive() != 0;
            Vector2 fallbackLookDelta = fallbackActive
                ? new Vector2(
                    BNPointerLock_ConsumeFallbackDeltaX(),
                    BNPointerLock_ConsumeFallbackDeltaY())
                : Vector2.zero;
            ApplyPointerState(
                isSupported && BNPointerLock_IsLocked() != 0,
                isPending,
                fallbackActive,
                fallbackLookDelta);
        }
#else
        private void RefreshDesktopPointerState()
        {
            isSupported = true;
            isPending = false;
            isFallbackActive = false;
            captureFailed = false;
            ApplyPointerState(
                Cursor.lockState == CursorLockMode.Locked,
                false,
                false,
                Vector2.zero);
        }
#endif

        private void ReleasePointer()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            BNPointerLock_Exit();
#endif
            isPending = false;
            isFallbackActive = false;
            captureFailed = false;
            ApplyPointerState(false, false, false, Vector2.zero);
        }

        private void ApplyPointerState(
            bool locked,
            bool pending,
            bool fallbackActive,
            Vector2 fallbackLookDelta)
        {
            isLocked = locked;
            isFallbackActive = !locked && fallbackActive;
            logicalInput?.SetPointerState(
                isLocked,
                isFallbackActive,
                fallbackLookDelta);
            CursorLockMode desiredLockState = locked
                ? CursorLockMode.Locked
                : CursorLockMode.None;

            // Do not write None while the DOM request is still pending. That
            // creates a second Unity-side pointer-lock transition which can
            // cancel the browser request before pointerlockchange arrives.
            if (!pending && Cursor.lockState != desiredLockState)
            {
                Cursor.lockState = desiredLockState;
            }

            bool shouldBeVisible = !locked && !isFallbackActive;
            if (Cursor.visible != shouldBeVisible)
            {
                Cursor.visible = shouldBeVisible;
            }
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
        private static extern int BNPointerLock_IsPending();

        [DllImport("__Internal")]
        private static extern int BNPointerLock_IsFallbackActive();

        [DllImport("__Internal")]
        private static extern int BNPointerLock_ConsumeFallbackDeltaX();

        [DllImport("__Internal")]
        private static extern int BNPointerLock_ConsumeFallbackDeltaY();

        [DllImport("__Internal")]
        private static extern int BNPointerLock_HasError();

        [DllImport("__Internal")]
        private static extern void BNPointerLock_Exit();
#endif
    }
}
