using System.Collections;
using System.Collections.Generic;
using HadiS.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUIManager : Singleton<GameplayUIManager>
{
    [Header("Health UI Settings")]
    [SerializeField] private Image _healthBar;
    
    [Header("Breath UI Settings")]
    [SerializeField] private Image _breathBar;
    
    [Header("Dash UI Settings")]
    [SerializeField] private List<GameObject> turtleDashIcons;
    [Range(1f, 1.5f)]
    [SerializeField] private float popScaleMultiplier = 1.2f;
    [SerializeField] private float popDuration = 0.3f;
    [SerializeField] private AnimationCurve popCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Trash UI Settings")]
    [SerializeField] private TextMeshProUGUI _trashText;
    
    private int currentMaxTrash = 0;
    private int collectedTrash = 0;
    private int[] previousCharges;
    private Dictionary<Transform, Vector3> originalScales = new();

    
    private void OnEnable() {
        TurtleController.OnTurtleHealthChanged += UpdateHealthUI;
        TurtleController.OnTurtleBreathChanged += UpdateBreathUI;
        TurtleController.OnTurtleDashChargesInitialized += InitializeTurtleDashUI;
        TurtleController.OnTurtleDashChargesChanged += UpdateDashUI;
        TrashSpawner.OnTrashSpawned += UpdateMaxTrash;
        TurtleController.OnTurtleCollectiblePickup += UpdateTrashUI;
    }

    private void OnDisable() {
        TurtleController.OnTurtleHealthChanged -= UpdateHealthUI;
        TurtleController.OnTurtleBreathChanged -= UpdateBreathUI;
        TurtleController.OnTurtleDashChargesInitialized -= InitializeTurtleDashUI;
        TurtleController.OnTurtleDashChargesChanged -= UpdateDashUI;
        TrashSpawner.OnTrashSpawned -= UpdateMaxTrash;
        TurtleController.OnTurtleCollectiblePickup -= UpdateTrashUI;
    }
    
    
    private void UpdateHealthUI(float currentHealth, float maxHealth) {
        if (_healthBar) {
            _healthBar.fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
        }
    }
    
    private void UpdateBreathUI(float currentBreath, float maxBreath) {
        if (_breathBar) {
            _breathBar.fillAmount = Mathf.Clamp01(currentBreath / maxBreath);
        }
    }

    
    private void InitializeTurtleDashUI(int maxCharges) {
        if (turtleDashIcons.Count < maxCharges) {
            Debug.LogWarning("Not enough UI elements to represent all dash charges.");
        }
        
        // Store original scales and set active state
        for (int i = 0; i < turtleDashIcons.Count; i++) {
            turtleDashIcons[i].SetActive(i < maxCharges);
            
            // Store the original scale of each icon
            if (!originalScales.ContainsKey(turtleDashIcons[i].transform)) {
                originalScales[turtleDashIcons[i].transform] = turtleDashIcons[i].transform.localScale;
            }
        }
        
        // Initialize previous charges tracking
        previousCharges = new int[maxCharges];

        UpdateDashUI(maxCharges);
    }
    
    private void UpdateDashUI(int currentCharges) {
        int previousChargeCount = previousCharges != null && previousCharges.Length > 0 ? previousCharges[0] : currentCharges;
        
        for (int i = 0; i < turtleDashIcons.Count; i++) {
            var icon = turtleDashIcons[i];
            var canvasGroup = icon.GetComponent<CanvasGroup>();
            if (!canvasGroup) {
                canvasGroup = icon.AddComponent<CanvasGroup>();
            }
            
            bool wasActive = i < previousChargeCount;
            bool isNowActive = i < currentCharges;
            
            canvasGroup.alpha = isNowActive ? 1f : 0.2f;
            
            // Trigger pop animation if charge became inactive (was used on Dash)
            if (previousCharges != null && wasActive && !isNowActive && i == currentCharges) {
                StartCoroutine(PopAnimation(icon.transform));
            }
        }
        
        // Store current charges for next frame
        if (previousCharges is { Length: > 0 }) {
            previousCharges[0] = currentCharges;
        }
    }
    
    private IEnumerator PopAnimation(Transform iconTransform) {
        // Note: Next time, we use Tweens. For now, this works...
        
        // Get the actual original scale of this specific icon
        if (!originalScales.TryGetValue(iconTransform, out Vector3 originalScale)) {
            originalScale = iconTransform.localScale;
            originalScales[iconTransform] = originalScale;
        }
        
        float elapsedTime = 0f;
        
        // Scale up
        while (elapsedTime < popDuration / 2f) {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (popDuration / 2f);
            float curveValue = popCurve.Evaluate(t);
            iconTransform.localScale = Vector3.Lerp(originalScale, originalScale * popScaleMultiplier, curveValue);
            yield return null;
        }
        
        elapsedTime = 0f;
        
        // Scale back down
        while (elapsedTime < popDuration / 2f) {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (popDuration / 2f);
            float curveValue = popCurve.Evaluate(t);
            iconTransform.localScale = Vector3.Lerp(originalScale * popScaleMultiplier, originalScale, curveValue);
            yield return null;
        }
        
        // Reset icon scale after animation
        iconTransform.localScale = originalScale;
    }
    
    private void UpdateMaxTrash(int maxTrash) {
        currentMaxTrash = maxTrash;
        collectedTrash = 0;
        UpdateTrashUI();
    }
    
    private void UpdateTrashUI(ICollectible trash = null) {
        if (trash != null) {
            collectedTrash ++;
        }
        
        if (_trashText) {
            _trashText.text = $"{collectedTrash} / {currentMaxTrash}";
        }
    }
}
