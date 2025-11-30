using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;
using System.Collections.Generic;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ChasePlayerAction", story: "Chase [Self] to [Player] using [ChaseSpeed] and [LostDetectionRange]", category: "Action", id: "8a6d05d3485b5c0e01420ca2541c4f4e")]
public partial class ChasePlayerAction : SharkAgentActionBase
{
    [SerializeReference] public BlackboardVariable<GameObject> Player;
    [SerializeReference] public BlackboardVariable<float> ChaseSpeed;
    [SerializeReference] public BlackboardVariable<float> LostDetectionRange;

    protected override Status OnStart()
    {
        if (!TryInitializeAgent(ChaseSpeed?.Value))
            return Status.Failure;

        if (Player == null || Player.Value == null)
            return Status.Failure;

        Agent.SetDestination(Player.Value.transform.position);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Status pathStatus = CheckAgentPathState();
        if (pathStatus == Status.Failure)
            return Status.Failure;

        if (Player != null && Player.Value != null && LostDetectionRange != null)
        {
            float distanceToPlayer = Vector3.Distance(
                Agent.transform.position,
                Player.Value.transform.position
            );

            // Debug.Log($"[Chase] Distance to player: {distanceToPlayer}");

            if (distanceToPlayer >= LostDetectionRange.Value)
            {
                Debug.Log("[Chase] Lost player");
                return Status.Failure; // or Success depending on your graph
            }

            Agent.SetDestination(Player.Value.transform.position);
        }

        UpdateRotationFromVelocity2D();

        if (Agent.remainingDistance <= Agent.stoppingDistance + 0.05f)
        {
            Debug.Log("[Chase] Reached player");
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
    }
}
