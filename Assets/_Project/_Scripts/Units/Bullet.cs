using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace _Project
{
    public class Bullet : NetworkBehaviour
    {
        public float speed = 20f;
        public int damage;
        private Vector3 _targetPoint;

        public void Initialize(Vector3 targetPoint, int damage)
        {
            _targetPoint = targetPoint;
            this.damage = damage;
        }

        private void Update()
        {
            if (IsServer)
            {
                var step = speed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, _targetPoint, step);

                if (Vector3.Distance(transform.position, _targetPoint) < 0.1f)
                {
                    Destroy(gameObject);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsServer && other.CompareTag("Player"))
            {
                var playerStats = other.GetComponent<PlayerNetworkStats>();
                if (playerStats != null)
                {
                    playerStats.TakeDamageServerRpc(damage);
                }
                Destroy(gameObject);
            }
        }
    }
}