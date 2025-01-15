using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class GameSessionDiscoverer : MonoBehaviour, IDisposable
{
    public event Action<string, int> OnGameSessionDiscovered;

    private UdpClient _udpClient;
    private bool _isDisposed;

    private const string GameSessionIdentifier = "1v1ShooterGameGameSession";

    private void InitializeUdpClient()
    {
        _udpClient = new UdpClient(7788);
        _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        StartListening();
    }

    public void RefreshReceiver()
    {
        DisposeUdpClient();
        InitializeUdpClient();
    }

    private async void StartListening()
    {
        while (!_isDisposed)
        {
            try
            {
                var result = await _udpClient.ReceiveAsync();
                string message = Encoding.UTF8.GetString(result.Buffer);
                Debug.Log($"Received: {message} from {result.RemoteEndPoint}");

                // Parse the message
                var parts = message.Split('|');
                if (parts.Length == 3 && parts[0] == GameSessionIdentifier)
                {
                    var lobbyName = parts[1];
                    var port = int.Parse(parts[2]);
                    OnGameSessionDiscovered?.Invoke(lobbyName, port);
                }
            }
            catch (ObjectDisposedException)
            {
                Debug.LogWarning("UdpClient has been disposed.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"UDP Receive Error: {ex.Message}");
            }
        }
    }

    private void DisposeUdpClient()
    {
        _udpClient?.Close();
        _udpClient = null;
    }

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
}
