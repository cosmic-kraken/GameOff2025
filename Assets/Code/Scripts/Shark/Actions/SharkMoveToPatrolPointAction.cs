using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;
using System.Collections.Generic;
using Unity.VisualScripting;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SharkMoveToPatrolPoint", story: "Move [Self] NavMeshAgent towards [CurrentPatrolIndex] from [PatrolPoints] with [SwimSpeed]", category: "Action", id: "e129c8c03887fa34695a20de77fa9bce")]
public partial class SharkMoveToPatrolPointAction : SharkAgentActionBase
{
    [SerializeReference] public BlackboardVariable<int> CurrentPatrolIndex;
    [SerializeReference] public BlackboardVariable<List<Vector3>> PatrolPoints;
    [SerializeReference] public BlackboardVariable<float> SwimSpeed;
    [SerializeReference] public BlackboardVariable<GameObject> Player;
    [SerializeReference] public BlackboardVariable<float> DetectionRange;
    [SerializeReference] public BlackboardVariable<float> InitialDetectionRange;

    private Vector3 _targetPosition;

    protected override Status OnStart()
    {
        if (!TryInitializeAgent(SwimSpeed?.Value))
            return Status.Failure;

        var points = PatrolPoints?.Value;
        if (points == null || points.Count == 0)
            return Status.Failure;

        int index = CurrentPatrolIndex.Value;
        if (index < 0 || index >= points.Count)
            index = 0;

        _targetPosition = points[index];
        CurrentPatrolIndex.Value = index;

        Agent.SetDestination(_targetPosition);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Status pathStatus = CheckAgentPathState();
        if (pathStatus == Status.Failure)
            return Status.Failure;

        // If pathPending, just keep running
        if (pathStatus == Status.Running)
        {
            UpdateRotationFromVelocity2D();
            return Status.Running;
        }

        UpdateRotationFromVelocity2D();

        if (Agent.remainingDistance <= Agent.stoppingDistance + 0.05f)
        {
            setNextPatrolPoint();
            return Status.Success;
        }

        return Status.Success;
    }

    protected override void OnEnd()
    {
    }

    private void setNextPatrolPoint()
    {
        var points = PatrolPoints?.Value;
        if (points != null && points.Count > 0)
        {
            int index = CurrentPatrolIndex.Value;
            index = (index + 1) % points.Count;
            CurrentPatrolIndex.Value = index;
        }
        
        DetectionRange.Value = InitialDetectionRange.Value;
    }
}