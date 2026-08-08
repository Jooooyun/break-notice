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
        private InputAction lookAction;
        private InputAction aimAction;
        private InputAction interactPrimaryAction;
        private InputAction throwAction;
        private InputAction callAllyAction;
        private InputAction strapReleaseAction;
        private InputAction resetEncounterAction;

        public LogicalInputSnapshot Snapshot { get; private set; }

        public InputActionAsset InputActions => inputActions;

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
            lookAction = actionMap.FindAction(nameof(PrototypeLogicalAction.Look), true);
            aimAction = actionMap.FindAction(nameof(PrototypeLogicalAction.Aim), true);
            interactPrimaryAction = actionMap.FindAction(nameof(PrototypeLogicalAction.InteractPrimary), true);
            throwAction = actionMap.FindAction(nameof(PrototypeLogicalAction.Throw), true);
            callAllyAction = actionMap.FindAction(nameof(PrototypeLogicalAction.CallAlly), true);
            strapReleaseAction = actionMap.FindAction(nameof(PrototypeLogicalAction.StrapRelease), true);
            resetEncounterAction = actionMap.FindAction(nameof(PrototypeLogicalAction.ResetEncounter), true);
        }

        private void CaptureSnapshot()
        {
            if (actionMap == null || !actionMap.enabled)
            {
                Snapshot = default;
                return;
            }

            Snapshot = new LogicalInputSnapshot(
                moveAction.ReadValue<Vector2>(),
                lookAction.ReadValue<Vector2>(),
                aimAction.IsPressed(),
                interactPrimaryAction.WasPressedThisFrame(),
                throwAction.WasPressedThisFrame(),
                callAllyAction.WasPressedThisFrame(),
                strapReleaseAction.WasPressedThisFrame(),
                resetEncounterAction.WasPressedThisFrame());
        }
    }
}
