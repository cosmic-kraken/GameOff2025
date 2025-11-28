using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

public class AirBubble : MonoBehaviour, IAirBubble
{
    [SerializeField] private ParticleSystem popEffect;
    [SerializeField] private float airAmount = 5f;
    [SerializeField] private float minLifetime = 1.5f;
    [SerializeField] private float maxLifeTime = 2.5f;
    [SerializeField] private float minSpeed = 1f;
    [SerializeField] private float maxSpeed = 3f;
    [SerializeField] private float xOscillationAmplitude = 0.2f;
    [SerializeField] private float xOscillationFrequency = 2.5f;

    private IObjectPool<AirBubble> pool;
    private float spawnTime;
    private Vector3 startPos;
    private Quaternion startRot;
    private Vector3 startScale;
    private bool active;
    private Coroutine moveCoroutine;

    private float floatSpeed;
    private float lifetime;

    
    public void Init(IObjectPool<AirBubble> poolRef, Vector3 spawnPosition) {
        pool = poolRef;
        startPos = spawnPosition;
        transform.position = startPos;
        transform.rotation = Quaternion.identity;
    }
    
    public float GetAirAmount() {
        return airAmount;
    }

    public void PopBubble() {
        if (!active) return;
        
        active = false;
        
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        StartCoroutine(ReturnToPoolAfterEffect());
    }

    private void OnEnable() {
        spawnTime = Time.time;
        floatSpeed = Random.Range(minSpeed, maxSpeed);
        lifetime = Random.Range(minLifetime, maxLifeTime);
        active = true;
        
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }
        
        moveCoroutine = StartCoroutine(MoveBubbleCoroutine());
    }

    private IEnumerator MoveBubbleCoroutine() {
        while (active)
        {
            var elapsed = Time.time - spawnTime;
            var pos = startPos + Vector3.up * (floatSpeed * elapsed);
            pos.x += Mathf.Sin(elapsed * xOscillationFrequency) * xOscillationAmplitude;
            transform.position = pos;
            if (elapsed >= lifetime)
            {
                PopBubble();
                yield break;
            }
            yield return null;
        }
    }
    
    private IEnumerator ReturnToPoolAfterEffect() {
        if (popEffect) {
            popEffect.Play();
            yield return new WaitForSeconds(popEffect.main.duration);
        }

        if (pool != null) {
            pool.Release(this);
        }
        else {
            Destroy(gameObject);
        }
    }
}
