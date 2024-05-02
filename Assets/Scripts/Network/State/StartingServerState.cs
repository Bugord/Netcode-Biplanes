using System;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network.State
{
    /// <summary>
    /// Connection state corresponding to a server starting up. Starts the server when entering the state. If successful,
    /// transitions to the ServerListening state, if not, transitions back to the Offline state.
    /// </summary>
    public class StartingServerState : OnlineState
    {
        public StartingServerState(ConnectionManager connectionManager) : base(connectionManager)
        {
        }
        
        private string m_IPAddress;
        private ushort m_Port;

        public void Configure(string iPAddress, ushort port)
        {
            m_IPAddress = iPAddress;
            m_Port = port;
        }

        public override void Enter()
        {
            StartServer();
        }

        public override void Exit()
        {
        }

        public override void OnServerStarted()
        {
            //todo: change to event system
            ConnectionManager.ChangeState(ConnectionManager.ServerListening);
        }

        public override void OnServerStopped()
        {
            StartServerFailed();
        }

        private void StartServerFailed()
        {
            ConnectionManager.ChangeState(ConnectionManager.Offline);
            Quit();
        }

        private void StartServer()
        {
            try {
                var utp = (UnityTransport)ConnectionManager.NetworkManager.NetworkConfig.NetworkTransport;
                utp.SetConnectionData(m_IPAddress, m_Port);

                // NGO's StartServer launches everything
                Debug.Log($"Starting server, listening on {m_IPAddress} with port {m_Port}");
                if (!ConnectionManager.NetworkManager.StartServer()) {
                    StartServerFailed();
                }
            }
            catch (Exception) {
                StartServerFailed();
                throw;
            }
        }

        private void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}