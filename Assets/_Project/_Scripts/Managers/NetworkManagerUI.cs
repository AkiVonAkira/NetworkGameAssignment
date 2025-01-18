using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace _Project
{
    public class NetworkManagerUI : NetworkSingleton<NetworkManagerUI>
    {
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button startHostButton;
        [SerializeField] private Button cancelHostButton;
        [SerializeField] private Button refreshButton;
        [SerializeField] internal Canvas networkManagerCanvas;
        [SerializeField] private GameObject lobbyList;
        [SerializeField] private GameObject hostForm;
        [SerializeField] private TMP_InputField lobbyNameInput;
        [SerializeField] private Transform hostListContent;
        [SerializeField] private GameObject joinLobbyButtonPrefab;

        [Space(20)] [Header("Port Range")] [SerializeField]
        internal int minPort = 8000;

        [SerializeField] internal int maxPort = 8100;

        //private GameSessionAnnouncer _announcer;
        //private GameSessionDiscoverer _discoverer;
        private NetworkStatsUI _networkStatsUI;

        private new void Awake()
        {
            base.Awake();
            //hostButton.onClick.AddListener(ShowHostForm);
            //startHostButton.onClick.AddListener(StartHost);
            //cancelHostButton.onClick.AddListener(ShowHostList);
            //refreshButton.onClick.AddListener(UpdateHostList);
            hostButton.onClick.AddListener(StartHost);
            joinButton.onClick.AddListener(JoinLobby);
        }

        private void Start()
        {
            //_announcer = gameObject.GetComponent<GameSessionAnnouncer>();
            //_discoverer = gameObject.GetComponent<GameSessionDiscoverer>();
            _networkStatsUI = FindFirstObjectByType<NetworkStatsUI>();

            //_discoverer.OnGameSessionDiscovered += AddHostToList;

            networkManagerCanvas.enabled = true;
        }

        // private new void OnDestroy()
        // {
        //     _announcer.Dispose();
        //     _announcer.DisposeUdpClient();
        //     _discoverer.Dispose();
        //     _discoverer.DisposeUdpClient();
        // }

        // private void ShowHostForm()
        // {
        //     lobbyList.SetActive(false);
        //     hostForm.SetActive(true);
        // }
        //
        // private void ShowHostList()
        // {
        //     lobbyList.SetActive(true);
        //     hostForm.SetActive(false);
        // }
        //
        // public void EnableUI()
        // {
        //     networkManagerCanvas.enabled = true;
        //     ShowHostList();
        // }

        private void StartHost()
        {
            if (NetworkManager.Singleton.StartHost())
            {
                networkManagerCanvas.enabled = false;
                _networkStatsUI?.UpdatePort();
            }
            else
            {
                Debug.LogError("Failed to start host.");
            }
            // var lobbyName = lobbyNameInput.text;
            // if (string.IsNullOrEmpty(lobbyName))
            // {
            //     Debug.LogError("Lobby name cannot be empty.");
            //     return;
            // }
            //
            // var port = GetRandomUnusedPort();
            // var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            // transport.ConnectionData.Port = (ushort)port;
            //
            // _announcer.InitializeUdpClient();
            //
            // if (NetworkManager.Singleton.StartHost())
            // {
            //     _discoverer.StopDiscovery();
            //     _announcer.Initialize(port, lobbyName);
            //     networkManagerCanvas.enabled = false;
            //     _networkStatsUI?.UpdatePort();
            // }
            // else
            // {
            //     Debug.LogError("Failed to start host.");
            // }
        }

        private void JoinLobby()
        {
            //NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Port = (ushort)port;
            NetworkManager.Singleton.StartClient();
            networkManagerCanvas.enabled = false;

            // Update the port display
            FindFirstObjectByType<NetworkStatsUI>().UpdatePort();
        }

        // private int GetRandomUnusedPort()
        // {
        //     var random = new Random();
        //     int port;
        //     TcpListener listener = null;
        //
        //     do
        //     {
        //         port = random.Next(minPort, maxPort);
        //         try
        //         {
        //             listener = new TcpListener(IPAddress.Any, port);
        //             listener.Start();
        //         }
        //         catch (SocketException)
        //         {
        //             port = 0;
        //         }
        //         finally
        //         {
        //             listener?.Stop();
        //         }
        //     } while (port == 0);
        //
        //     return port;
        // }
        //
        // private void AddHostToList(string hostName, int port)
        // {
        //     if (string.IsNullOrEmpty(hostName) || port <= 0)
        //     {
        //         Debug.LogWarning("Invalid lobby name or port received. Skipping button creation.");
        //         return;
        //     }
        //
        //     var lobbyButtonInstance = Instantiate(joinLobbyButtonPrefab, hostListContent);
        //     var hostButtonScript = lobbyButtonInstance.GetComponent<LobbyButton>();
        //     hostButtonScript.Initialize(hostName, () => JoinLobby(port));
        // }
        //
        // private void UpdateHostList()
        // {
        //     foreach (Transform child in hostListContent) Destroy(child.gameObject);
        //
        //     // Trigger discovery of game sessions
        //     _discoverer.StartDiscovery();
        // }
        //
        // public void StopBroadcasting()
        // {
        //     _announcer.Dispose();
        // }
        //
        // public void StopDiscovering()
        // {
        //     _discoverer.Dispose();
        // }
    }
}
