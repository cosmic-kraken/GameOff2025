using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class CameraEffectsController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CinemachineCamera virtualCamera;
    [SerializeField] private CinemachineImpulseSource impulseSource;
    
    [Header("Shake Settings")]
    [SerializeField] private float shakeForce = 3f;
    [SerializeField] private Vector3 shakeVelocity = new (2f, 2f, 0f);
    
    [Header("Zoom Settings")]
    [SerializeField] private float zoomAmount = 5f;
    [SerializeField] private float zoomDuration = 0.2f;
    [SerializeField] private AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private float originalFieldOfView;
    private Coroutine zoomCoroutine;

    
    private void Awake()
    {
        if (virtualCamera == null) {
            virtualCamera = GetComponent<CinemachineCamera>();
        }

        if (virtualCamera == null) {
            Debug.LogError("CameraEffectsController: No CinemachineCamera found!");
            enabled = false;
            return;
        }
        
        if (impulseSource == null) {
            impulseSource = GetComponent<CinemachineImpulseSource>();
        }
        
        if (impulseSource == null) {
            Debug.Log("CameraEffectsController: Adding CinemachineImpulseSource...");
            impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
            
            // Configure impulse source with proper defaults
            impulseSource.ImpulseDefinition.ImpulseDuration = 0.3f;
            impulseSource.ImpulseDefinition.ImpulseType = CinemachineImpulseDefinition.ImpulseTypes.Uniform;
            impulseSource.DefaultVelocity = shakeVelocity;
        }
        
        // Check if the camera has an impulse listener
        var listener = virtualCamera.GetComponent<CinemachineImpulseListener>();
        if (listener == null) {
            Debug.LogWarning("CameraEffectsController: No CinemachineImpulseListener found on camera! Creating runtime listener...");
            listener = virtualCamera.gameObject.AddComponent<CinemachineImpulseListener>();
            listener.Gain = 2f; // Increased gain for more visible effect
            listener.Use2DDistance = false;
            
            // Configure reaction settings
            listener.ReactionSettings.AmplitudeGain = 1f;
            listener.ReactionSettings.FrequencyGain = 1f;
            listener.ReactionSettings.Duration = 1f;
        }

        
        originalFieldOfView = virtualCamera.Lens.FieldOfView;
    }
    
    public void TriggerDashEffect() {
        
        // Trigger impulse shake
        if (impulseSource) {
            Vector3 impulseVelocity = shakeVelocity * shakeForce;
            impulseSource.GenerateImpulseWithVelocity(impulseVelocity);
        }

        // Trigger zoom
        if (zoomCoroutine != null) {
            StopCoroutine(zoomCoroutine);
        }
        
        zoomCoroutine = StartCoroutine(ZoomCamera());
    }


    private IEnumerator ZoomCamera() {
        float elapsedTime = 0f;
        
        // Zoom in
        while (elapsedTime < zoomDuration) {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / zoomDuration;
            float curveValue = zoomCurve.Evaluate(t);
            
            virtualCamera.Lens.FieldOfView = originalFieldOfView - (zoomAmount * curveValue);
            
            yield return null;
        }
        
        elapsedTime = 0f;
        
        // Zoom out (return to original FOV)
        while (elapsedTime < zoomDuration) {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / zoomDuration;
            float curveValue = zoomCurve.Evaluate(1f - t);
            
            virtualCamera.Lens.FieldOfView = originalFieldOfView - (zoomAmount * curveValue);
            
            yield return null;
        }
        
        virtualCamera.Lens.FieldOfView = originalFieldOfView;
        zoomCoroutine = null;
    }
}

