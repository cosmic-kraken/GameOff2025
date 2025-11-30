using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;
using System.Collections.Generic;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SharkMoveToPatrolPoint", story: "Move [Self] NavMeshAgent towards [CurrentPatrolIndex] from [PatrolPoints] with [SwimSpeed]", category: "Action", id: "e129c8c03887fa34695a20de77fa9bce")]
public partial class SharkMoveToPatrolPointAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<int> CurrentPatrolIndex;
    [SerializeReference] public BlackboardVariable<List<Vector3>> PatrolPoints;
    [SerializeReference] public BlackboardVariable<float> SwimSpeed;
    [SerializeReference] public BlackboardVariable<GameObject> Player;
    [SerializeReference] public BlackboardVariable<float> DetectionRange;


    private NavMeshAgent _agent;
    private Vector3 _targetPosition;

    protected override Status OnStart()
    {
        if (Self.Value == null)
            return Status.Failure;

        _agent = Self.Value.GetComponent<NavMeshAgent>();
        if (_agent == null)
            return Status.Failure;

        _agent.updateRotation = false;
        _agent.updateUpAxis = false;

        if (SwimSpeed != null)
            _agent.speed = SwimSpeed.Value;

        var points = PatrolPoints?.Value;
        if (points == null || points.Count == 0)
            return Status.Failure;

        int idx = CurrentPatrolIndex.Value;
        if (idx < 0 || idx >= points.Count)
            idx = 0;

        _targetPosition = points[idx];
        CurrentPatrolIndex.Value = idx;

        // Debug.Log("Moving to patrol point " + idx + " at " + _targetPosition);       

        _agent.SetDestination(_targetPosition);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_agent == null)
            return Status.Failure;

        if (Player != null && Player.Value != null && DetectionRange != null)
        {
            float dist = Vector3.Distance(
                _agent.transform.position,
                Player.Value.transform.position
            );

            Debug.Log($"Distance to player: {dist}");

            if (dist <= DetectionRange.Value)
            {
                // Do NOT advance patrol index here, we’re interrupting patrol
                Debug.Log("Player in range, breaking out of patrol");
                return Status.Failure; // or Status.Success, depending on how your graph is wired
            }
        }

        Vector3 vel = _agent.velocity;

        if (vel.sqrMagnitude > 0.0001f)
        {
            // Direction in XY plane, ignore Z
            Vector3 dir = new Vector3(vel.x, vel.y, 0f);
            dir.Normalize();

            float rotSpeed = 8f;

            // Rotate so that local LEFT (-X) (nose of shark model) points towards movement direction
            Quaternion target = Quaternion.FromToRotation(Vector3.left, dir);

            // Debug.Log(dir);
            // Add 180 degrees around X axis to account for model orientation
            if (dir.x > 0f)
            {
                target *= Quaternion.Euler(180f, 0f, 0f);
            }

            _agent.transform.rotation = Quaternion.Slerp(
                _agent.transform.rotation,
                target,
                rotSpeed * Time.deltaTime
            );
        }

        if (_agent.pathPending)
            return Status.Running;

        if (_agent.pathStatus == NavMeshPathStatus.PathInvalid)
            return Status.Failure;

        if (_agent.remainingDistance <= _agent.stoppingDistance + 0.05f)
        {
            var points = PatrolPoints?.Value;
            if (points != null && points.Count > 0)
            {
                int idx = CurrentPatrolIndex.Value;
                idx = (idx + 1) % points.Count;
                CurrentPatrolIndex.Value = idx;
            }

            return Status.Success;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (_agent != null && _agent.isActiveAndEnabled)
        {
            _agent.ResetPath();
        }
    }
}
