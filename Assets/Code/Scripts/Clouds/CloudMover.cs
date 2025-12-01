using System.Collections.Generic;
using UnityEngine;

public class CloudMover : MonoBehaviour
{
    [Header("Cloud Prefabs")]
    [SerializeField] List<GameObject> cloudsPrefabs;
    
    [Header("Spawn Settings")]
    [SerializeField] int numberOfClouds = 10;
    [SerializeField] float minSpacing = 5f;
    [SerializeField] float maxSpacing = 15f;
    [SerializeField] float yzOffsetMin = -2f;
    [SerializeField] float yzOffsetMax = 2f;
    
    [Header("Movement Settings")]
    public float speed = 2f;
    public float resetDistance = 50f;

    private Vector3 startPos;
    private int direction = 1; // 1 = right, -1 = left
    private List<GameObject> spawnedClouds = new List<GameObject>();

    private void Start()
    {
        startPos = transform.position;
        SpawnClouds();
    }

    private void SpawnClouds()
    {
        if (cloudsPrefabs == null || cloudsPrefabs.Count == 0)
        {
            Debug.LogWarning("No cloud prefabs assigned to CloudMover!");
            return;
        }

        float currentX = startPos.x;

        for (int i = 0; i < numberOfClouds; i++)
        {
            // Random spacing
            float spacing = Random.Range(minSpacing, maxSpacing);
            currentX += spacing;

            // Random Y offset
            float yzOffset = Random.Range(yzOffsetMin, yzOffsetMax);

            // Random cloud prefab
            GameObject cloudPrefab = cloudsPrefabs[Random.Range(0, cloudsPrefabs.Count)];

            // Spawn position
            Vector3 spawnPos = new Vector3(currentX, startPos.y + yzOffset, transform.position.z + yzOffset);

            // Instantiate cloud
            GameObject cloud = Instantiate(cloudPrefab, spawnPos, Quaternion.identity, transform);
            spawnedClouds.Add(cloud);
        }
    }

    private void Update()
    {
        // Move left or right
        transform.position += Vector3.right * (direction * speed * Time.deltaTime);

        // If moving right and we've gone too far → switch to left
        if (direction == 1 && Vector3.Distance(transform.position, startPos) >= resetDistance)
        {
            direction = -1;
        }

        // If moving left and we've returned to start → switch to right
        if (direction == -1 && Vector3.Distance(transform.position, startPos) <= 0.1f)
        {
            direction = 1;
        }
    }
}
