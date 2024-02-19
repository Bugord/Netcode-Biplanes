using System.Text.RegularExpressions;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkService : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField inputField;

    public void StartHost()
    {
        var utpTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        if (!utpTransport) {
            return;
        }
        utpTransport.ConnectionData = new UnityTransport.ConnectionAddressData() {
            Port = 7777,
            ServerListenAddress = "0.0.0.0"
        };

        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        var utpTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        if (utpTransport) {
            utpTransport.ConnectionData = new UnityTransport.ConnectionAddressData() {
                Address = Sanitize(inputField.text),
                Port = 7777
            };
        }
        if (!NetworkManager.Singleton.StartClient()) {
            Debug.LogError("Failed to start client.");
        }
    }

    static string Sanitize(string dirtyString)
    {
        // sanitize the input for the ip address
        return Regex.Replace(dirtyString, "[^A-Za-z0-9.]", "");
    }
}