using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "PlayerInRangeCondition", story: "Check if [Self] is withing [Range] of [Player]", category: "Conditions", id: "6f262f308f8b058bdc7d6b29ce37a1ac")]
public partial class PlayerInRangeCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<float> Range;
    [SerializeReference] public BlackboardVariable<GameObject> Player;

    public override bool IsTrue()
    {
        // Debug.Log($"Checking PlayerInRangeCondition: Self={Self.Value}, Player={Player.Value}, Range={Range.Value}");

        if (Self.Value == null || Player.Value == null)
            return false;

        float dist = Vector3.Distance(
            Self.Value.transform.position,
            Player.Value.transform.position);

        Debug.Log($"Distance to player: {dist}");

        return dist < Range.Value;
    }
}
