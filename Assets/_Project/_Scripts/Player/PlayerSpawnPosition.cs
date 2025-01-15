using UnityEngine;

public class PlayerSpawnPosition : MonoBehaviour
{
    public float spawnRangeX = 8f;
    public float spawnRangeZ = 1f;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnRangeX * 2, 0.1f, spawnRangeZ * 2));
    }

    public Vector3 GetRandomSpawnPosition()
    {
        var randomX = Random.Range(-spawnRangeX, spawnRangeX);
        var randomZ = Random.Range(-spawnRangeZ, spawnRangeZ);

        return new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);
    }
}