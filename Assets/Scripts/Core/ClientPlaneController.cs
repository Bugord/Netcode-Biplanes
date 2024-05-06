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

        private void OnPlaneCrashed()
        {
            planeSpriteController.SetDestroyedSpriteRpc();
            planeMovement.DisableMovement();
        }
        
        private void OnPlaneTookOff()
        {
            planeSpriteController.SetFlySpriteRpc();
        }

        private void OnNetworkSpawn()
        {
            var isMirrored = networkedPlaneController.Team == Team.Red;
            planeRotation.Init(isMirrored);
            planeSpriteController.Init(networkedPlaneController.Team);
            
            planeSpriteController.SetDefaultSpriteRpc();
        }
    }
}