using System;
using Pilot;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

namespace Plane
{
    public class PlanePilotEjectController : NetworkBehaviour
    {
        public event Action PilotEjected;

        [SerializeField]
        private NetworkedPlaneController networkedPlaneController;

        [SerializeField]
        private NetworkedPilotController pilotPrefab;

        private bool isEjectionDisabled = true;
        private NetworkedPilotController pilot;
        public bool WasPilotEjected { get; private set; }

        private void Update()
        {
            if (!IsOwner) {
                return;
            }

            if (isEjectionDisabled || WasPilotEjected) {
                return;
            }

            if (Input.GetKeyDown(KeyCode.E)) {
                EjectPilot();
            }
        }

        public void Reset()
        {
            WasPilotEjected = false;
        }

        public void DisableEjection()
        {
            isEjectionDisabled = true;
        }

        public void EnableEjection()
        {
            isEjectionDisabled = false;
        }

        private void EjectPilot()
        {
            SpawnPilotRpc(transform.position, transform.up);
            PilotEjected?.Invoke();
            WasPilotEjected = true;
        }

        [Rpc(SendTo.Server)]
        private void SpawnPilotRpc(Vector2 position, Vector2 ejectDirection)
        {
            WasPilotEjected = true;

            pilot = Instantiate(pilotPrefab, position, quaternion.identity);
            pilot.GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
            pilot.Init(networkedPlaneController.EdgeDistance);

            pilot.Died += OnPilotDied;
            pilot.EnteredRespawnArea += OnPilotEnteredRespawnArea;

            pilot.Eject(ejectDirection);
        }

        private void OnPilotEnteredRespawnArea()
        {
            pilot.Died -= OnPilotDied;
            pilot.EnteredRespawnArea -= OnPilotEnteredRespawnArea;

            networkedPlaneController.OnPilotEnteredRespawnArea();
        }

        private void OnPilotDied(PlaneDiedReason reason)
        {
            pilot.Died -= OnPilotDied;
            pilot.EnteredRespawnArea -= OnPilotEnteredRespawnArea;
            networkedPlaneController.OnPilotDied(reason);
        }
    }
}