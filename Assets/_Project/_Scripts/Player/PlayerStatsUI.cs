using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace _Project
{
    public class PlayerStatsUI : NetworkBehaviour
    {
        [SerializeField] private PlayerNetworkStats playerNetworkStats;
        [SerializeField] private PlayerNetworkGun playerNetworkGun;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI ammoText;
        [SerializeField] private TextMeshProUGUI playerNumberText;
        [SerializeField] private Canvas playerCanvas;

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
            playerNetworkGun.ammo.OnValueChanged += OnAmmoValueChanged;
            SetPlayerNumberText();
            UpdateHealthText(playerNetworkStats.health.Value);
            UpdateAmmoText(playerNetworkGun.ammo.Value);
            playerCanvas.enabled = true;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (!IsOwner) return;
            playerNetworkStats.health.OnValueChanged -= OnHealthValueChanged;
            playerNetworkGun.ammo.OnValueChanged -= OnAmmoValueChanged;
        }

        private void OnHealthValueChanged(int previousValue, int newValue)
        {
            UpdateHealthText(newValue);
        }

        private void OnAmmoValueChanged(int previousValue, int newValue)
        {
            UpdateAmmoText(newValue);
        }

        private void UpdateHealthText(int health)
        {
            healthText.text = $"Health: {health}/{playerNetworkStats.maxHealth.Value}";
        }

        private void UpdateAmmoText(int ammo)
        {
            ammoText.text = $"Ammo: {ammo}/{playerNetworkGun.maxAmmo.Value}";
        }

        private void SetPlayerNumberText()
        {
            var playerNumber = (int)NetworkManager.Singleton.LocalClientId + 1;
            playerNumberText.text = "Player  " + playerNumber;
        }
    }
}