using UnityEngine;

namespace Characters.Scripts
{
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class Sword : MonoBehaviour
    {
        public float damage = 30;

        private void Reset()
        {
            SetRigidbody();
        }

        private void OnValidate()
        {
            SetRigidbody();
        }

        private void SetRigidbody()
        {
            var rb = GetComponent<Rigidbody>();
            rb.angularDrag = Mathf.Infinity;
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.None;
            rb.constraints = RigidbodyConstraints.FreezeAll;
            GetComponent<Rigidbody>().hideFlags = HideFlags.HideInInspector;
        }

        private void Start()
        {
            GetComponent<Collider>().enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other && other.transform != GetComponentInParent<Health>().transform)
            {
                other.GetComponent<IDamageable>()?.TakeDamage(damage, transform.GetComponentInParent<Humanoid>().transform.position);
            }
        }
    }
}
