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

        private NetworkManagerUI _networkManagerUI;
        private NetworkStatsUI _networkStatsUI;
        private int _playerCount;

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
            
            var playerNumber = (int)NetworkManager.Singleton.LocalClientId + 1;
            ChatManager.Instance.playerName = $"Player {playerNumber}";
            ChatManager.Instance.SendChatMessage($"Player {playerNumber} has joined the game.", "Server");
            
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
            
            var playerNumber = (int)NetworkManager.Singleton.LocalClientId + 1;
            ChatManager.Instance.playerName = $"Player {playerNumber}";
            ChatManager.Instance.SendChatMessage($"Player {playerNumber} has disconnected the game.", "Server");
            
            UpdatePlayerCountText();
        }

        private void StartGame()
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                InitializePlayer(client.ClientId);
            }
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
            networkStatsText.text = $"{_playerCount}P Connected";
        }

        private void EndGame()
        {
            // Implement game end logic here
        }
    }
}
