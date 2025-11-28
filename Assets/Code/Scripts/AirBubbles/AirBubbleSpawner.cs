using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class AirBubbleSpawner : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private List<GameObject> spawnerVisualsPrefabs;
    
    [Header("Performance Settings")]
    [SerializeField] private bool onlySpawnWhenVisible = true;
    
    [Header("Spawn Settings")]
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
        
        if (spawnerVisualsPrefabs.Count > 0) {
            var visualPrefab = spawnerVisualsPrefabs[Random.Range(0, spawnerVisualsPrefabs.Count)];
            Instantiate(visualPrefab, transform);
        }
    }

    private void OnValidate() {
        spawnWait = new WaitForSeconds(spawnInterval);
    }
    
    private void OnEnable() {
        spawnWait = new WaitForSeconds(spawnInterval);
        if (!onlySpawnWhenVisible) {
            if (spawnCoroutine != null) {
                StopCoroutine(spawnCoroutine);
            }
            spawnCoroutine = StartCoroutine(SpawnBubbles());
        }
    }

    private void OnBecameVisible() {
        if (onlySpawnWhenVisible && spawnCoroutine == null) {
            spawnWait = new WaitForSeconds(spawnInterval);
            spawnCoroutine = StartCoroutine(SpawnBubbles());
        }
    }

    private void OnBecameInvisible() {
        if (onlySpawnWhenVisible && spawnCoroutine != null) {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }
    
    private IEnumerator SpawnBubbles() {
        while (true) {
            yield return spawnWait;

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
            spawnCoroutine = null;
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
