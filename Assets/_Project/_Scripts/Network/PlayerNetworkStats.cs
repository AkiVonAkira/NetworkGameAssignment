using Unity.Netcode;
using UnityEngine;

namespace _Project
{
    public class PlayerNetworkStats : NetworkBehaviour
    {
        public NetworkVariable<int> maxHealth = new(100);
        public NetworkVariable<int> health = new(100);

        public delegate void PlayerDiedHandler(ulong playerId);
        public static event PlayerDiedHandler OnPlayerDied;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                health.Value = maxHealth.Value;
            }

            if (IsOwner)
            {
                health.OnValueChanged += OnHealthChanged;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (!IsOwner) return;

            health.OnValueChanged -= OnHealthChanged;
        }

        private void OnHealthChanged(int oldValue, int newValue)
        {
            Debug.Log($"Health changed to: {newValue.ToString()}");
            if (newValue <= 0)
            {
                Debug.Log("Player died");
                OnPlayerDied?.Invoke(NetworkManager.Singleton.LocalClientId);
                GameManager.Instance.EndGame();
            }
        }

        [Rpc(SendTo.Server)]
        public void TakeDamageServerRpc(int damage)
        {
            if (!IsServer) return;
            health.Value -= damage;
        }

        [ClientRpc]
        private void UpdateHealthClientRpc(int newHealth)
        {
            health.Value = newHealth;
        }
    }
}