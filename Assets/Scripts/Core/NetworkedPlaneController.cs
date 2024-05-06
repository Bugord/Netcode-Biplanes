﻿using System;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEngine;

namespace Core
{
    public class NetworkedPlaneController : NetcodeHooks
    {
        public event Action<NetworkedPlaneController, PlaneCrashReason> Crashed;

        [SerializeField]
        private Health health;

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

        private void OnPlaneHealthEmpty()
        {
            Crashed?.Invoke(this, PlaneCrashReason.Destroyed);
        }
        
        [Rpc(SendTo.Server)]
        private void OnPlaneCrashedRpc()
        {
            Crashed?.Invoke(this, PlaneCrashReason.Suicide);
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            SetDefaultRotation();
        }

        public void Respawn()
        {
            health.Reset();
            RepawnRpc();
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