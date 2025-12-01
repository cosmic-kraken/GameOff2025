using UnityEngine;

public class JellyfishController : MonoBehaviour
{   
    public Vector3 startPosition;
    [Header("Respawn time")]
    public float respawnTime = 180f;
    [Header("Patrol Waypoints (XY positions)")]
    public Vector3[] patrolPoints;
    public float arriveDistance = 0.2f;
    private int currentPoint = 0;

    [Header("Pulse / Propulsion")]
    public float pulseStrength = 2f;
    public float pulseFrequency = 1f;

    [Header("Directional Bobbing")]
    public float bobAmplitude = 0.5f;
    public float bobFrequency = 0.5f;

    [Header("Rotation")]
    public float rotationSpeed = 5f;

    [Header("Squash & Stretch")]
    public float stretchAmount = 0.2f;   // how much it stretches on pulse peak
    public float shrinkAmount = 0.1f;    // how much it squashes on pulse low
    public float scaleSmoothing = 6f;    // smooth dampening for scaling
    private Vector3 originalScale;

    private Vector3 movementDir;

    void Start()
    {
        startPosition = transform.position;
        originalScale = transform.localScale;

        for (int i = 0; i < patrolPoints.Length; i++)
            patrolPoints[i].z = 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<TurtleController>().Heal(25f);
            if (gameObject.transform.parent != null)
            {
                gameObject.transform.parent.GetComponent<JellyfishManager>().ScheduleRespawn(gameObject, respawnTime);
                transform.position = startPosition;
                currentPoint = 0;
                gameObject.SetActive(false);
            }
            else{
                Destroy(gameObject);
            }
        }
    }

    void Update()
    {
        if (patrolPoints.Length == 0)
            return;

        float time = Time.time;
        Vector3 target = patrolPoints[currentPoint];

        //------------------------------------------------------------------
        // 1. Compute direction toward target (XY only)
        //------------------------------------------------------------------
        movementDir = (target - transform.position).normalized;
        movementDir.z = 0f;

        //------------------------------------------------------------------
        // 2. Pulse propulsion along the direction of movement
        //------------------------------------------------------------------
        float pulse = Mathf.Max(0f, Mathf.Sin(time * pulseFrequency * Mathf.PI * 2f));
        
        //------------------------------------------------------------------
        // Stretch/Squash based on pulse amplitude
        //------------------------------------------------------------------

        // pulse = 0 → minimum  
        // pulse = 1 → maximum  
        float stretch = 1f + (pulse * stretchAmount);
        float squash = 1f - ((1f - pulse) * shrinkAmount);

        // We want it to stretch UP, squash DOWN
        Vector3 targetScale = new Vector3(
            originalScale.x * squash,     // slightly narrower on strong pulse
            originalScale.y * stretch,    // taller on strong pulse
            originalScale.z
        );

        // Smooth scaling
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.deltaTime * scaleSmoothing
        );
        Vector3 pulseMovement = movementDir * pulse * pulseStrength * Time.deltaTime;

        //------------------------------------------------------------------
        // 3. Directional bobbing (this moves ALONG the forward direction)
        //------------------------------------------------------------------
        float bob = Mathf.Sin(time * bobFrequency * Mathf.PI * 2f) * bobAmplitude;
        Vector3 bobMovement = movementDir * bob * Time.deltaTime;

        //------------------------------------------------------------------
        // 4. Apply final movement (only X & Y)
        //------------------------------------------------------------------
        Vector3 newPos = transform.position + pulseMovement + bobMovement;
        newPos.z = 0f;
        transform.position = newPos;

        //------------------------------------------------------------------
        // 5. Smooth rotation to face forward (XY plane)
        //------------------------------------------------------------------
        if (movementDir.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(movementDir.y, movementDir.x) * Mathf.Rad2Deg;
            Quaternion targetRot = Quaternion.Euler(0f, 0f, angle - 90f); 
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        //------------------------------------------------------------------
        // 6. Move to next waypoint when close
        //------------------------------------------------------------------
        if (Vector3.Distance(transform.position, target) < arriveDistance)
            currentPoint = (currentPoint + 1) % patrolPoints.Length;
    }
}