using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace _Project
{
    public class Bullet : NetworkBehaviour
    {
        public float speed = 20f;
        public int damage = 10;
        public float lifetime = 3f;
        
        private void Start()
        {
            if (IsServer)
            {
                StartCoroutine(DestroyAfterLifetime());
            }
        }
        
        // private void Update()
        // {
        //     if (IsServer)
        //     {
        //         transform.Translate(Vector3.left * speed * Time.deltaTime);
        //     }
        // }

        // private void OnTriggerEnter(Collider other)
        // {
        //     if (IsServer && other.CompareTag("Player"))
        //     {
        //         var playerStats = other.GetComponent<PlayerNetworkStats>();
        //         if (playerStats != null)
        //         {
        //             playerStats.TakeDamageServerRpc(damage);
        //         }
        //         Destroy(gameObject);
        //     }
        // }

        private IEnumerator DestroyAfterLifetime()
        {
            yield return new WaitForSeconds(lifetime);
            Destroy(gameObject);
        }
    }
}