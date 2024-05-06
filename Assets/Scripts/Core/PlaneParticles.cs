using UnityEngine;
using UnityEngine.Serialization;

namespace Core
{
    public class PlaneParticles : MonoBehaviour
    {
        [SerializeField]
        private ParticleSystem smokeParticles;

        [SerializeField]
        private ParticleSystem fireParticles;

        [SerializeField]
        private Animator explosionAnimator;

        public void Reset()
        {
            smokeParticles.Stop();
            fireParticles.Stop();
        }

        public void DisableDamageEffects()
        {
            smokeParticles.Stop();
            fireParticles.Stop();
        }

        public void PlayExplosion()
        {
            explosionAnimator.Play("explosion_animation");
        }

        public void SmokeSetActive(bool isActive)
        {
            if (isActive) {
                smokeParticles.Play();
            }
            else {
                smokeParticles.Stop();
            }
        }

        public void FireSetActive(bool isActive)
        {
            if (isActive) {
                fireParticles.Play();
            }
            else {
                fireParticles.Stop();
            }
        }
    }
}