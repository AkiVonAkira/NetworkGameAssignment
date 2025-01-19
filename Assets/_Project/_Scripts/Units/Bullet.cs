using Unity.Netcode;
using UnityEngine;

namespace _Project
{
    public class Bullet : NetworkBehaviour
    {
        public float speed = 20f;
        public int damage = 10;

        private void Update()
        {
            if (IsServer)
            {
                transform.Translate(Vector3.forward * speed * Time.deltaTime);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsServer && other.CompareTag("Player"))
            {
                var playerStats = other.GetComponent<PlayerNetworkStats>();
                if (playerStats != null)
                {
                    playerStats.health.Value -= damage;
                }
                Destroy(gameObject);
            }
        }
        
        public void SetDamage(int newDamage)
        {
            damage = newDamage;
        }
    }
}