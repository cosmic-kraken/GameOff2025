using UnityEngine;

public class SharkDetectionVisual : MonoBehaviour
{
    public Transform SharkEyePosition;
    private float _detectionRange;
    private float _chaseDetectionRange;

    private Color _detectionColor = Color.red;
    private Color _chaseDetectionColor = Color.green;


    void Start()
    {
        var behaviorGraphAgent = GetComponent<Unity.Behavior.BehaviorGraphAgent>();
        if (behaviorGraphAgent != null)
        {
            var blackboard = behaviorGraphAgent.BlackboardReference.Blackboard;
            foreach (var variable in blackboard.Variables)
            {
                if (variable == null)
                    continue;

                if (variable.Name == "DetectionRange")
                    _detectionRange = (float)variable.ObjectValue;
                
                if (variable.Name == "ChaseDetectionRange")
                    _chaseDetectionRange = (float)variable.ObjectValue;
            }

            // Debug.Log($"Shark Detection Range: {_detectionRange}");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = _detectionColor;
        Gizmos.DrawWireSphere(SharkEyePosition.position, _detectionRange);

        Gizmos.color = _chaseDetectionColor;
        Gizmos.DrawWireSphere(SharkEyePosition.position, _chaseDetectionRange);
    }

}
