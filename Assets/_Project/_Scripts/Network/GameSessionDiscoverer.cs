using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

    public void StartDiscovery()
    {
        Dispose();
        InitializeUdpClient();
    }

    public void StopDiscovery()
    {
        Dispose();
    }

    private void InitializeUdpClient()
    {
        _udpClient = new UdpClient
        {
            Client = { ExclusiveAddressUse = false }
        };
        _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 7788));
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

                ReadMessage(message);
            }
            catch (Exception ex)
            {
                if (!_isDisposed) Debug.LogError($"UDP Receive Error: {ex.Message}");
            }

            PruneStaleSessions();
        }
    }

    private void ReadMessage(string message)
    {
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

    public void DisposeUdpClient()
    {
        if (_udpClient != null)
        {
            _udpClient.Close();
            _udpClient = null;
            Debug.Log("GameSessionDiscoverer: UdpClient disposed.");
        }
    }
}
