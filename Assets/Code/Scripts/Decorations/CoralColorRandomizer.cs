using System.Collections.Generic;
using UnityEngine;

public class CoralColorRandomizer : MonoBehaviour
{
    [SerializeField] private List<Material> materials;
    
    private void Start()
    {
        if (materials == null || materials.Count == 0)
        {
            Debug.LogWarning("No materials assigned for CoralColorRandomizer.");
            return;
        }

        var renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            int randomIndex = Random.Range(0, materials.Count);
            renderer.material = materials[randomIndex];
        }
        else
        {
            Debug.LogWarning("No Renderer component found on the GameObject.");
        }
    }
}
