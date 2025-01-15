using System.Net;
using System.Net.Sockets;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

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

    [Space(20)] [Header("Port Range")] [SerializeField]
    internal int minPort = 8000;

    [SerializeField] internal int maxPort = 8100;

    private GameSessionAnnouncer _announcer;
    private GameSessionDiscoverer _discoverer;
    private NetworkStatsUI _networkStatsUI;

    private void Awake()
    {
        hostButton.onClick.AddListener(ShowHostForm);
        startHostButton.onClick.AddListener(StartHost);
        cancelHostButton.onClick.AddListener(ShowHostList);
        refreshButton.onClick.AddListener(UpdateHostList);
    }

    private void Start()
    {
        networkManagerCanvas.enabled = true;
        ShowHostList();

        _announcer = gameObject.GetComponent<GameSessionAnnouncer>();
        _discoverer = gameObject.GetComponent<GameSessionDiscoverer>();
        _networkStatsUI = FindFirstObjectByType<NetworkStatsUI>();

        _discoverer.OnGameSessionDiscovered += AddHostToList;
        UpdateHostList();
    }

    private new void OnDestroy()
    {
        _announcer.Dispose();
        _discoverer.Dispose();
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

    public void EnableUI()
    {
        networkManagerCanvas.enabled = true;
        ShowHostList();
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
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.ConnectionData.Port = (ushort)port;

        if (NetworkManager.Singleton.StartHost())
        {
            _announcer.Initialize(port, lobbyName);
            networkManagerCanvas.enabled = false;
            _networkStatsUI?.UpdatePort();
        }
        else
        {
            Debug.LogError("Failed to start host.");
        }
    }

    private int GetRandomUnusedPort()
    {
        var random = new Random();
        int port;
        TcpListener listener = null;

        do
        {
            port = random.Next(minPort, maxPort);
            try
            {
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
            }
            catch (SocketException)
            {
                port = 0;
            }
            finally
            {
                listener?.Stop();
            }
        } while (port == 0);

        return port;
    }

    private void AddHostToList(string hostName, int port)
    {
        if (string.IsNullOrEmpty(hostName) || port <= 0)
        {
            Debug.LogWarning("Invalid lobby name or port received. Skipping button creation.");
            return;
        }

        var lobbyButtonInstance = Instantiate(joinLobbyButtonPrefab, hostListContent);
        var hostButtonScript = lobbyButtonInstance.GetComponent<LobbyButton>();
        hostButtonScript.Initialize(hostName, () => JoinLobby(port));
    }

    private void UpdateHostList()
    {
        foreach (Transform child in hostListContent) Destroy(child.gameObject);

        // Trigger discovery of game sessions
        _discoverer.RefreshReceiver();
    }

    private void JoinLobby(int port)
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Port = (ushort)port;
        NetworkManager.Singleton.StartClient();
        networkManagerCanvas.enabled = false;

        // Update the port display
        FindFirstObjectByType<NetworkStatsUI>().UpdatePort();
    }
}
