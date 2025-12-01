using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ChasePlayerAction", story: "Chase [Self] to [Player] using [ChaseSpeed] and [LostDetectionRange]", category: "Action", id: "8a6d05d3485b5c0e01420ca2541c4f4e")]
public partial class ChasePlayerAction : SharkAgentActionBase
{
    [SerializeReference] public BlackboardVariable<bool> IsChasing;
    [SerializeReference] public BlackboardVariable<GameObject> Player;
    [SerializeReference] public BlackboardVariable<float> ChaseSpeed;
    [SerializeReference] public BlackboardVariable<float> LostDetectionRange;
    [SerializeReference] public BlackboardVariable<float> DetectionRange;
    [SerializeReference] public BlackboardVariable<float> InitialDetectionRange;
    [SerializeReference] public BlackboardVariable<bool> TurtleBite;
    private bool _hasJustEaten = false;
    private float _eatCooldown = 2f;
    private float _eatCooldownEndTime = -1f;

    protected override Status OnStart()
    {
        if (TurtleBite != null)
            TurtleBite.Value = false;

        if (!TryInitializeBase(ChaseSpeed?.Value))
            return Status.Failure;

        if (Player == null || Player.Value == null)
            return Status.Failure;

        if (Time.time < _eatCooldownEndTime)
        {
            // Spaghetti: force close mouth if we have just eaten 
            PlayAnimationIfNotRunning("Swimming", "LostChase");   
            return Status.Failure;
        }

        Debug.Log("[Chase] Starting chase towards player");

        _hasJustEaten = false;

        Agent.SetDestination(Player.Value.transform.position);
        PlayAnimationIfNotRunning("Chase", "StartChase");
        IsChasing.Value = true;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Player == null || Player.Value == null || LostDetectionRange == null)
            return Status.Failure;

        if (TurtleBite != null && TurtleBite.Value)
        {
            if (!_hasJustEaten)
            {
                Debug.Log("[Chase] Shark has bitten the turtle!");
                _hasJustEaten = true;
                _eatCooldownEndTime = Time.time + _eatCooldown;
            }
            return Status.Failure;
        }

        Status pathStatus = CheckAgentPathState();
        if (pathStatus == Status.Failure)
            return Status.Failure;

        float distanceToPlayer = Vector3.Distance(
            Agent.transform.position,
            Player.Value.transform.position
        );

        // Debug.Log($"[Chase] Distance to player: {distanceToPlayer}");

        if (distanceToPlayer >= LostDetectionRange.Value)
        {
            Debug.Log("[Chase] Lost player");
            return Status.Failure;
        }

        NavMeshPath path = new NavMeshPath();
        bool canReach = Agent.CalculatePath(Player.Value.transform.position, path) 
                        && path.status == NavMeshPathStatus.PathComplete;
        
        if (!canReach)
        {
            Debug.Log("[Chase] Cannot reach player");
            DetectionRange.Value = InitialDetectionRange.Value / 2f;
            return Status.Failure;
        }

        Agent.SetDestination(Player.Value.transform.position);

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
