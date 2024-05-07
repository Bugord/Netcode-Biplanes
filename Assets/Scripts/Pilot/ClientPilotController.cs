using System;
using System.Collections;
using Core;
using Unity.Netcode;
using UnityEngine;

namespace Pilot
{
    public class ClientPilotController : MonoBehaviour
    {
        [SerializeField]
        private NetworkedPilotController networkedPilotController;

        [SerializeField]
        private float fallSpeedToDie;

        [SerializeField]
        private Animator animator;

        [SerializeField]
        private PilotMovement pilotMovement;

        [SerializeField]
        private PilotParachuteController pilotParachuteController;

        private PilotState state;

        private bool isInRespawnArea;

        private static readonly int Die = Animator.StringToHash("Die");
        private static readonly int Grounded = Animator.StringToHash("Grounded");

        private void Awake()
        {
            networkedPilotController.OnNetworkSpawnHook += OnNetworkSpawn;
            pilotParachuteController.ParachuteOpened += OnParachuteOpened;
            pilotParachuteController.ParachuteHiden += OnParachuteHidden;
        }

        private void OnDestroy()
        {
            networkedPilotController.OnNetworkSpawnHook -= OnNetworkSpawn;
            pilotParachuteController.ParachuteOpened -= OnParachuteOpened;
            pilotParachuteController.ParachuteHiden -= OnParachuteHidden;
        }
        
        public void OnPilotDead()
        {
            state = PilotState.Dead;
            animator.SetTrigger(Die);
            pilotMovement.SetDeadMovement();
            pilotParachuteController.HideParachute();
        }

        private void OnNetworkSpawn()
        {
            if (!networkedPilotController.IsOwner) {
                pilotMovement.enabled = false;
            }
        }

        private void OnParachuteHidden()
        {
            if (state != PilotState.Parachuted) {
                return;
            }

            state = PilotState.Falling;
            pilotMovement.SetFallingMovement();
        }

        private void OnParachuteOpened()
        {
            if (state != PilotState.Falling) {
                return;
            }

            state = PilotState.Parachuted;
            pilotMovement.SetParachuteMovement();
        }

        private void Update()
        {
            if (!networkedPilotController.IsOwner) {
                return;
            }

            if (Input.GetKeyDown(KeyCode.E)) {
                pilotParachuteController.OpenParachute();
            }

            var direction = 0;
            if (Input.GetKey(KeyCode.A)) {
                direction = -1;
            }
            if (Input.GetKey(KeyCode.D)) {
                direction = 1;
            }

            switch (state) {
                case PilotState.Parachuted:
                    pilotMovement.MoveWithParachute(direction);
                    pilotParachuteController.SetParachuteDirection(direction);
                    break;
                case PilotState.Grounded:
                    pilotMovement.MoveOnGround(direction);
                    break;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision2D)
        {
            if (collision2D.gameObject.CompareTag("Ground") || collision2D.gameObject.CompareTag("House")) {
                if (collision2D.relativeVelocity.y >= fallSpeedToDie) {
                    Debug.Log($"[{nameof(NetworkedPilotController)}] Pilot fell");
                    OnPilotDead();
                    networkedPilotController.OnPilotDiedRpc(PlaneCrashReason.Suicide);
                }
                else {
                    SetStateGrounded();
                }
            }
        }

        private void SetStateGrounded()
        {
            state = PilotState.Grounded;
            pilotParachuteController.HideParachute();
            animator.SetTrigger(Grounded);
            if (isInRespawnArea) {
                networkedPilotController.OnPilotEnterRespawnArea();
            }
        }

        private void OnTriggerEnter2D(Collider2D collider2D)
        {
            if (collider2D.CompareTag("RespawnArea")) {
                isInRespawnArea = true;
                if (state == PilotState.Grounded) {
                    networkedPilotController.OnPilotEnterRespawnArea();
                }
            }
        }
    }
}