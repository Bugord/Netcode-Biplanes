using System;
using Unity.Multiplayer.Samples.Utilities.ClientAuthority;
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
        
        private void Awake()
        {
            networkedPlaneController.OnNetworkSpawnHook += OnNetworkSpawn;
            health.Died += OnPlaneCrashed;
            planeMovement.TookOff += OnPlaneTookOff;
        }

        private void OnDestroy()
        {
            networkedPlaneController.OnNetworkSpawnHook -= OnNetworkSpawn;
            health.Died -= OnPlaneCrashed;
            planeMovement.TookOff -= OnPlaneTookOff;
        }

        public void Respawn()
        {
            planeMovement.Teleport(networkedPlaneController.SpawnPosition);
            planeSpriteController.SetDefaultSprite();
            planeMovement.EnableMovement();
        }
        
        private void OnPlaneCrashed()
        {
            planeSpriteController.SetDestroyedSprite();
            planeMovement.DisableMovement();
        }

        private void OnPlaneTookOff()
        {
            planeSpriteController.SetFlySprite();
        }

        private void OnNetworkSpawn()
        {
            var isMirrored = networkedPlaneController.Team == Team.Red;
            planeRotation.Init(isMirrored);
            planeSpriteController.Init(networkedPlaneController.Team);

            planeSpriteController.SetDefaultSprite();
        }
    }
}