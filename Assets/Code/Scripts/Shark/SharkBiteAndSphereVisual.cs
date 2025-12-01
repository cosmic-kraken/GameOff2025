using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

public class SharkDetectionVisual : MonoBehaviour
{
    [SerializeField]
    public Transform SharkEyePosition;

    [SerializeField]
    private ParticleSystem _biteEffect;

    [SerializeField]
    private int lineSegments = 64;

    [SerializeField]
    private Material normalMaterial;
    [SerializeField]
    private Material alertMaterial;


    private List<Vector3> _patrolPoints;

    private float _detectionRange;
    private float _chaseDetectionRange;
    private bool _isChasing = false;

    private Color _detectionColor = Color.red;
    private Color _chaseDetectionColor = Color.green;
    private Color _patrolColor = Color.blue;
    private BehaviorGraphAgent _behaviorGraphAgent;
    private LineRenderer _lineRenderer;

    void Start()
    {
        _behaviorGraphAgent = GetComponent<BehaviorGraphAgent>();
        _lineRenderer = GetComponentInChildren<LineRenderer>();
        _biteEffect = GetComponentInChildren<ParticleSystem>();

        _lineRenderer.positionCount = lineSegments + 1;
        _lineRenderer.loop = true;
        _lineRenderer.useWorldSpace = true;
    }

    void Update()
    {
        UpdateBehaviorParameters();
    }
    void LateUpdate()
    {
        DrawCircle();
    }

    private void OnTriggerEnter(Collider other)
    {
        TurtleController turtle = other.GetComponent<TurtleController>();

        if (turtle != null)
        {
            AudioManager.Instance?.Play("Bite");

            _behaviorGraphAgent.SetVariableValue("TurtleBite", true);
            if (_biteEffect != null)
            {
                _biteEffect.Play();
            }

            turtle.InflictDamage(50f);
        }
    }
    private void DrawCircle()
    {
        float radius = _isChasing ? _chaseDetectionRange : _detectionRange;
        _lineRenderer.material = _isChasing ? alertMaterial : normalMaterial;
        
        if (radius <= 0f || _lineRenderer == null) return;

        float angleStep = 360f / lineSegments;
        Vector3 center = SharkEyePosition != null ? SharkEyePosition.position : transform.position;

        for (int i = 0; i <= lineSegments; i++)
        {
            float angle = Mathf.Deg2Rad * (i * angleStep);

            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;

            Vector3 pos = center + new Vector3(x, y, 0f);   // Circle in XY plane
            _lineRenderer.SetPosition(i, pos);
        }
    }

    private void UpdateBehaviorParameters()
    {
        if (_behaviorGraphAgent != null)
        {
            var blackboard = _behaviorGraphAgent.BlackboardReference.Blackboard;
            foreach (var variable in blackboard.Variables)
            {
                if (variable == null)
                    continue;

                if (variable.Name == "DetectionRange")
                    _detectionRange = (float)variable.ObjectValue;

                if (variable.Name == "ChaseDetectionRange")
                    _chaseDetectionRange = (float)variable.ObjectValue;

                if (variable.Name == "PatrolPoints")
                    _patrolPoints = (List<Vector3>)variable.ObjectValue;

                if (variable.Name == "IsChasing")
                    _isChasing = (bool)variable.ObjectValue;
            }

            // Debug.Log($"Shark Detection Range: {_detectionRange}");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = _detectionColor;
        Gizmos.DrawWireSphere(SharkEyePosition.position, _detectionRange);

        Gizmos.color = _chaseDetectionColor;
        Gizmos.DrawWireSphere(SharkEyePosition.position, _chaseDetectionRange);

        Gizmos.color = _patrolColor;
        if (_patrolPoints != null)
        {
            foreach (var point in _patrolPoints)
            {
                Gizmos.DrawSphere(point, 2f);
            }
        }
    }

}
