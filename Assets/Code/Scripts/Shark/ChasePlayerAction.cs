using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;
using System.Collections.Generic;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ChasePlayerAction", story: "Chase [Self] to [Player] using [ChaseSpeed] and [LostDetectionRange]", category: "Action", id: "8a6d05d3485b5c0e01420ca2541c4f4e")]
public partial class ChasePlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Player;
    [SerializeReference] public BlackboardVariable<float> ChaseSpeed;
    [SerializeReference] public BlackboardVariable<float> LostDetectionRange;
    private NavMeshAgent _agent;

    protected override Status OnStart()
    {
        if (Self.Value == null)
            return Status.Failure;

        _agent = Self.Value.GetComponent<NavMeshAgent>();
        if (_agent == null)
            return Status.Failure;

        _agent.updateRotation = false;
        _agent.updateUpAxis = false;

        if (ChaseSpeed != null)
            _agent.speed = ChaseSpeed.Value;


        // Debug.Log("Moving to patrol point " + idx + " at " + _targetPosition);       

        _agent.SetDestination(Player.Value.transform.position);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (_agent == null)
            return Status.Failure;

        if (Player != null && Player.Value != null && LostDetectionRange != null)
        {
            float dist = Vector3.Distance(
                _agent.transform.position,
                Player.Value.transform.position
            );

            Debug.Log($"Distance to player: {dist}");

            if (dist >= LostDetectionRange.Value)
            {
                Debug.Log("Lost player");
                return Status.Failure; // or Status.Success, depending on how your graph is wired
            }

            _agent.SetDestination(Player.Value.transform.position);
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
            Debug.Log("Reached player");
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
    }
}

