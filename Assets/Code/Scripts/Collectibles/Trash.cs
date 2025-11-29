using System.Collections;
using UnityEngine;

public class Trash : MonoBehaviour, ICollectible
{
    [Header("Collectible Settings")]
    [SerializeField] private int value;
    
    [Header("Animation Settings")]
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0, 50, 0);
    [SerializeField] private bool randomizeRotationSpeed = true;
    [SerializeField] private float randomSpeedVariation = 0.3f;
    
    public int Value => value;
    
    private Vector3 currentRotationSpeed;
    private Coroutine _rotationCoroutine;
    
    
    private void Awake() {
        if (randomizeRotationSpeed)
        {
            currentRotationSpeed = rotationSpeed * Random.Range(1f - randomSpeedVariation, 1f + randomSpeedVariation);;
            
            if (Random.value > 0.5f)
            {
                currentRotationSpeed.y *= -1f;
            }
        }
        else
        {
            currentRotationSpeed = rotationSpeed;
        }
    }
    
    public void Collect(GameObject collector) {
        GameStateManager.Instance?.AddScore(value);
        Destroy(gameObject);
    }
    
    public void StartRotating()
    {
        if (_rotationCoroutine == null)
        {
            _rotationCoroutine = StartCoroutine(RotateWhileVisible());
        }
    }
    
    public void StopRotating() {
        if (_rotationCoroutine != null)
        {
            StopCoroutine(_rotationCoroutine);
            _rotationCoroutine = null;
        }
    }
    
    private IEnumerator RotateWhileVisible() {
        while (true)
        {
            transform.Rotate(currentRotationSpeed * Time.deltaTime, Space.Self);
            yield return null;
        }
    }
}
