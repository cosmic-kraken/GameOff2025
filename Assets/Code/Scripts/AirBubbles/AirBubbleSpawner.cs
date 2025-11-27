using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class AirBubbleSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> bubblePrefabs;
    [SerializeField] private float spawnInterval = 0.5f;
    [SerializeField] private int poolSize = 10;
    [SerializeField] private float xSpawnRange = 0.5f;

    private Coroutine spawnCoroutine;
    private WaitForSeconds spawnWait;
    private ObjectPool<AirBubble> bubblePool;

    private void Awake() {
        bubblePool = new ObjectPool<AirBubble>(
            CreateBubble,
            OnGetBubble,
            OnReleaseBubble,
            OnDestroyBubble,
            true,
            poolSize,
            poolSize * 2
        );
    }
    
    private void OnEnable() {
        spawnWait = new WaitForSeconds(spawnInterval);
        if (spawnCoroutine != null) {
            StopCoroutine(spawnCoroutine);
        }
        spawnCoroutine = StartCoroutine(SpawnBubbles());
    }
    
    private IEnumerator SpawnBubbles() {
        while (true) {
            yield return spawnWait;

            // Spawn bubbles at the spawner's position, with a slight random X offset
            var spawnPosition = transform.position;
            spawnPosition.x += Random.Range(-xSpawnRange, xSpawnRange);
            var bubble = bubblePool.Get();
            bubble.Init(bubblePool, spawnPosition);
            bubble.gameObject.SetActive(true);
        }
    }
    
    private void OnDisable() {
        if (spawnCoroutine != null) {
            StopCoroutine(spawnCoroutine);
        }
    }

    private AirBubble CreateBubble() {
        var prefab = bubblePrefabs[Random.Range(0, bubblePrefabs.Count)];
        var obj = Instantiate(prefab, transform);
        var bubble = obj.GetComponent<AirBubble>();
        return bubble;
    }

    private void OnGetBubble(AirBubble bubble) {
        bubble.gameObject.SetActive(true);
    }

    private void OnReleaseBubble(AirBubble bubble) {
        bubble.gameObject.SetActive(false);
    }

    private void OnDestroyBubble(AirBubble bubble) {
        Destroy(bubble.gameObject);
    }
}
