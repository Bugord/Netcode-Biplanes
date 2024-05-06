using System;
using Unity.Mathematics;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Core
{
    public class NetworkedPlaneController : NetcodeHooks
    {
        public event Action<NetworkedPlaneController, PlaneCrashReason> Crashed;

        [SerializeField]
        private Health health;

        [SerializeField]
        private PlaneSpriteController planeSpriteController;

        [SerializeField]
        private NetworkTransform networkTransform;
        
        private readonly NetworkVariable<Team> team = new NetworkVariable<Team>();

        private readonly Vector3 mirroredPlayerRotation = new Vector3(0, 180, 0);

        private Vector3 spawnPosition;
        
        public Team Team => team.Value;

        public void Init(Team team, Vector3 spawnPosition)
        {
            this.team.Value = team;
            this.spawnPosition = spawnPosition;
        }

        private void OnEnable()
        {
            health.HealthChanged += OnHealthChanged;
        }

        private void OnDisable()
        {
            health.HealthChanged -= OnHealthChanged;
        }

        private void OnHealthChanged(int health)
        {
            if (health == 0) {
                Crashed?.Invoke(this, PlaneCrashReason.Destroyed);
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            SetDefaultRotation();
        }

        public void Respawn()
        {
            health.Reset();
            planeSpriteController.SetDefaultSpriteRpc();
            MoveToPositionRpc(spawnPosition);
        }

        [Rpc(SendTo.Owner)]
        private void MoveToPositionRpc(Vector3 position)
        {
            networkTransform.Teleport(position, quaternion.identity, Vector3.one);
            SetDefaultRotation();
        }

        private void SetDefaultRotation()
        {
            if (team.Value == Team.Red) {
                transform.rotation = Quaternion.Euler(mirroredPlayerRotation);
            }
        }
    }
}