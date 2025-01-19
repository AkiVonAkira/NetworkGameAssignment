using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace _Project
{
    public class PlayerNetworkGun : NetworkBehaviour
    {
        // stats for gun damage, current and max ammo capacity, reload time, etc.
        public NetworkVariable<int> damage = new(10);
        public NetworkVariable<int> maxAmmo = new(30);
        public NetworkVariable<int> ammo = new(30);
        public NetworkVariable<float> reloadTime = new(1.5f);

        [SerializeField] private GameObject gun;
        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private AudioSource gunShotSound;

        private bool _canFire = true;

        private InputSystem _inputSystem;
        private Animator _animator;
    
        private static readonly int Fire = Animator.StringToHash("Fire");
    
        private void Start()
        {
            _inputSystem = FindFirstObjectByType<InputSystem>();
            _animator = gun.GetComponent<Animator>();
        }
    
        // Update is called once per frame
        void Update()
        {
            if (IsOwner && _inputSystem.fire)
            {
                ShootServerRpc();
            }
            
            if (IsOwner && _inputSystem.reload)
            {
                ReloadServerRpc();
            }
        }

        [ServerRpc]
        private void ShootServerRpc()
        {
            if (ammo.Value > 0 && _canFire)
            {
                _canFire = false;
                ammo.Value--;
                var bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
                bullet.GetComponent<Bullet>().SetDamage(damage.Value);
                bullet.GetComponent<NetworkObject>().Spawn();
                ShootClientRpc();
            }
        }

        [ClientRpc]
        private void ShootClientRpc()
        {
            StartCoroutine(FiringGun());
        }

        [ServerRpc]
        private void ReloadServerRpc()
        {
            StartCoroutine(Reload());
        }

        IEnumerator FiringGun()
        {
            gunShotSound.Play();
            _animator.SetBool(Fire, true);
            yield return new WaitForSeconds(0.5f);
            _animator.SetBool(Fire, false);
            yield return new WaitForSeconds(0.1f);
            _canFire = true;
        }

        private IEnumerator Reload()
        {
            yield return new WaitForSeconds(reloadTime.Value);
            ammo.Value = maxAmmo.Value;
        }
    }
}