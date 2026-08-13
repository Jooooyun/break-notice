using UnityEngine;

namespace BreakNotice.Gameplay.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public sealed class ShowcaseDrawerObjectiveReceiver : MonoBehaviour
    {
        [SerializeField]
        private ShowcaseObjectiveController objectiveController;

        [SerializeField]
        private ShowcaseCargoStrapController cargoStrapController;

        public int AcceptedImpactCount { get; private set; }

        public ShowcaseDrawerSolution LastAcceptedSolution { get; private set; }

        public void Configure(
            ShowcaseObjectiveController objective,
            ShowcaseCargoStrapController cargoStrap)
        {
            objectiveController = objective;
            cargoStrapController = cargoStrap;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision == null || collision.collider == null ||
                objectiveController == null)
            {
                return;
            }

            PrototypeInteractionTarget physicalObject =
                collision.collider.GetComponentInParent<PrototypeInteractionTarget>();
            ShowcaseRunnerAgent runner =
                collision.collider.GetComponentInParent<ShowcaseRunnerAgent>();

            ShowcaseDrawerSolution solution = ShowcaseDrawerSolution.None;
            Transform sourceTransform = null;
            if (physicalObject != null &&
                physicalObject.WeightClass == PrototypeWeightClass.Medium &&
                (physicalObject.Role == PrototypeInteractionRole.DirectChair ||
                 physicalObject.Role == PrototypeInteractionRole.AllyChair))
            {
                solution = cargoStrapController != null &&
                    cargoStrapController.WasRedirected(physicalObject)
                    ? ShowcaseDrawerSolution.CargoStrapRedirect
                    : ShowcaseDrawerSolution.DirectChairThrow;
                sourceTransform = physicalObject.transform;
            }
            else if (runner != null)
            {
                solution = ShowcaseDrawerSolution.RunnerCausalCollision;
                sourceTransform = runner.transform;
            }

            if (solution == ShowcaseDrawerSolution.None || sourceTransform == null)
            {
                return;
            }

            Vector3 direction = Vector3.ProjectOnPlane(
                transform.position - sourceTransform.position,
                Vector3.up);
            if (objectiveController.TryRegisterDrawerImpact(
                    solution,
                    direction,
                    collision.relativeVelocity.magnitude))
            {
                LastAcceptedSolution = solution;
                AcceptedImpactCount++;
            }
        }
    }
}
