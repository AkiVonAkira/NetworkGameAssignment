using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace _Project
{
    public class PlayerNetworkGun : NetworkBehaviour
    {
        public NetworkVariable<int> damage = new(10);
        public NetworkVariable<int> maxAmmo = new(30);
        public NetworkVariable<int> ammo = new(30);
        public NetworkVariable<float> reloadTime = new(2.5f);

        [SerializeField] private GameObject gun;
        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject bulletImpactPrefab;
        [SerializeField] private AudioSource gunShotSound;
        
        private bool _canFire = true;
        private InputSystem _inputSystem;
        private Animator _animator;
        private Camera _playerCamera;
    
        private static readonly int Fire = Animator.StringToHash("Fire");
    
        private void Start()
        {
            _inputSystem = FindFirstObjectByType<InputSystem>();
            _animator = gun.GetComponent<Animator>();
            _playerCamera = Camera.main;
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

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (!IsOwner) return;

            ammo.OnValueChanged += OnAmmoChanged;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();

            if (!IsOwner) return;

            ammo.OnValueChanged -= OnAmmoChanged;
        }

        private void OnAmmoChanged(int oldValue, int newValue)
        {
            if (IsOwner && newValue <= 0)
            {
                ReloadServerRpc();
            }
        }

        [Rpc(SendTo.Server)]
        private void ShootServerRpc()
        {
            if (!IsServer) return;
            
            if (ammo.Value > 0 && _canFire)
            {
                _canFire = false;
                ammo.Value--;

                Ray ray = _playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        var playerStats = hit.collider.GetComponent<PlayerNetworkStats>();
                        if (playerStats != null)
                        {
                            playerStats.TakeDamageServerRpc(damage.Value);
                        }
                    }

                    var bulletImpact = Instantiate(bulletImpactPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    bulletImpact.GetComponent<NetworkObject>().Spawn();
                }

                ShootClientRpc();
            }
        }

        [ClientRpc]
        private void ShootClientRpc()
        {
            StartCoroutine(FiringGun());
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

        [Rpc(SendTo.Server)]
        private void ReloadServerRpc()
        {
            if (!IsServer) return;
            
            StartCoroutine(Reload());
        }

        private IEnumerator Reload()
        {
            yield return new WaitForSeconds(reloadTime.Value);
            ammo.Value = maxAmmo.Value;
        }
    }
}