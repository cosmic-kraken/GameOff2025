using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

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

    [Header("Damage Settings")]
    [SerializeField] private Volume postProcessVolume; // Voor damage effect
    [SerializeField] private float damageVignetteIntensity = 0.4f; // Maximale intensiteit
    [SerializeField] private float damageUpTime = 0.08f; // Tijd om te stijgen
    [SerializeField] private float damageDownTime = 0.18f; // Tijd om terug te gaan

    private float originalFieldOfView;
    private Coroutine zoomCoroutine;
    private Vignette vignette;
    private Coroutine damageCoroutine;

    private void Awake()
    {
        // Setup virtual camera
        if (virtualCamera == null) {
            virtualCamera = GetComponent<CinemachineCamera>();
        }

        if (virtualCamera == null) {
            Debug.LogError("CameraEffectsController: No CinemachineCamera found!");
            enabled = false;
            return;
        }

        // Setup impulse source
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

        // Setup vignette for damage effect
        if (postProcessVolume && postProcessVolume.profile.TryGet(out Vignette v)) {
            vignette = v;
        }
        else if (postProcessVolume != null) {
            Debug.LogWarning("CameraEffectsController: No Vignette override found in Volume!");
        }
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

    // New function: trigger damage flash effect
    public void TriggerDamageFlash()
    {
        if (vignette == null) return;

        if (damageCoroutine != null) StopCoroutine(damageCoroutine);

        damageCoroutine = StartCoroutine(DamageFlashRoutine());
        TriggerDashEffect();
    }

    private IEnumerator DamageFlashRoutine()
    {
        float t = 0f;

        // Fade in vignette
        while (t < damageUpTime)
        {
            t += Time.deltaTime;
            vignette.intensity.value = Mathf.Lerp(0f, damageVignetteIntensity, t / damageUpTime);
            yield return null;
        }

        t = 0f;

        // Fade out vignette
        while (t < damageDownTime)
        {
            t += Time.deltaTime;
            vignette.intensity.value = Mathf.Lerp(damageVignetteIntensity, 0f, t / damageDownTime);
            yield return null;
        }

        vignette.intensity.value = 0f;
        damageCoroutine = null;
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
