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
        private Slider _healthbar;

        protected virtual void Start()
        {
            currentHealth = maxHealth;
            _healthbar = GetComponentInChildren<Slider>();
        }

        public virtual void TakeDamage(float ammount, Vector3 hitPoint = default)
        {
            currentHealth = Mathf.Clamp(currentHealth -= ammount, 0, maxHealth);
            if (currentHealth <= 0)
            {
                IsDead();
            }

            if (_healthbar)
            {
                _healthbar.value = (float) currentHealth / maxHealth;
            }
        }

        protected virtual void IsDead()
        {
            Destroy(gameObject);
        }
    }
}
