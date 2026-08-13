using UnityEngine;

namespace BreakNotice.Presentation
{
    [DisallowMultipleComponent]
    public sealed class ShowcaseWorldHealthAnchor : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)]
        private float normalizedHealth = 1f;

        [SerializeField]
        private bool visible = true;

        public float NormalizedHealth => normalizedHealth;

        public bool Visible => visible;

        public void SetNormalizedHealth(float value)
        {
            normalizedHealth = Mathf.Clamp01(value);
        }

        public void SetHealth(float current, float maximum)
        {
            SetNormalizedHealth(maximum > 0f ? current / maximum : 0f);
        }

        public void SetVisible(bool value)
        {
            visible = value;
        }

        private void OnValidate()
        {
            normalizedHealth = Mathf.Clamp01(normalizedHealth);
        }
    }
}
