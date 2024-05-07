using System;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
using Unity.Netcode;
using UnityEngine;

namespace Core
{
    [RequireComponent(typeof(NetworkedPlaneController))]
    public class ClientPlaneController : MonoBehaviour
    {
        [SerializeField]
        private NetworkedPlaneController networkedPlaneController;

        [SerializeField]
        private PlaneRotation planeRotation;

        [SerializeField]
        private PlaneSpriteController planeSpriteController;

        [SerializeField]
        private Health health;

        [SerializeField]
        private PlaneMovement planeMovement;

        [SerializeField]
        private PlaneParticles planeParticles;

        [SerializeField]
        private PlaneWeapon planeWeapon;
        
        private void Awake()
        {
            networkedPlaneController.OnNetworkSpawnHook += OnNetworkSpawn;
            health.HealthEmpty += OnPlaneCrashed;
            health.HealthChanged += OnHealthChanged;
            planeMovement.EngineStarted += OnPlaneEngineStarted;
        }

        private void OnDestroy()
        {
            networkedPlaneController.OnNetworkSpawnHook -= OnNetworkSpawn;
            health.HealthEmpty -= OnPlaneCrashed;
            health.HealthChanged -= OnHealthChanged;
            planeMovement.EngineStarted -= OnPlaneEngineStarted;
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (!networkedPlaneController.IsOwner) {
                return;
            }
            
            if (!planeMovement.WasEngineStarted) {
                return;
            }
            
            if (col.gameObject.CompareTag("Ground") && planeMovement.DidTookOff) {
                networkedPlaneController.OnPlaneCrashed();
                OnPlaneCrashed();
            }

            if (col.gameObject.CompareTag("House")) {
                networkedPlaneController.OnPlaneCrashed();
                OnPlaneCrashed();
            }
        }

        public void Respawn()
        {
            planeMovement.Reset();
            planeMovement.Teleport(networkedPlaneController.SpawnPosition);
            planeSpriteController.SetDefaultSprite();
            planeMovement.EnableMovement();
            planeParticles.Reset();
            planeRotation.Reset();
            planeWeapon.enabled = true;
        }
        
        public void OnPlaneCrashed()
        {
            networkedPlaneController.PlayCrashedGraphicsRpc();   
            planeMovement.DisableMovement();
            planeRotation.DisableRotation();
            planeWeapon.enabled = false;
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void PlayCrashedGraphics()
        {
            planeParticles.DisableDamageEffects();
            planeParticles.PlayExplosion();
            planeSpriteController.SetDestroyedSprite();
        }

        private void OnPlaneEngineStarted()
        {
            planeSpriteController.SetFlySprite();
            planeRotation.EnableRotation();
        }

        private void OnNetworkSpawn()
        {
            var isMirrored = networkedPlaneController.Team == Team.Red;
            planeRotation.Init(isMirrored);
            planeSpriteController.Init(networkedPlaneController.Team);

            planeSpriteController.SetDefaultSprite();
        }

        private void OnHealthChanged(int health)
        {
            Debug.Log($"{networkedPlaneController.Team} health changed {health}");
            switch (health) {
                case 2: 
                    planeParticles.SmokeSetActive(true);
                    break;
                case 1:
                    planeParticles.SmokeSetActive(false);
                    planeParticles.FireSetActive(true);
                    break;
            }
        }
    }
}