using UnityEngine;
using Unity.Netcode;

public class PlayerStatsUI : NetworkBehaviour
{
    public PlayerNetworkStats playerNetworkStats;
    public GameObject ui;

    void Awake()
    {
        if (playerNetworkStats == null) {
            enabled = false;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner) return;
        playerNetworkStats.Rizz.OnValueChanged += OnRizzValueChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (!IsOwner) return;
        playerNetworkStats.Rizz.OnValueChanged -= OnRizzValueChanged;
    }

    private void OnRizzValueChanged(int previousValue, int newValue)
    {
        // Implement a visual UI change here with the UI variable
    }
}
