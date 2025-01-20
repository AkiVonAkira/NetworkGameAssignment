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
        public TextMeshProUGUI rematchStatusText;

        private NetworkManagerUI _networkManagerUI;
        private NetworkStatsUI _networkStatsUI;
        private int _playerCount;
            
        private readonly NetworkVariable<int> _rematchVotes = new();
        private string _player1Name = new("Player 1");
        private string _player2Name = new("Player 2");

        private void Start()
        {
            if (player1SpawnPosition == null || player2SpawnPosition == null)
                Debug.LogError("Please assign the player spawn positions.");

            _networkManagerUI = GetComponent<NetworkManagerUI>();
            _networkStatsUI = FindFirstObjectByType<NetworkStatsUI>();

            if (_networkStatsUI == null) Debug.LogError("NetworkStatsUI not found in the scene.");
            if (cameraSwitcher == null) Debug.LogError("Please assign the CameraSwitcher.");
            
            PlayerNetworkStats.OnPlayerDied += HandlePlayerDied;
        }

        private new void OnDestroy()
        {
            if (NetworkManager.Singleton != null && IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }

            PlayerNetworkStats.OnPlayerDied -= HandlePlayerDied;
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
            if (!IsServer) return;
            _playerCount++;
            switch (_playerCount)
            {
                case 1:
                    cameraSwitcher.enabled = false;
                    InitializePlayer(clientId, _player1Name);
                    break;
                case 2:
                    InitializePlayer(clientId, _player2Name);
                    StartGame();
                    break;
                case >= 2:
                    NetworkManager.Singleton.DisconnectClient(clientId);
                    return;
            }

            ChatManager.Instance.playerName = clientId == 0 ? _player1Name : _player2Name;
            ChatManager.Instance.SendChatMessage($"{ChatManager.Instance.playerName} has joined the game.", "Server");

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            UpdatePlayerCountText();
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (!IsServer) return;
            _playerCount--;
            switch (_playerCount)
            {
                case 0:
                    cameraSwitcher.enabled = true;
                    break;
                case < 2:
                    ChatManager.Instance.playerName = clientId == 0 ? _player1Name : _player2Name;
                    ChatManager.Instance.SendChatMessage($"{ChatManager.Instance.playerName} has disconnected the game.", "Server");
                    ChatManager.Instance.chatPanel.SetActive(false);
                    EndGame(false);
                    break;
            }

            UpdatePlayerCountText();
        }

        private void StartGame()
        {
            if (!IsServer) return;
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                var playerName = client.ClientId == 0 ? _player1Name : _player2Name;
                InitializePlayer(client.ClientId, playerName);
            }
        }

        private void InitializePlayer(ulong clientId, string playerName)
        {
            if (!IsServer) return;
            var playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
            var spawnPosition = clientId == 0
                ? player1SpawnPosition.GetRandomSpawnPosition()
                : player2SpawnPosition.GetRandomSpawnPosition();
            playerObject.transform.position = spawnPosition;
            playerObject.transform.Rotate(Vector3.up, clientId == 0 ? 0 : 180);

            _networkStatsUI.UpdatePort();
            UpdatePlayerCountText();
        }

        private void UpdatePlayerCountText()
        {
            networkStatsText.text = $"#{_playerCount}";
        }

        private void HandlePlayerDied(ulong playerId)
        {
            var playerName = playerId == 0 ? _player1Name : _player2Name;
            ChatManager.Instance.SendChatMessage($"{playerName} has died.", "Server");
            var winnerName = playerId == 0 ? _player2Name : _player1Name;
            SetGameOverText(winnerName);
        }

        public void EndGame(bool allowRematch = true)
        {
            if (!IsServer) return;
            GameOverUI.Instance.Show();
            if (allowRematch)
            {
                GameOverUI.Instance.rematchButton.interactable = true;
                _rematchVotes.Value = 0;
                UpdateRematchStatus();
            }
            else
            {
                rematchStatusText.text = "Rematch Disabled";
                GameOverUI.Instance.rematchButton.interactable = false;
            }

            Time.timeScale = 0;
        }

        public void Rematch()
        {
            if (!IsServer) return;
            _rematchVotes.Value++;
            UpdateRematchStatus();
            if (_rematchVotes.Value == 2)
            {
                GameOverUI.Instance.Hide();
                Time.timeScale = 1;
                StartGame();
            }
        }

        private void UpdateRematchStatus()
        {
            rematchStatusText.text = $"Rematch {_rematchVotes.Value}/2";
        }

        private void SetGameOverText(string winnerName)
        {
            GameOverUI.Instance.winnerText.text = $"{winnerName} has won the game!";
        }
    }
}