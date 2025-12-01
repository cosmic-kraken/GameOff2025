using UnityEngine;

public class CloudMover : MonoBehaviour
{
    public float speed = 2f;
    public float resetDistance = 50f;

    private Vector3 startPos;
    private int direction = 1; // 1 = right, -1 = left

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Move left or right
        transform.position += Vector3.right * direction * speed * Time.deltaTime;

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
