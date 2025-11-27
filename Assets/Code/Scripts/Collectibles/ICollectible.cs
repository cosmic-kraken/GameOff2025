using UnityEngine;

public interface ICollectible
{
    int Value { get; }
    
    void Collect(GameObject collector);
}
