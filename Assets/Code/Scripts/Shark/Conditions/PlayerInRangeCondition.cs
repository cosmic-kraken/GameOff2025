using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "PlayerInRangeCondition", story: "Check if [Self] is withing [Range] of [Player] and [Player] is visible from [SharkEyePosition]", category: "Conditions", id: "6f262f308f8b058bdc7d6b29ce37a1ac")]
public partial class PlayerInRangeCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<float> Range;
    [SerializeReference] public BlackboardVariable<GameObject> Player;
    [SerializeReference] public BlackboardVariable<Transform> SharkEyePosition;

    public override bool IsTrue()
    {
        // Debug.Log($"Checking PlayerInRangeCondition: Self={Self.Value}, Player={Player.Value}, Range={Range.Value}");

        if (Self.Value == null || Player.Value == null)
            return false;

        float dist = Vector3.Distance(
            Self.Value.transform.position,
            Player.Value.transform.position);

        // Check line of sight from SharkEyePosition to Player
        RaycastHit hit;
        Vector3 direction = Player.Value.transform.position - SharkEyePosition.Value.position;
        if (Physics.Raycast(SharkEyePosition.Value.position, direction, out hit, Range.Value))
        {
            if (hit.collider.gameObject != Player.Value)
            {
                // Debug.Log("Player is not visible due to obstruction.");
                return false;
            }
        }

        // Check if player is behind obstacles (raycast from player to +Z direction)
        RaycastHit hitBehind;
        Vector3 towardsCamDir = Vector3.back;
        float rayLength = 10.0f;

        Debug.DrawRay(Player.Value.transform.position, towardsCamDir * rayLength, Color.red);

        if (Physics.Raycast(Player.Value.transform.position, towardsCamDir, out hitBehind, rayLength))
        {
            // Debug.Log("Player is behind an obstacle.");
            return false;
        }
        else
        {
            // Debug.Log("Nothing behind player.");
        }

        return dist < Range.Value;
    }
}
