using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerStatsUI : NetworkBehaviour
{
    public PlayerNetworkStats playerNetworkStats;
    public TextMeshProUGUI maxHealthText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI maxAmmoText;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI playerNumberText;
    public Canvas playerCanvas;

    private void Awake()
    {
        if (playerNetworkStats == null) enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (!IsOwner)
        {
            playerCanvas.enabled = false;
            return;
        }

        playerNetworkStats.health.OnValueChanged += OnHealthValueChanged;
        SetPlayerNumberText();
        playerCanvas.enabled = true;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (!IsOwner) return;
        playerNetworkStats.health.OnValueChanged -= OnHealthValueChanged;
    }

    private void OnHealthValueChanged(int previousValue, int newValue)
    {
        // Implement a visual UI change here with the UI variable
    }

    private void SetPlayerNumberText()
    {
        var playerNumber = (int)NetworkManager.Singleton.LocalClientId + 1;
        playerNumberText.text = "Player  " + playerNumber;
    }
}