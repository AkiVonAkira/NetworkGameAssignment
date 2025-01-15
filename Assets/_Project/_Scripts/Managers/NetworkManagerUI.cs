using System;
using System.Net;
using System.Net.Sockets;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class NetworkManagerUI : NetworkBehaviour
{
    [SerializeField] private Button hostButton;
    [SerializeField] private Button startHostButton;
    [SerializeField] private Button cancelHostButton;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Canvas networkManagerCanvas;
    [SerializeField] private GameObject lobbyList;
    [SerializeField] private GameObject hostForm;
    [SerializeField] private TMP_InputField lobbyNameInput;
    [SerializeField] private Transform hostListContent;
    [SerializeField] private GameObject joinLobbyButtonPrefab;

    private readonly NetworkList<Lobby> _availableLobbies = new();

    private void Awake()
    {
        hostButton.onClick.AddListener(ShowHostForm);
        startHostButton.onClick.AddListener(StartHost);
        cancelHostButton.onClick.AddListener(ShowHostList);
        refreshButton.onClick.AddListener(UpdateHostList);
    }

    private void Start()
    {
        hostForm.SetActive(false);
        lobbyList.SetActive(true);
        networkManagerCanvas.enabled = true;
        _availableLobbies.OnListChanged += OnAvailableLobbiesChanged;
        UpdateHostList();
    }

    private void ShowHostForm()
    {
        lobbyList.SetActive(false);
        hostForm.SetActive(true);
    }

    private void ShowHostList()
    {
        lobbyList.SetActive(true);
        hostForm.SetActive(false);
    }

    private void StartHost()
    {
        var lobbyName = lobbyNameInput.text;
        if (string.IsNullOrEmpty(lobbyName))
        {
            Debug.LogError("Lobby name cannot be empty.");
            return;
        }

        var port = GetRandomUnusedPort();
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Port = (ushort)port;
        NetworkManager.Singleton.StartHost();
        hostForm.SetActive(false);
        networkManagerCanvas.enabled = false;

        // Add the host to the list of available lobbies
        _availableLobbies.Add(new Lobby { name = lobbyName, port = port });

        // Update the port display
        FindFirstObjectByType<NetworkStatsUI>().UpdatePort();
    }

    private int GetRandomUnusedPort()
    {
        var listener = new TcpListener(IPAddress.Any, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private void AddHostToList(FixedString64Bytes hostName, UnityAction joinAction)
    {
        var lobbyButtonInstance = Instantiate(joinLobbyButtonPrefab, hostListContent);
        var hostButtonScript = lobbyButtonInstance.GetComponent<LobbyButton>();
        hostButtonScript.Initialize(hostName, joinAction);
    }

    private void UpdateHostList()
    {
        foreach (Transform child in hostListContent) Destroy(child.gameObject);

        foreach (var lobby in _availableLobbies) AddHostToList(lobby.name, () => JoinLobby(lobby.port));
    }

    private void JoinLobby(int port)
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Port = (ushort)port;
        NetworkManager.Singleton.StartClient();
        networkManagerCanvas.enabled = false;
        
        // Update the port display
        FindFirstObjectByType<NetworkStatsUI>().UpdatePort();
    }

    private void OnAvailableLobbiesChanged(NetworkListEvent<Lobby> changeEvent)
    {
        UpdateHostList();
    }

    [Serializable]
    private struct Lobby : INetworkSerializable, IEquatable<Lobby>
    {
        public FixedString64Bytes name;
        public int port;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref name);
            serializer.SerializeValue(ref port);
        }

        public bool Equals(Lobby other)
        {
            return name.Equals(other.name) && port == other.port;
        }

        public override bool Equals(object obj)
        {
            return obj is Lobby other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(name, port);
        }
    }
}
