using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject objectToSpawn;
    public float spawnInterval = 2f;
    private bool isSpawning = true;
    private Coroutine spawnCoroutine;

    private void Start()
    {
        if (objectToSpawn == null)
        {
            Debug.LogError("objectToSpawn has not been assigned in the inspector.");
            return;
        }

        spawnCoroutine = StartCoroutine(SpawnObjects());
    }

    private IEnumerator SpawnObjects()
    {
        while (isSpawning)
        {
            Instantiate(objectToSpawn, transform.position, Quaternion.identity);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void OnDestroy()
    {
        isSpawning = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        // Null 검사를 추가하여 참조가 유효한지 확인
        if (objectToSpawn != null)
        {
            Destroy(objectToSpawn);
        }
    }
}
