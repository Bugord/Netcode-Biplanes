using System.Threading.Tasks;
using UnityEngine;

namespace Network.ConnectionMethods
{
    public abstract class BaseConnectionMethod
    {
        protected ConnectionManager ConnectionManager;
        protected readonly string PlayerName;

        /// <summary>
        /// Setup the host connection prior to starting the NetworkManager
        /// </summary>
        /// <returns></returns>
        public abstract Task SetupHostConnectionAsync();

        /// <summary>
        /// Setup the client connection prior to starting the NetworkManager
        /// </summary>
        /// <returns></returns>
        public abstract Task SetupClientConnectionAsync();

        /// <summary>
        /// Setup the client for reconnection prior to reconnecting
        /// </summary>
        /// <returns>
        /// success = true if succeeded in setting up reconnection, false if failed.
        /// shouldTryAgain = true if we should try again after failing, false if not.
        /// </returns>
        public abstract Task<(bool success, bool shouldTryAgain)> SetupClientReconnectionAsync();

        protected BaseConnectionMethod(ConnectionManager connectionManager, string playerName)
        {
            ConnectionManager = connectionManager;
            PlayerName = playerName;
        }

        protected void SetConnectionPayload(string playerId, string playerName)
        {
            var payload = JsonUtility.ToJson(new ConnectionPayload() {
                playerId = playerId,
                playerName = playerName,
                isDebug = Debug.isDebugBuild,
                applicationVersion = Application.version
            });

            var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);

            ConnectionManager.NetworkManager.NetworkConfig.ConnectionData = payloadBytes;
        }

        /// Using authentication, this makes sure your session is associated with your account and not your device. This means you could reconnect
        /// from a different device for example. A playerId is also a bit more permanent than player prefs. In a browser for example,
        /// player prefs can be cleared as easily as cookies.
        /// The forked flow here is for debug purposes and to make UGS optional in Boss Room. This way you can study the sample without
        /// setting up a UGS account. It's recommended to investigate your own initialization and IsSigned flows to see if you need
        /// those checks on your own and react accordingly. We offer here the option for offline access for debug purposes, but in your own game you
        /// might want to show an error popup and ask your player to connect to the internet.
        protected string GetPlayerId()
        {
            //     if (Services.Core.UnityServices.State != ServicesInitializationState.Initialized) {
            //         return ClientPrefs.GetGuid() + m_ProfileManager.Profile;
            //     }
            //
            //     return AuthenticationService.Instance.IsSignedIn
            //         ? AuthenticationService.Instance.PlayerId
            //         : ClientPrefs.GetGuid() + m_ProfileManager.Profile;

            return PlayerName;
        }
    }
    
}