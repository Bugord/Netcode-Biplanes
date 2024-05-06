using UnityEngine;

namespace Core
{
    public class PlaneParticles : MonoBehaviour
    {
        [SerializeField]
        private GameObject smokeParticles;

        [SerializeField]
        private GameObject fireParticles;

        [SerializeField]
        private GameObject explosion;

        public void Reset()
        {
            smokeParticles.SetActive(false);
            fireParticles.SetActive(false);
            explosion.SetActive(false);
        }

        public void DisableDamageEffects()
        {
            smokeParticles.SetActive(false);
            fireParticles.SetActive(false);
        }

        public void PlayExplosion()
        {
            explosion.SetActive(true);
        }

        public void SmokeSetActive(bool isActive)
        {
            smokeParticles.SetActive(isActive);
        }

        public void FireSetActive(bool isActive)
        {
            smokeParticles.SetActive(isActive);
        }
    }
}