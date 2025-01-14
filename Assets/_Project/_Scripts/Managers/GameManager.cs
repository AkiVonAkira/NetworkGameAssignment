using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public PlayerSpawnPosition player1SpawnPosition;
    public PlayerSpawnPosition player2SpawnPosition;

    private NetworkStatsUI _networkStatsUI;
    private int _playerCount;

    private void Start()
    {
        if (player1SpawnPosition == null || player2SpawnPosition == null)
            Debug.LogError("Please assign the player spawn positions.");

        _networkStatsUI = FindFirstObjectByType<NetworkStatsUI>();
        if (_networkStatsUI == null) Debug.LogError("NetworkStatsUI not found in the scene.");
    }

    private new void OnDestroy()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (_playerCount >= 2)
        {
            NetworkManager.Singleton.DisconnectClient(clientId);
            return;
        }

        _playerCount++;
        if (_playerCount == 2) StartGame();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        _playerCount--;
        if (_playerCount < 2) EndGame();
    }

    private void StartGame()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var spawnPosition = client.ClientId == 0
                ? player1SpawnPosition.GetRandomSpawnPosition()
                : player2SpawnPosition.GetRandomSpawnPosition();
            var player = Instantiate(NetworkManager.Singleton.NetworkConfig.PlayerPrefab, spawnPosition,
                Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(client.ClientId);
            _networkStatsUI.UpdatePort();
        }
    }

    private void EndGame()
    {
        // Implement game end logic here
    }
}
