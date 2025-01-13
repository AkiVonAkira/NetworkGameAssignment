using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;

public class PlayerNetworkStats : NetworkBehaviour
{
    public NetworkVariable<int> XP = new NetworkVariable<int>(0);
    public NetworkVariable<int> Health = new NetworkVariable<int>(100);
    public NetworkVariable<int> Strength = new(1);
    public NetworkVariable<int> Dexterity = new(1);
    public NetworkVariable<int> Rizz = new(0);
    public NetworkVariable<float> Charisma = new(0f);

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();

        if (!IsOwner) return;

        if (Rizz.Value < 15)
        {
            Debug.Log("Rizz is too low");
        }

        Rizz.OnValueChanged += OnRizzValueChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        
        if (!IsOwner) return;

        Rizz.OnValueChanged -= OnRizzValueChanged;
    }

    void Update() {
        if (IsOwner && Input.GetKeyDown(KeyCode.H)) {
            ChangeRizzServerRpc();
        }
    }

    public void OnRizzValueChanged(int oldValue, int newValue)
    {
        Debug.Log("NewValue of Rizz: " + newValue);
    }

    [ServerRpc]
    private void ChangeRizzServerRpc() {
        // This is the server doing things

        Rizz.Value = 15;
    }


    // === REFERENCE CODE ===
    UnityAction DoSomethingAction;
    System.Action Action;

    void CallInvoke() {
        DoSomethingAction?.Invoke();
    }

    void SubscribeToCall() {
        DoSomethingAction += () => {};
    }

}
