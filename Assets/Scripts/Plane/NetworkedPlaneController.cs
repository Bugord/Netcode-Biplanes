using System;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEngine;

namespace Core
{
    public class NetworkedPlaneController : NetcodeHooks
    {
        public event Action<NetworkedPlaneController, PlaneDiedReason> Crashed;
        public event Action<NetworkedPlaneController> EnteredRespawnArea; 

        [SerializeField]
        private Health health;
        
        [SerializeField]
        private PlanePilotEjectController planePilotEjectController;

        [SerializeField]
        private ClientPlaneController clientPlaneController;

        private readonly NetworkVariable<Team> team = new NetworkVariable<Team>();

        private readonly NetworkVariable<float> edgeDistance = new NetworkVariable<float>();

        private readonly NetworkVariable<Vector3> spawnPosition = new NetworkVariable<Vector3>();

        private readonly Vector3 mirroredPlayerRotation = new Vector3(0, 180, 0);

        public Team Team => team.Value;
        public float EdgeDistance => edgeDistance.Value;
        public Vector3 SpawnPosition => spawnPosition.Value;

        public void Init(Team team, Vector3 spawnPosition, float edgeDistance)
        {
            this.team.Value = team;
            this.spawnPosition.Value = spawnPosition;
            this.edgeDistance.Value = edgeDistance;
        }

        private void OnEnable()
        {
            health.HealthEmpty += OnPlaneHealthEmpty;
        }

        private void OnDisable()
        {
            health.HealthEmpty -= OnPlaneHealthEmpty;
        }

        public void OnPlaneCrashed()
        {
            OnPlaneCrashedRpc();
        }

        public void OnPilotDied(PlaneDiedReason diedReason)
        {
            Debug.Log($"[{nameof(NetworkedPlaneController)}] Pilot died: {Team} {diedReason}");
            OnPilotDiedRpc(diedReason);
        }

        public void OnPilotEnteredRespawnArea()
        {
            Debug.Log($"[{nameof(NetworkedPlaneController)}] Pilot enter respawn area: {Team}");
            EnteredRespawnArea?.Invoke(this);
        }

        private void OnPlaneHealthEmpty()
        {
            Crashed?.Invoke(this, PlaneDiedReason.PlaneDestroyed);
        }

        [Rpc(SendTo.Server)]
        private void OnPlaneCrashedRpc()
        {
            if (planePilotEjectController.WasPilotEjected) {
                Debug.Log($"[{nameof(NetworkedPlaneController)}] (ServerRpc) Plane crashed: {Team} but pilot is alive");
                return;
            }
            
            Debug.Log($"[{nameof(NetworkedPlaneController)}] (ServerRpc) Plane crashed: {Team} {PlaneDiedReason.Suicide}");
            Crashed?.Invoke(this, PlaneDiedReason.Suicide);
        }
        
        [Rpc(SendTo.Server)]
        public void OnPilotDiedRpc(PlaneDiedReason diedReason)
        {
            Debug.Log($"[{nameof(NetworkedPlaneController)}] (ServerRpc) Pilot died: {Team} {diedReason}");
            Crashed?.Invoke(this, diedReason);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            SetDefaultRotation();
        }

        public void Respawn()
        {
            health.Reset();
            planePilotEjectController.Reset();
            RepawnRpc();
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void PlayCrashedGraphicsRpc()
        {
            clientPlaneController.PlayCrashedGraphics();
        }

        [Rpc(SendTo.Owner)]
        private void RepawnRpc()
        {
            clientPlaneController.Respawn();
        }

        private void SetDefaultRotation()
        {
            if (team.Value == Team.Red) {
                transform.rotation = Quaternion.Euler(mirroredPlayerRotation);
            }
        }
    }
}