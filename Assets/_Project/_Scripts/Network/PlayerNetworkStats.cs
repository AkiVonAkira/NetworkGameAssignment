using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkStats : NetworkBehaviour
{
    public NetworkVariable<int> maxHealth = new(100);
    public NetworkVariable<int> health = new(100);
    public NetworkVariable<int> ammo = new(30);

    private void Update()
    {
        if (IsOwner && Input.GetKeyDown(KeyCode.R)) ReloadAmmoServerRpc();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner) return;

        health.Value = maxHealth.Value;
        health.OnValueChanged += OnHealthChanged;
        ammo.OnValueChanged += OnAmmoChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsOwner) return;

        health.OnValueChanged -= OnHealthChanged;
        ammo.OnValueChanged -= OnAmmoChanged;
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        Debug.Log($"Health changed to: {newValue.ToString()}");
    }

    private void OnAmmoChanged(int oldValue, int newValue)
    {
        Debug.Log($"Ammo changed to: {newValue.ToString()}");
    }

    [ServerRpc]
    private void ReloadAmmoServerRpc()
    {
        ammo.Value = 30;
    }
}
