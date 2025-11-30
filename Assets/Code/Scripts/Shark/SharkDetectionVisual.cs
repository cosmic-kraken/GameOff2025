using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

public class SharkDetectionVisual : MonoBehaviour
{
    [SerializeField]
    public Transform SharkEyePosition;
    
    private List<Vector3> _patrolPoints;

    private float _detectionRange;
    private float _chaseDetectionRange;

    private Color _detectionColor = Color.red;
    private Color _chaseDetectionColor = Color.green;
    private Color _patrolColor = Color.blue;
    private BehaviorGraphAgent _behaviorGraphAgent;

    void Start()
    {
        _behaviorGraphAgent = GetComponent<Unity.Behavior.BehaviorGraphAgent>();
    }

    void Update()
    {
        runtimePullBehavior();
    }

    private void runtimePullBehavior()
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
