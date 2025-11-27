using UnityEngine;

public class Trash : MonoBehaviour, ICollectible
{
    [SerializeField] private int _value;
    
    public int Value => _value;
    
    
    public void Collect(GameObject collector)
    {
        // TODO: any effects or sounds upon collection
        Destroy(gameObject);
    }
}
