using System;
using System.Collections;
using Core;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEngine;

namespace Pilot
{
    public class NetworkedPilotController : NetcodeHooks
    {
        public event Action<PlaneCrashReason> Died;
        public event Action EnteredRespawnArea;

        [SerializeField]
        private PilotMovement pilotMovement;

        [SerializeField]
        private ClientPilotController clientPilotController;
        
        public void Init(float edgeDistance)
        {
            InitRpc(edgeDistance);
        }

        public void Eject(Vector2 ejectDirection)
        {
            JumpRpc(ejectDirection);
        }

        [Rpc(SendTo.Server)]
        public void OnPilotDiedRpc(PlaneCrashReason crashReason)
        {
            Debug.Log($"[{nameof(NetworkedPilotController)}] (ServerRpc) Pilot died {crashReason}");
            Died?.Invoke(crashReason);
            DespawnWithDelay();
        }

        public void OnPilotEnterRespawnArea()
        {
            OnPilotEnterSafeAreaRpc();
        }

        public void OnPilotShot()
        {
            Debug.Log($"[{nameof(NetworkedPilotController)}] Pilot shot");
            OnPilotDiedRpc(PlaneCrashReason.PilotShot);
            OnPilotDeadRpc();
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void OnPilotDeadRpc()
        {
            clientPilotController.OnPilotDead();
        }
        
        [Rpc(SendTo.Owner)]
        private void InitRpc(float edgeDistance)
        {
            pilotMovement.Init(edgeDistance);
        }

        [Rpc(SendTo.Owner)]
        private void JumpRpc(Vector2 ejectDirection)
        {
            pilotMovement.Jump(ejectDirection);
        }

        private void DespawnWithDelay()
        {
            StartCoroutine(DespawnWithDelayRoutine());
        }

        [Rpc(SendTo.Server)]
        private void OnPilotEnterSafeAreaRpc()
        {
            EnteredRespawnArea?.Invoke();
            NetworkObject.Despawn();
        }
        
        private IEnumerator DespawnWithDelayRoutine()
        {
            yield return new WaitForSeconds(3);
            NetworkObject.Despawn();
        }
    }
}