using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace _Project
{
    public class GameManager : NetworkSingleton<GameManager>
    {
        public PlayerSpawnPosition player1SpawnPosition;
        public PlayerSpawnPosition player2SpawnPosition;
        public CameraSwitcher cameraSwitcher;
        public TextMeshProUGUI networkStatsText;
        public GameObject gameOverPanel;
        public TextMeshProUGUI rematchStatusText;

        private NetworkManagerUI _networkManagerUI;
        private NetworkStatsUI _networkStatsUI;
        private int _playerCount;
        private NetworkVariable<int> rematchVotes = new(0);

        private void Start()
        {
            if (player1SpawnPosition == null || player2SpawnPosition == null)
                Debug.LogError("Please assign the player spawn positions.");

            _networkManagerUI = GetComponent<NetworkManagerUI>();
            _networkStatsUI = FindFirstObjectByType<NetworkStatsUI>();

            if (_networkStatsUI == null) Debug.LogError("NetworkStatsUI not found in the scene.");
            if (cameraSwitcher == null) Debug.LogError("Please assign the CameraSwitcher.");
        }

        private new void OnDestroy()
        {
            if (NetworkManager.Singleton != null && IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (NetworkManager.Singleton != null && IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            _playerCount++;
            switch (_playerCount)
            {
                case 1:
                    cameraSwitcher.enabled = false;
                    InitializePlayer(clientId);
                    break;
                case 2:
                    StartGame();
                    break;
                case >= 2:
                    NetworkManager.Singleton.DisconnectClient(clientId);
                    return;
            }

            ChatManager.Instance.playerName = $"Player {_playerCount}";
            ChatManager.Instance.SendChatMessage($"Player {_playerCount} has joined the game.", "Server");

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            UpdatePlayerCountText();
        }

        private void OnClientDisconnected(ulong clientId)
        {
            _playerCount--;
            switch (_playerCount)
            {
                case 0:
                    cameraSwitcher.enabled = true;
                    break;
                case < 2:
                    EndGame();
                    break;
            }

            ChatManager.Instance.playerName = $"Player {_playerCount}";
            ChatManager.Instance.SendChatMessage($"Player {_playerCount} has disconnected the game.", "Server");
            ChatManager.Instance.chatPanel.SetActive(false);

            UpdatePlayerCountText();
        }

        private void StartGame()
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList) InitializePlayer(client.ClientId);
        }

        private void InitializePlayer(ulong clientId)
        {
            var playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
            var spawnPosition = clientId == 0
                ? player1SpawnPosition.GetRandomSpawnPosition()
                : player2SpawnPosition.GetRandomSpawnPosition();
            playerObject.transform.position = spawnPosition;
            playerObject.transform.rotation = Quaternion.identity;

            _networkStatsUI.UpdatePort();
            UpdatePlayerCountText();
        }

        private void UpdatePlayerCountText()
        {
            networkStatsText.text = $"#{_playerCount}";
        }

        private void EndGame()
        {
            gameOverPanel.SetActive(true);
            rematchVotes.Value = 0;
            UpdateRematchStatus();
            Time.timeScale = 0;
        }

        public void Rematch()
        {
            rematchVotes.Value++;
            UpdateRematchStatus();
            if (rematchVotes.Value == 2)
            {
                gameOverPanel.SetActive(false);
                Time.timeScale = 1;
                foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
                {
                    InitializePlayer(client.ClientId);
                }
            }
        }

        private void UpdateRematchStatus()
        {
            rematchStatusText.text = $"Rematch {rematchVotes.Value}/2";
        }
    }
}