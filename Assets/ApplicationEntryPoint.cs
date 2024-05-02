using System;
using Network;
using Unity.Multiplayer;
using UnityEngine;
using Random = UnityEngine.Random;

public class ApplicationEntryPoint : MonoBehaviour
{
    private readonly string DefaultServerListenIpAddress = "0.0.0.0";
    private readonly string DefaultClientConnectIpAddress = "127.0.0.1";

    [SerializeField]
    private bool clientAutoConnect = false;

    [SerializeField]
    ConnectionManager сonnectionManager;
    
    public int PlayersCount => 2;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeNetworkLogic();
    }

    private void InitializeNetworkLogic()
    {
        var commandLineArgumentsParser = new CommandLineArgumentsParser();
        var listeningPort = (ushort)commandLineArgumentsParser.Port;

        switch (MultiplayerRolesManager.ActiveMultiplayerRoleMask) {
            case MultiplayerRoleFlags.Server:
                //lock framerate on dedicated servers
                Application.targetFrameRate = commandLineArgumentsParser.TargetFramerate;
                QualitySettings.vSyncCount = 0;
                сonnectionManager.StartServerIp(DefaultServerListenIpAddress, listeningPort);
                break;
            case MultiplayerRoleFlags.Client: {
                // SceneManager.LoadScene("MetagameScene");
                if (clientAutoConnect) {
                    сonnectionManager.StartClientIp($"Client {Random.value}", DefaultClientConnectIpAddress,
                        listeningPort);
                }
                break;
            }
            case MultiplayerRoleFlags.ClientAndServer:
                throw new ArgumentOutOfRangeException("MultiplayerRole",
                    "ClientAndServer is an invalid multiplayer role. Please select the Client or Server role.");
        }
    }
}