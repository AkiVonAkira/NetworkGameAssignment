﻿using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class GameSessionAnnouncer : MonoBehaviour, IDisposable
{
    private const string GameSessionIdentifier = "1v1ShooterGameGameSession";
    private IPEndPoint _broadcastEndPoint;
    private bool _isDisposed;
    private string _lobbyName;
    private int _port;
    private UdpClient _udpClient;

    private void OnApplicationQuit()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _isDisposed = true;
        CancelInvoke(nameof(BroadcastGameSession));
        DisposeUdpClient();
    }

    public void Initialize(int port, string lobbyName)
    {
        _port = port;
        _lobbyName = lobbyName;
    }

    public void InitializeUdpClient()
    {
        _udpClient = new UdpClient { EnableBroadcast = true };
        _broadcastEndPoint = new IPEndPoint(IPAddress.Broadcast, 7788);

        // Start broadcasting every 1 second
        InvokeRepeating(nameof(BroadcastGameSession), 1f, 1f);
    }

    private void BroadcastGameSession()
    {
        if (_isDisposed) return;

        if (_lobbyName == null || _port == 0)
        {
            Debug.LogError("Lobby name or port is not set.");
            return;
        }

        try
        {
            var message = $"{GameSessionIdentifier}|{_lobbyName}|{_port}";
            var data = Encoding.UTF8.GetBytes(message);
            _udpClient.Send(data, data.Length, _broadcastEndPoint);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error broadcasting game session: {ex.Message}");
        }
    }

    public void DisposeUdpClient()
    {
        if (_udpClient != null)
        {
            _udpClient.Close();
            _udpClient = null;
            Debug.Log("GameSessionAnnouncer: UdpClient disposed.");
        }
    }
}