using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

namespace Core
{
    [RequireComponent(typeof(NetworkedPlaneController))]
    public class ClientPlaneController : MonoBehaviour
    {
        [SerializeField]
        private NetworkedPlaneController networkedPlaneController;

        private bool isEngineOn;

        private void Awake()
        {
            networkedPlaneController.OnNetworkSpawnHook += OnNetworkSpawn;
        }

        private void OnDestroy()
        {
            networkedPlaneController.OnNetworkSpawnHook -= OnNetworkSpawn;
        }

        private void OnNetworkSpawn()
        {
            if (!networkedPlaneController.IsOwner) {
                enabled = false;
            }
        }

        private void Update()
        {
            ChangeEngineEnabled();
        }

        private void ChangeEngineEnabled()
        {
            if (Input.GetKey(KeyCode.W)) {
                isEngineOn = true;
            }
            if (Input.GetKey(KeyCode.S)) {
                isEngineOn = false;
            }
        }
    }
}