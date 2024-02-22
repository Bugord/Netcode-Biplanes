using System.Text.RegularExpressions;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Network
{
    public class NetworkService : MonoBehaviour
    {
        private static NetworkService instance;
        public static NetworkService Instance => instance;
        private NetworkManager NetworkManager => NetworkManager.Singleton;

        private void Awake()
        {
            instance = this;
        }

        public bool StartHost(string port)
        {
            var utpTransport = (UnityTransport)NetworkManager.NetworkConfig.NetworkTransport;
            if (!utpTransport) {
                return false;
            }

            NetworkManager.NetworkConfig.ConnectionApproval = true;
            NetworkManager.OnClientConnectedCallback += OnClientConnected; 
        
            utpTransport.ConnectionData = new UnityTransport.ConnectionAddressData() {
                Port = ushort.Parse(port),
                ServerListenAddress = "0.0.0.0"
            };

            return NetworkManager.Singleton.StartHost();
        }

        private void OnClientConnected(ulong id)
        {
            Debug.Log(id);
        }

        public bool StartClient(string ipAddress, string port)
        {
            var utpTransport = (UnityTransport)NetworkManager.NetworkConfig.NetworkTransport;
            if (utpTransport) {
                utpTransport.ConnectionData = new UnityTransport.ConnectionAddressData() {
                    Address = SanitizeIpAddress(ipAddress),
                    Port = ushort.Parse(port),
                };
            }

            if (NetworkManager.StartClient()) {
                return true;
            }
            
            Debug.LogError("Failed to start client.");
            return false;
        }

        public void Shutdown()
        {
            NetworkManager.Shutdown();
        }

        static string SanitizeIpAddress(string dirtyString)
        {
            return Regex.Replace(dirtyString, "[^A-Za-z0-9.]", "");
        }
    }
}