using System;
using Network;
using UI;
using UI.Screens;
using Unity.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        DontDestroyOnLoad(сonnectionManager.gameObject);
    }

    private void Start()
    {
#if !UNITY_EDITOR
        SetWindowResolution();
#endif
        InitializeNetworkLogic();
    }

    private void SetWindowResolution()
    {
        Screen.SetResolution(512, 512, FullScreenMode.Windowed);
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
            case MultiplayerRoleFlags.ClientAndServer:
            case MultiplayerRoleFlags.Client:
                if (clientAutoConnect) {
                    сonnectionManager.StartClientIp($"Client {Random.value}", DefaultClientConnectIpAddress, listeningPort);
                }
                else {
                    SceneManager.LoadScene("MainMenuScene");
                    NavigationSystem.Instance.Push<MainMenuScreen>();
                }
                break;
        }
    }
}