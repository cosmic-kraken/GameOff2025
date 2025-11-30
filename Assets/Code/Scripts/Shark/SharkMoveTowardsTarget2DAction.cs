using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SharkMoveTowardsTarget2D", story: "Shark moves [Self] towards [TargetPosition] in 2D using speed [SwimSpeed]", category: "Action", id: "3ad2042d8c0ab12e94362c67615d8e96")]
public partial class SharkMoveTowardsTarget2DAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Vector3> TargetPosition;
    [SerializeReference] public BlackboardVariable<float> SwimSpeed;

    private Rigidbody2D _rb;

    protected override Status OnStart()
    {
        if (Self.Value != null)
        {
            _rb = Self.Value.GetComponent<Rigidbody2D>();
        }
        return _rb != null ? Status.Running : Status.Failure;
    }

    protected override Status OnUpdate()
    {
        if (_rb == null) return Status.Failure;

        Vector2 currentPos = _rb.position;
        Vector2 targetPos = TargetPosition.Value;
        Vector2 dir = (targetPos - currentPos);

        // Arrived?
        if (dir.sqrMagnitude < 0.01f)
            return Status.Success;

        dir = dir.normalized;
        Vector2 newPos = currentPos + dir * SwimSpeed.Value * Time.deltaTime;
        _rb.MovePosition(newPos);

        // Optional: flip sprite based on dir.x
        if (dir.x != 0f)
        {
            var tf = _rb.transform;
            var scale = tf.localScale;
            scale.x = Mathf.Sign(dir.x) * Mathf.Abs(scale.x);
            tf.localScale = scale;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
    }
}

