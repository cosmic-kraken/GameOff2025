
using UnityEngine;
using UnityEngine.Events;

public class VisibilityNotifier : MonoBehaviour
{
    
    [Header("Visibility Events")]
    [SerializeField] private UnityEvent _onBecameVisible;
    [SerializeField] private UnityEvent _onBecameInvisible;
    
    private void OnBecameVisible() => _onBecameVisible?.Invoke();
    
    private void OnBecameInvisible() => _onBecameInvisible?.Invoke();
    
}
