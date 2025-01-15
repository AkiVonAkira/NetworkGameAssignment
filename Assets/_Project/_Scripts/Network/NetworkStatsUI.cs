using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkStatsUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private TextMeshProUGUI pingText;
    [SerializeField] private TextMeshProUGUI portText;
    private ushort _cachedPort;

    private float _deltaTime;

    private void Start()
    {
        UpdatePort();
    }

    private void Update()
    {
        if (NetworkManager.Singleton != null &&
            (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)) UpdatePing();
        UpdateFPS();
    }

    private void UpdatePing()
    {
        if (NetworkManager.Singleton != null)
        {
            // Get the current ping
            var ping = NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(NetworkManager.Singleton
                .LocalClientId);
            pingText.text = ping != 0 ? $"Ping: {ping}ms" : "";
        }
    }

    private void UpdateFPS()
    {
        _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
        var fps = 1.0f / _deltaTime;
        fpsText.text = $"FPS: {Mathf.Ceil(fps)}";
    }

    public void UpdatePort()
    {
        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
        {
            _cachedPort = NetworkManager.Singleton.GetComponent<UnityTransport>().ConnectionData.Port;
            portText.text = _cachedPort != 0 ? $"Port: {_cachedPort}" : "";
        }
    }
}
