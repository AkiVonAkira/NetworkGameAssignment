using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class GameSessionDiscoverer : MonoBehaviour, IDisposable
{
    private const string GameSessionIdentifier = "1v1ShooterGameGameSession";

    private readonly Dictionary<string, DateTime> _activeSessions = new();
    private readonly TimeSpan _sessionTimeout = TimeSpan.FromSeconds(30);
    private bool _isDisposed;

    private UdpClient _udpClient;

    private void OnApplicationQuit()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _isDisposed = true;
        DisposeUdpClient();
    }

    public event Action<string, int> OnGameSessionDiscovered;

    public void RefreshReceiver()
    {
        DisposeUdpClient();
        InitializeUdpClient();
    }

    private void InitializeUdpClient()
    {
        _udpClient = new UdpClient(7788);
        _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        StartListening();
    }

    private void PruneStaleSessions()
    {
        var now = DateTime.UtcNow;
        foreach (var session in _activeSessions.Keys.ToList())
            if (now - _activeSessions[session] > _sessionTimeout)
            {
                _activeSessions.Remove(session);
                Debug.Log($"Session {session} pruned due to timeout.");
            }
    }

    private async void StartListening()
    {
        while (!_isDisposed)
        {
            try
            {
                var result = await _udpClient.ReceiveAsync();
                var message = Encoding.UTF8.GetString(result.Buffer);

                var parts = message.Split('|');
                if (parts.Length == 3 && parts[0] == GameSessionIdentifier)
                {
                    var key = $"{parts[1]}|{parts[2]}"; // Unique key: lobbyName|port
                    if (!_activeSessions.ContainsKey(key))
                    {
                        _activeSessions[key] = DateTime.UtcNow;
                        var lobbyName = parts[1];
                        var port = int.Parse(parts[2]);
                        OnGameSessionDiscovered?.Invoke(lobbyName, port);
                    }
                    else
                    {
                        // Update the timestamp for the session
                        _activeSessions[key] = DateTime.UtcNow;
                    }
                }
            }
            catch (Exception ex)
            {
                if (!_isDisposed) Debug.LogError($"UDP Receive Error: {ex.Message}");
            }

            PruneStaleSessions();
        }
    }

    private void DisposeUdpClient()
    {
        if (_udpClient != null)
        {
            _udpClient.Close();
            _udpClient = null;
            Debug.Log("GameSessionDiscoverer: UdpClient disposed.");
        }
    }
}
