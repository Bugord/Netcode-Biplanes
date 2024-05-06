using UnityEngine;

namespace Core
{
    public class PlaneParticles : MonoBehaviour
    {
        [SerializeField]
        private Health health;

        [SerializeField]
        private GameObject smokeParticles;

        [SerializeField]
        private GameObject fireParticles;

        [SerializeField]
        private GameObject explosion;

        private void OnEnable()
        {
            health.HealthChanged += OnHealthChanged;
        }

        private void OnDisable()
        {
            health.HealthChanged -= OnHealthChanged;
        }

        private void OnHealthChanged(int currentHealth)
        {
            smokeParticles.SetActive(currentHealth == 2);
            fireParticles.SetActive(currentHealth == 1);
            explosion.SetActive(currentHealth == 0);
        }
    }
}