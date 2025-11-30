using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

public abstract class SharkAgentActionBase : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    protected NavMeshAgent Agent;

    protected bool TryInitializeAgent(float? speed = null)
    {
        if (Self == null || Self.Value == null)
            return false;

        Agent = Self.Value.GetComponent<NavMeshAgent>();
        if (Agent == null)
            return false;

        Agent.updateRotation = false;
        Agent.updateUpAxis = false;

        if (speed.HasValue)
            Agent.speed = speed.Value;

        return true;
    }

    protected void UpdateRotationFromVelocity2D(float rotationSpeed = 8f)
    {
        if (Agent == null)
            return;

        Vector3 velocity = Agent.velocity;
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

        Agent.transform.rotation = Quaternion.Slerp(
            Agent.transform.rotation,
            target,
            rotationSpeed * Time.deltaTime
        );
    }

    protected Status CheckAgentPathState()
    {
        if (Agent == null)
            return Status.Failure;

        if (Agent.pathPending)
            return Status.Running;

        if (Agent.pathStatus == NavMeshPathStatus.PathInvalid)
            return Status.Failure;

        return Status.Success;
    }

}
