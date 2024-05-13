using System;
using Core;
using Plane;
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

        private readonly int Die = Animator.StringToHash("Die");
        private readonly int Grounded = Animator.StringToHash("Grounded");

        private const string RespawnAreaTag = "RespawnArea";
        private const string GroundTag = "Ground";
        private const string HouseTag = "House";

        private void Awake()
        {
            pilotParachuteController.ParachuteOpened += OnParachuteOpened;
            pilotParachuteController.ParachuteHidden += OnParachuteHidden;
            networkedPilotController.OnNetworkSpawnHook += OnNetworkSpawn;
        }

        private void OnDestroy()
        {
            pilotParachuteController.ParachuteOpened -= OnParachuteOpened;
            pilotParachuteController.ParachuteHidden -= OnParachuteHidden;
            networkedPilotController.OnNetworkSpawnHook -= OnNetworkSpawn;
        }

        private void OnNetworkSpawn()
        {
            if (!networkedPilotController.IsOwner) {
                enabled = false;
                pilotMovement.enabled = false;
                pilotParachuteController.enabled = false;
            }
        }

        private void Update()
        {
            ProcessParachuteOpening();
            ProcessMovement();
        }

        public void KillPlayer()
        {
            state = PilotState.Dead;
            animator.SetTrigger(Die);
            pilotMovement.SetDeadMovement();
            pilotParachuteController.HideParachute();
        }

        private void OnParachuteOpened()
        {
            if (state != PilotState.Falling) {
                return;
            }

            state = PilotState.Parachuted;
            pilotMovement.SetParachuteMovement();
        }

        private void OnParachuteHidden()
        {
            if (state != PilotState.Parachuted) {
                return;
            }

            state = PilotState.Falling;
            pilotMovement.SetFallingMovement();
        }

        private void SetStateGrounded()
        {
            state = PilotState.Grounded;
            pilotParachuteController.HideParachute();
            animator.SetTrigger(Grounded);
            if (isInRespawnArea) {
                networkedPilotController.RespawnPlayer();
            }
        }

        private void ProcessParachuteOpening()
        {
            if (state != PilotState.Falling) {
                return;
            }

            if (Input.GetKeyDown(KeyCode.E)) {
                pilotParachuteController.OpenParachute();
            }
        }

        private void ProcessMovement()
        {
            var direction = GetMovement();

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

        private int GetMovement()
        {
            if (Input.GetKey(KeyCode.A)) {
                return -1;
            }

            if (Input.GetKey(KeyCode.D)) {
                return 1;
            }

            return 0;
        }

        private void OnCollisionEnter2D(Collision2D collision2D)
        {
            if (collision2D.gameObject.CompareTag(GroundTag) || collision2D.gameObject.CompareTag(HouseTag)) {
                if (collision2D.relativeVelocity.y >= fallSpeedToDie) {
                    networkedPilotController.KillPlayerWithReason(PlaneDiedReason.Suicide);
                }
                else {
                    SetStateGrounded();
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collider2D)
        {
            if (!collider2D.CompareTag(RespawnAreaTag)) {
                return;
            }

            isInRespawnArea = true;
            if (state == PilotState.Grounded) {
                networkedPilotController.RespawnPlayer();
            }
        }

        private void OnTriggerExit2D(Collider2D collider2D)
        {
            if (!collider2D.CompareTag(RespawnAreaTag)) {
                return;
            }

            isInRespawnArea = false;
        }
    }
}