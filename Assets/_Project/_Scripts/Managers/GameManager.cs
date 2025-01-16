using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public PlayerSpawnPosition player1SpawnPosition;
    public PlayerSpawnPosition player2SpawnPosition;

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
        if (NetworkManager.Singleton != null && IsServer)
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

        _networkManagerUI.StopDiscovering();
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
        _networkManagerUI.StopBroadcasting();
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
    }

    private void EndGame()
    {
        // Implement game end logic here
    }
}
