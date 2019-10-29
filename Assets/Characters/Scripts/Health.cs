using UnityEngine;
using UnityEngine.UI;

namespace Characters.Scripts
{
    public class Health : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        public int maxHealth = 100;
        public float currentHealth;
        public bool healthBar = true;
        public Slider healthbar;

        protected virtual void Start()
        {
            currentHealth = maxHealth;
        }

        public virtual void TakeDamage(float ammount, Vector3 hitPoint = default)
        {
            currentHealth = Mathf.Clamp(currentHealth -= ammount, 0, maxHealth);
            if (currentHealth <= 0)
            {
                Kill();
            }

            if (healthbar)
            {
                healthbar.value = currentHealth / maxHealth;
            }
        }

        protected virtual void Kill()
        {
            Destroy(gameObject);
        }
    }
}
