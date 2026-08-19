using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BreakNotice.Gameplay.Input
{
    [DefaultExecutionOrder(-300)]
    [DisallowMultipleComponent]
    public sealed class InputSystemLogicalInputAdapter : MonoBehaviour
    {
        private const string ActionMapName = "PrototypePlayer";

        [SerializeField]
        private InputActionAsset inputActions;

        private InputActionMap actionMap;
        private InputAction moveAction;
        private InputAction sprintAction;
        private InputAction jumpAction;
        private InputAction lookAction;
        private InputAction aimAction;
        private InputAction interactPrimaryAction;
        private InputAction throwAction;
        private InputAction selectFistsAction;
        private InputAction selectObjectWeaponAction;
        private InputAction allyTaskAction;
        private InputAction callAllyAction;
        private InputAction regroupAllyAction;
        private InputAction strapReleaseAction;
        private InputAction resetEncounterAction;
        private InputAction restartAction;
        private bool pointerCaptureRequired;
        private int pointerCapturedFrame = -1;

        public LogicalInputSnapshot Snapshot { get; private set; }

        public InputActionAsset InputActions => inputActions;

        public bool IsWebPointerLocked { get; private set; }

        public bool IsWebPointerFallbackActive { get; private set; }

        public bool IsWebPointerGameplayActive =>
            IsWebPointerLocked || IsWebPointerFallbackActive;

        public bool IsPointerLocked => IsWebPointerLocked;

        public bool IsPointerFallbackActive => IsWebPointerFallbackActive;

        public bool IsPointerGameplayActive => IsWebPointerGameplayActive;

        public bool IsPointerCaptureRequired => pointerCaptureRequired;

        public bool PointerJustCapturedThisFrame =>
            pointerCaptureRequired && pointerCapturedFrame == Time.frameCount;

        public bool IsPointerInputAllowed =>
            !pointerCaptureRequired ||
            (IsPointerGameplayActive && !PointerJustCapturedThisFrame);

        public Vector2 WebPointerFallbackLookDelta { get; private set; }

        private void Awake()
        {
            BindActions();
        }

        private void OnEnable()
        {
            if (actionMap == null)
            {
                BindActions();
            }

            actionMap.Enable();
            CaptureSnapshot();
        }

        private void OnDisable()
        {
            actionMap?.Disable();
            Snapshot = default;
            ClearWebPointerState();
        }

        private void Update()
        {
            CaptureSnapshot();
        }

        private void BindActions()
        {
            if (inputActions == null)
            {
                throw new InvalidOperationException("The approved Input Actions asset is required.");
            }

            actionMap = inputActions.FindActionMap(ActionMapName, true);
            moveAction = actionMap.FindAction(nameof(PrototypeLogicalAction.Move), true);
            sprintAction = actionMap.FindAction(nameof(PrototypeLogicalAction.Sprint), true);
            jumpAction = actionMap.FindAction(nameof(PrototypeLogicalAction.Jump), true);
            lookAction = actionMap.FindAction(nameof(PrototypeLogicalAction.Look), true);
            aimAction = actionMap.FindAction(nameof(PrototypeLogicalAction.Aim), true);
            interactPrimaryAction = actionMap.FindAction(nameof(PrototypeLogicalAction.InteractPrimary), true);
            throwAction = actionMap.FindAction(nameof(PrototypeLogicalAction.Throw), true);
            selectFistsAction = actionMap.FindAction(nameof(PrototypeLogicalAction.SelectFists), true);
            selectObjectWeaponAction = actionMap.FindAction(nameof(PrototypeLogicalAction.SelectObjectWeapon), true);
            allyTaskAction = actionMap.FindAction(nameof(PrototypeLogicalAction.AllyTask), true);
            callAllyAction = actionMap.FindAction(nameof(PrototypeLogicalAction.CallAlly), true);
            regroupAllyAction = actionMap.FindAction(nameof(PrototypeLogicalAction.RegroupAlly), true);
            strapReleaseAction = actionMap.FindAction(nameof(PrototypeLogicalAction.StrapRelease), true);
            resetEncounterAction = actionMap.FindAction(nameof(PrototypeLogicalAction.ResetEncounter), true);
            restartAction = actionMap.FindAction(nameof(PrototypeLogicalAction.Restart), true);
        }

        private void CaptureSnapshot()
        {
            if (actionMap == null || !actionMap.enabled)
            {
                Snapshot = default;
                return;
            }

            bool debugResetRequested = false;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            debugResetRequested = resetEncounterAction.WasPressedThisFrame();
#endif

            bool pointerInputAllowed = IsPointerInputAllowed;
            Snapshot = new LogicalInputSnapshot(
                moveAction.ReadValue<Vector2>(),
                pointerInputAllowed ? lookAction.ReadValue<Vector2>() : Vector2.zero,
                pointerInputAllowed && aimAction.IsPressed(),
                interactPrimaryAction.WasPressedThisFrame(),
                pointerInputAllowed && throwAction.WasPressedThisFrame(),
                callAllyAction.WasPressedThisFrame(),
                strapReleaseAction.WasPressedThisFrame(),
                debugResetRequested,
                pointerInputAllowed && throwAction.IsPressed(),
                pointerInputAllowed && throwAction.WasReleasedThisFrame(),
                allyTaskAction.WasPressedThisFrame(),
                regroupAllyAction.WasPressedThisFrame(),
                sprintAction.IsPressed(),
                jumpAction.WasPressedThisFrame(),
                restartAction.WasPressedThisFrame(),
                selectFistsAction.WasPressedThisFrame(),
                selectObjectWeaponAction.WasPressedThisFrame(),
                interactPrimaryAction.IsPressed(),
                interactPrimaryAction.WasReleasedThisFrame());
        }

        public void SetWebPointerState(
            bool locked,
            bool fallbackActive,
            Vector2 fallbackLookDelta)
        {
            SetPointerState(locked, fallbackActive, fallbackLookDelta);
        }

        public void SetPointerCaptureRequired(bool required)
        {
            pointerCaptureRequired = required;
            if (!pointerCaptureRequired)
            {
                ClearWebPointerState();
            }
        }

        public void SetPointerState(
            bool locked,
            bool fallbackActive,
            Vector2 fallbackLookDelta)
        {
            bool wasGameplayActive = IsPointerGameplayActive;
            IsWebPointerLocked = locked;
            IsWebPointerFallbackActive = !locked && fallbackActive;
            WebPointerFallbackLookDelta = IsWebPointerFallbackActive
                ? fallbackLookDelta
                : Vector2.zero;
            if (!wasGameplayActive && IsPointerGameplayActive)
            {
                pointerCapturedFrame = Time.frameCount;
            }
            else if (!IsPointerGameplayActive)
            {
                pointerCapturedFrame = -1;
            }
        }

        public void ClearWebPointerState()
        {
            SetWebPointerState(false, false, Vector2.zero);
        }
    }
}
