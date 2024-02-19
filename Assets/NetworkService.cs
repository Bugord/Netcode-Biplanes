using System.Net;
using System.Text.RegularExpressions;
using Netcode.Transports.LiteNetLib;
using Netcode.Transports.Ruffles;
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
        var utpTransport = (RufflesTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        if (!utpTransport) {
            return;
        }
        
        utpTransport.Port = 7777;
        NetworkManager.Singleton.StartHost();
    }

    public void StartClient()
    {
        var utpTransport = (RufflesTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        if (utpTransport) {
            utpTransport.ConnectAddress = Sanitize(inputField.text);
            utpTransport.Port = 7777;
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