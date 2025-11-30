using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TrashSpawner : MonoBehaviour
{
    public static Action<int> OnTrashSpawned;
    
    [Header("Spawn Settings")]
    [SerializeField] private List<GameObject> trashPrefabs;
    [SerializeField] private float easternXLimit = 20f;
    [SerializeField] private float westernXLimit = -10f;
    [SerializeField] private float northernYLimit = 20f;
    [SerializeField] private float southernYLimit = -50f;
    [SerializeField] private float zSpawnPosition = 0f;
    [SerializeField] private int numberOfTrashToSpawn = 10;
    
    [Header("Collision Check Settings")]
    [SerializeField] private float _checkRadius = 0.5f; 
    [SerializeField] private bool _limitSpawnAttempts = false;
    [SerializeField] private int _maxSpawnAttempts = 30; 
    [SerializeField] private LayerMask _obstacleLayer = -1; 
    
    private List<GameObject> spawnedTrash = new();
    private Collider[] positionCheckResults = new Collider[10];

    private void Awake() {
        SpawnTrash();
    }

    private void SpawnTrash() {
        if (trashPrefabs == null || trashPrefabs.Count == 0)
        {
            Debug.LogError("TrashSpawner: No trash objects assigned to spawner.");
            return;
        }

        int totalAttempts = 0;
        int maxTotalAttempts = _limitSpawnAttempts ? numberOfTrashToSpawn * _maxSpawnAttempts : int.MaxValue;
        
        while (spawnedTrash.Count < numberOfTrashToSpawn && totalAttempts < maxTotalAttempts)
        {
            var spawnPosition = GetRandomPosition();
            totalAttempts++;

            if (IsPositionBlocked(spawnPosition)) continue;
            
            SpawnTrashAt(spawnPosition);
        }
        
        if (spawnedTrash.Count < numberOfTrashToSpawn)
        {
            Debug.LogWarning($"TrashSpawner: Only spawned {spawnedTrash.Count}/{numberOfTrashToSpawn} trash after {totalAttempts} attempts. Scene might be too crowded or spawn area too small.");
        }
        
        // Debug.Log($"TrashSpawner: Successfully spawned {spawnedTrash.Count}/{numberOfTrashToSpawn} trash in {totalAttempts} attempts. Have fun cleaning it up.");
    }

    private void Start() {
        OnTrashSpawned?.Invoke(spawnedTrash.Count);
    }

    private Vector3 GetRandomPosition() {
        float randomX = Random.Range(westernXLimit, easternXLimit);
        float randomY = Random.Range(southernYLimit, northernYLimit);
        return new Vector3(randomX, randomY, zSpawnPosition);
    }

    private bool IsPositionBlocked(Vector3 position) {
        return Physics.OverlapSphereNonAlloc(position, _checkRadius, positionCheckResults, _obstacleLayer) > 0;
    }

    private void SpawnTrashAt(Vector3 position) {
        GameObject trashPrefab = trashPrefabs[Random.Range(0, trashPrefabs.Count)];
        GameObject newTrash = Instantiate(trashPrefab, position, Quaternion.identity, transform);
        spawnedTrash.Add(newTrash);
    }

    
#if UNITY_EDITOR
    [ContextMenu("Respawn All Trash")]
    private void RespawnAllTrash() {
        ClearSpawnedTrash();
        SpawnTrash();
    }

    [ContextMenu("Clear All Trash")]
    private void ClearSpawnedTrash() {
        foreach (var trash in spawnedTrash)
        {
            if (trash != null)
            {
                Destroy(trash);
            }
        }
        spawnedTrash.Clear();
    }
    
    private void OnDrawGizmos() {
        // Draw spawn boundaries
        Gizmos.color = Color.yellow;
        Vector3 topLeft = new Vector3(westernXLimit, northernYLimit, zSpawnPosition);
        Vector3 topRight = new Vector3(easternXLimit, northernYLimit, zSpawnPosition);
        Vector3 bottomRight = new Vector3(easternXLimit, southernYLimit, zSpawnPosition);
        Vector3 bottomLeft = new Vector3(westernXLimit, southernYLimit, zSpawnPosition);

        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
        
        // Draw check radius visualization at center
        Gizmos.color = Color.cyan;
        Vector3 centerPosition = new Vector3(
            (easternXLimit + westernXLimit) / 2f,
            (northernYLimit + southernYLimit) / 2f,
            zSpawnPosition
        );
        Gizmos.DrawWireSphere(centerPosition, _checkRadius);
    }
#endif
}

