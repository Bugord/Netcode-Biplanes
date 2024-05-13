using System;
using System.Collections;
using Core;
using Plane;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEngine;

namespace Pilot
{
    public class NetworkedPilotController : NetcodeHooks
    {
        public event Action<PlaneDiedReason> Died;
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

        public void KillPlayerWithReason(PlaneDiedReason diedReason)
        {
            PlayKillVisualsRpc();
            KillPlayerRpc(diedReason);
        }

        public void RespawnPlayer()
        {
            RespawnPlayerRpc();
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
        private void RespawnPlayerRpc()
        {
            EnteredRespawnArea?.Invoke();
            NetworkObject.Despawn();
        }

        [Rpc(SendTo.Server)]
        private void KillPlayerRpc(PlaneDiedReason diedReason)
        {
            Died?.Invoke(diedReason);
            DespawnWithDelay();
        }

        [Rpc(SendTo.Owner)]
        private void PlayKillVisualsRpc()
        {
            clientPilotController.KillPlayer();
        }
        
        private IEnumerator DespawnWithDelayRoutine()
        {
            yield return new WaitForSeconds(3);
            NetworkObject.Despawn();
        }
    }
}