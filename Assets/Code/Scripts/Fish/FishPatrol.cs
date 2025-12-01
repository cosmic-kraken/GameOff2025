using UnityEngine;
using UnityEngine.AI;

public class FishPatrol : MonoBehaviour
{
    [SerializeField] private Transform[] PatrolPoints;
    
    [SerializeField] private int CurrentPatrolIndex = 0;

    private NavMeshAgent _agent;

    private int minScale = 2;
    private int maxScale = 4;

    void Start()
    {
        // Randomize scale
        int scaleValue = Random.Range(minScale, maxScale + 1);
        transform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);

        if (PatrolPoints == null || PatrolPoints.Length == 0)
        {
            Debug.LogError("No patrol points assigned.");
            return;
        }
        _agent = GetComponent<NavMeshAgent>();
        if (_agent == null)
        {
            Debug.LogError("No NavMeshAgent component found.");
            return;
        }
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;

        // Slightly randomize starting position
        float yOffset = Random.Range(-0.5f, 0.5f);

        Vector3 startPos = PatrolPoints[CurrentPatrolIndex].position;
        startPos.y += yOffset;
        transform.position = startPos;

        // Slightly randomize agent speed
        float speedVariation = Random.Range(-0.5f, 0.5f);
        _agent.speed += speedVariation;

        // Randomize wait until first move
        float initialWait = Random.Range(0f, 2f);
        Invoke(nameof(setNextPatrolPoint), initialWait);
    }

    void Update()
    {
        UpdateRotationFromVelocity2D();

        if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
        {
            setNextPatrolPoint();
        }
    }

    private void setNextPatrolPoint()
    {
        if (PatrolPoints != null && PatrolPoints.Length > 0)
        {
            // If last point reached, teleport to start
            if (CurrentPatrolIndex == PatrolPoints.Length - 1)
            {
                transform.position = PatrolPoints[0].position;
            }

            CurrentPatrolIndex = (CurrentPatrolIndex + 1) % PatrolPoints.Length;

            Vector3 targetPos = PatrolPoints[CurrentPatrolIndex].position;
            float yOffset = Random.Range(-5f, 5f);
            targetPos.y += yOffset;

            _agent.SetDestination(targetPos);
        }
    }

    private void UpdateRotationFromVelocity2D(float rotationSpeed = 8f)
    {
        if (_agent == null)
            return;

        Vector3 velocity = _agent.velocity;
        if (velocity.sqrMagnitude <= 0.0001f)
            return;

        // Direction in XY plane, ignore Z
        Vector3 direction = new Vector3(velocity.x, velocity.y, 0f).normalized;

        // Rotate so that -X (the nose part) points towards movement direction
        Quaternion target = Quaternion.FromToRotation(Vector3.left, direction);

        // Correct for upside-down facing shark model
        if (direction.x > 0f)
        {
            target *= Quaternion.Euler(180f, 0f, 0f);
        }

        _agent.transform.rotation = Quaternion.Slerp(
            _agent.transform.rotation,
            target,
            rotationSpeed * Time.deltaTime
        );
    }


    private void OnDrawGizmosSelected()
    {
        if (PatrolPoints == null || PatrolPoints.Length == 0)
            return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < PatrolPoints.Length; i++)
        {
            if (PatrolPoints[i] != null)
            {
                Gizmos.DrawSphere(PatrolPoints[i].position, 0.2f);
                if (i > 0 && PatrolPoints[i - 1] != null)
                {
                    Gizmos.DrawLine(PatrolPoints[i - 1].position, PatrolPoints[i].position);
                }
            }
        }
        // Draw line from last to first point
        // if (PatrolPoints.Length > 1 && PatrolPoints[0] != null && PatrolPoints[PatrolPoints.Length - 1] != null)
        // {
        //     Gizmos.DrawLine(PatrolPoints[PatrolPoints.Length - 1].position, PatrolPoints[0].position);
        // }
    }
}
