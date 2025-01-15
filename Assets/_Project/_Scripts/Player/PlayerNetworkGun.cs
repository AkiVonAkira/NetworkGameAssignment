using Unity.Netcode;
using UnityEngine;

namespace _Project
{
    public class PlayerNetworkGun : NetworkBehaviour
    {
        // stats for gun damage, current and max ammo capacity, reload time, etc.
        public NetworkVariable<int> Damage = new(10);
        public NetworkVariable<int> MaxAmmo = new(30);
        public NetworkVariable<int> Ammo = new(30);
        public NetworkVariable<float> ReloadTime = new(1.5f);

        // reference to the gun object
        [SerializeField] private GameObject gun;

        // reference to the gun muzzle
        [SerializeField] private Transform gunMuzzle;
    }
}
