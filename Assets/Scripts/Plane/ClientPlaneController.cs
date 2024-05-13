﻿using System;
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
        private PlanePilotEjectController planePilotEjectController;

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
            planeMovement.TookOff += OnPlaneTookOff;
            planePilotEjectController.PilotEjected += OnPilotEjected;
        }

        private void OnDestroy()
        {
            networkedPlaneController.OnNetworkSpawnHook -= OnNetworkSpawn;
            health.HealthEmpty -= OnPlaneCrashed;
            health.HealthChanged -= OnHealthChanged;
            planeMovement.EngineStarted -= OnPlaneEngineStarted;
            planeMovement.TookOff -= OnPlaneTookOff;
            planePilotEjectController.PilotEjected -= OnPilotEjected;
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

        private void OnNetworkSpawn()
        {
            var isMirrored = networkedPlaneController.Team == Team.Red;
            planeRotation.Init(isMirrored);
            planeSpriteController.Init(networkedPlaneController.Team);

            if (networkedPlaneController.IsOwner) {
                Respawn();
            }
        }

        public void Respawn()
        {
            planePilotEjectController.Reset();
            planePilotEjectController.DisableEjection();

            planeMovement.Reset();
            planeMovement.Teleport(networkedPlaneController.SpawnPosition);
            planeMovement.EnableMovement();
            planeMovement.EnableInput();

            planeSpriteController.SetDefaultSprite();

            planeParticles.Reset();
            planeRotation.Reset();
            planeWeapon.enabled = false;
        }

        private void OnPlaneEngineStarted()
        {
            planePilotEjectController.EnableEjection();
            planeSpriteController.SetFlySprite();
            planeRotation.EnableRotationUp();
            planeMovement.DisableInput();
            planeWeapon.enabled = true;
        }

        private void OnPlaneTookOff()
        {
            planeRotation.EnableRotationDown();
            planeMovement.EnableInput();
        }

        public void OnPlaneCrashed()
        {
            planePilotEjectController.DisableEjection();
            networkedPlaneController.PlayCrashedGraphicsRpc();
            planeMovement.DisableMovement();
            planeRotation.DisableRotation();
            planeWeapon.enabled = false;
        }

        public void PlayCrashedGraphics()
        {
            planeParticles.DisableDamageEffects();
            planeParticles.PlayExplosion();
            planeSpriteController.SetDestroyedSprite();
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

        private void OnPilotEjected()
        {
            planeRotation.DisableRotation();
            planeMovement.DisableInput();
            if (planeMovement.DidTookOff) {
                planeMovement.DisableEngine();
            }
        }
    }
}