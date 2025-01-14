using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkStats : NetworkBehaviour
{
    public NetworkVariable<int> MaxHealth = new(100);
    public NetworkVariable<int> Health = new(100);
    public NetworkVariable<int> Ammo = new(30);

    private void Update()
    {
        if (IsOwner && Input.GetKeyDown(KeyCode.R)) ReloadAmmoServerRpc();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner) return;

        Health.Value = MaxHealth.Value;
        Health.OnValueChanged += OnHealthChanged;
        Ammo.OnValueChanged += OnAmmoChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (!IsOwner) return;

        Health.OnValueChanged -= OnHealthChanged;
        Ammo.OnValueChanged -= OnAmmoChanged;
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        Debug.Log("Health changed to: " + newValue);
    }

    private void OnAmmoChanged(int oldValue, int newValue)
    {
        Debug.Log("Ammo changed to: " + newValue);
    }

    [ServerRpc]
    private void ReloadAmmoServerRpc()
    {
        Ammo.Value = 30;
    }
}
