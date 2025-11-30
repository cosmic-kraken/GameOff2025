using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TurtleController : MonoBehaviour, IDamageable
{
    public static Action OnTurtleDeath = delegate { };
    public static Action OnTurtleDash = delegate { };
    public static Action<ICollectible> OnTurtleCollectiblePickup = delegate { };
    public static Action<float, float> OnTurtleHealthChanged = delegate { }; // Current health, max health
    public static Action<float, float> OnTurtleBreathChanged = delegate { }; // Current breath, max breath
    public static Action<int> OnTurtleDashChargesInitialized = delegate { }; // Max dash charges
    public static Action<int> OnTurtleDashChargesChanged = delegate { }; // Current dash charges

    private readonly int SwimAnimHash = Animator.StringToHash("Swim");
    private readonly int DashAnimHash = Animator.StringToHash("Dash");

    [Header("Breathing")]
    [SerializeField] private float _maxBreathTime = 20f;
    [SerializeField] private bool _disableBreathing;
    
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;
    [SerializeField] private bool invincible;
    [SerializeField] private float regenPerSecond = 2f;

    [Header("Movement")]
    [SerializeField] private float swimSpeed = 5f;
    [SerializeField] private float swimForce = 50f;
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private float dashTurnSpeed = 15f;
    [Range(0f, 1f)]
    [SerializeField] private float surfaceDeflectAngle = 0.7f;
    [SerializeField] private Transform seaLevelTransform;
    [SerializeField] private float outOfWaterEffectiveness = 0.50f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 20f;
    [Range(0f, 4f)]
    [SerializeField] private int dashCharges = 3;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float dashChargeRegenTime = 5f;
    [SerializeField] private ParticleSystem dashParticles;

    [Header("Camera Effects")]
    [SerializeField] private CameraEffectsController cameraEffects;

    [Header("Other Settings")]
    [SerializeField] private float trashHintThreshold = 30f;
    [SerializeField] private float trashHintCooldown = 10f;
    [SerializeField] private float trashHintDuration = 10f;
    [SerializeField] private float hintArrowOffset = 1.5f; 
    [SerializeField] private GameObject hintArrow;
    
    private Rigidbody rb;
    private Animator animator;
    private PlayerControls controls;

    private Vector3 moveInput;
    private float lastHorizontalInput = 1f;
    private Vector3 moveDir => new Vector3(moveInput.x, moveInput.y, 0f).normalized;

    private float dashCooldownTimer;

    private bool isDead;
    private bool isDashing;
    private bool dashAnimationComplete; // Doesn't affect logic yet, but we can use it for logic like particles or sound timing later.
    private int currentDashCharges;
    private float dashTimer;
    private float dashChargeRegenTimer;
    private float breathTimer;
    private Vector3 dashDirection = Vector3.zero;
    private float dashFacingDirection;
    private float timeSinceLastTrashPickup = 0f;
    private float timeSinceLastHint = 0f;
    
    private List<GameObject> trashInLevel = new();
    private Coroutine hintCoroutine;
    
    private bool isAboveWater = false;


    private void Awake() {
        animator = GetComponentInChildren<Animator>();
        
        if (animator == null) {
            Debug.LogError("TurtleController: No Animator found in children.");
        }
        
        rb = GetComponent<Rigidbody>();
        cameraEffects ??= FindFirstObjectByType<CameraEffectsController>();

        // Move on X and Y only. Rotation is done manually, so ensure no physics-based rotation occurs.
        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        dashParticles.Clear();
        dashParticles.Stop();
        breathTimer = _maxBreathTime;
        currentHealth = maxHealth;
        currentDashCharges = dashCharges;
    }

    private void Start() {
        OnTurtleHealthChanged?.Invoke(currentHealth, maxHealth);
        OnTurtleBreathChanged?.Invoke(breathTimer, _maxBreathTime);
        OnTurtleDashChargesInitialized?.Invoke(currentDashCharges);
        OnTurtleDashChargesChanged?.Invoke(currentDashCharges);
        
        trashInLevel = FindFirstObjectByType<TrashSpawner>()?.SpawnedTrash ?? new List<GameObject>();
    }

    private void OnEnable() {
        controls = new PlayerControls();
        controls.Player.Enable();
    }

    private void OnDisable() {
        controls.Player.Disable();
        controls = null;
    }

    private void Update() {
        if (isDead) return;

        HandleInput();
        HandleTimers();
    }

    private void FixedUpdate() {
        if (isDead) return;

        HandleSwim();
        HandleRotation();
        ClampToSeaLevel();
    }
    
    private void ClampToSeaLevel() {
        if (!seaLevelTransform) return;
        
        var seaLevel = seaLevelTransform.position.y;
        bool wasAboveWater = isAboveWater;
        isAboveWater = rb.position.y > seaLevel;
        
        // Detect state transitions and play sounds
        if (isAboveWater && !wasAboveWater) {
            AudioManager.Instance?.Play("Splash");
        }
        else if (!isAboveWater && wasAboveWater) {
            AudioManager.Instance?.Play("Splash");
        }
        
        // If turtle is above water
        if (isAboveWater) {
            // Apply strong downward force to create realistic falling speed
            rb.AddForce(Vector3.down * 80f, ForceMode.Acceleration);
            return;
        }
        
        // Turtle is in the water, prevent swimming above surface unless dashing
        if (rb.position.y >= seaLevel - 0.5f) {
            // Near or at the surface
            if (rb.linearVelocity.y > 0f && !isDashing) {
                // Has upward velocity but not dashing - clamp to surface
                var clampedPosition = rb.position;
                clampedPosition.y = Mathf.Min(clampedPosition.y, seaLevel);
                rb.position = clampedPosition;
                
                // Cancel ALL upward velocity when not dashing near surface
                var velocity = rb.linearVelocity;
                velocity.y = 0f;
                rb.linearVelocity = velocity;
            }
        }
    }
    
    private bool IsAtSeaLevel() {
        if (!seaLevelTransform) return false;
        return Mathf.Abs(rb.position.y - seaLevelTransform.position.y) < 0.5f;
    }

    private void HandleInput() {
        moveInput = controls.Player.Move.ReadValue<Vector2>();

        // Latest horizontal input. Only update when input is given.
        if (Mathf.Abs(moveInput.x) > 0.01f) {
            lastHorizontalInput = Mathf.Sign(moveInput.x);
        }

        // Dash input - only allow dashing when in water
        if (!isDashing && !isAboveWater && currentDashCharges > 0 && dashCooldownTimer <= 0f && controls.Player.Dash.WasPressedThisDynamicUpdate()) {
            StartDash();
        }
    }

    private void StartDash() {
        OnTurtleDash?.Invoke();
        isDashing = true;
        dashTimer = dashDuration;
        currentDashCharges--;
        OnTurtleDashChargesChanged?.Invoke(currentDashCharges);
        dashDirection = moveDir.sqrMagnitude > 0.01f ? moveDir : (lastHorizontalInput > 0 ? Vector3.right : Vector3.left);
        dashFacingDirection = lastHorizontalInput;

        rb.linearVelocity = dashDirection.normalized * dashSpeed;

        dashParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        dashParticles.Play();
        
        dashAnimationComplete = false;
        animator.speed = 1f;
        animator.Play(DashAnimHash);
        
        cameraEffects?.TriggerDashEffect();
        AudioManager.Instance?.Play("Dash");
    }

    private void CancelDash() {
        if (!isDashing) return;

        isDashing = false;
        dashCooldownTimer = dashCooldown;
        dashParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        rb.linearVelocity = Vector3.zero;
    }

    private void HandleTimers() {

        // Health regeneration
        if (currentHealth < maxHealth) {
            currentHealth += regenPerSecond * Time.deltaTime;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }

        // Breathing timer
        if (IsAtSeaLevel() || isAboveWater) {
            breathTimer += Time.deltaTime * 5f;
            breathTimer = Mathf.Min(breathTimer, _maxBreathTime);
        }
        else {
            breathTimer -= Time.deltaTime;
            if (breathTimer <= 0f && !_disableBreathing) {
                isDead = true;
                rb.linearVelocity = Vector3.zero;
                dashParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                OnTurtleDeath?.Invoke();
                return;
            }
        }
        OnTurtleBreathChanged?.Invoke(breathTimer, _maxBreathTime);
        
        // Update dash timer
        if (isDashing) {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f) {
                isDashing = false;
                dashCooldownTimer = dashCooldown;
                dashParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        // Update dash cooldown timer
        if (dashCooldownTimer > 0f) {
            dashCooldownTimer -= Time.deltaTime;
        }

        // Dash charge regeneration
        if (currentDashCharges < dashCharges) {
            dashChargeRegenTimer += Time.deltaTime;
            if (dashChargeRegenTimer >= dashChargeRegenTime) {
                currentDashCharges++;
                dashChargeRegenTimer = 0f;
                OnTurtleDashChargesChanged?.Invoke(currentDashCharges);
            }
        }
        
        // Trash hint timer
        timeSinceLastTrashPickup += Time.deltaTime;
        timeSinceLastHint += Time.deltaTime;
        if (timeSinceLastTrashPickup >= trashHintThreshold && timeSinceLastHint >= trashHintCooldown && hintCoroutine == null) {
            if (trashInLevel.Exists(t => t)) {
                hintCoroutine = StartCoroutine(HintCoroutine());
            }
        }
    }

    private IEnumerator HintCoroutine() {
        // Configuration missing
        if (!hintArrow) {
            Debug.LogWarning("TurtleController: No hint arrow child assigned!");
            timeSinceLastHint = 0f;
            yield break;
        }
        
        // Enable the arrow child
        hintArrow.SetActive(true);
        
        float elapsedTime = 0f;
        while (elapsedTime < trashHintDuration) {
            
            // Find nearest trash
            GameObject nearestTrash = null;
            var nearestDistanceSqr = float.MaxValue;
            foreach (var trash in trashInLevel) {
                // Skip null or collected trash
                if (!trash) continue;
                var distSqr = (trash.transform.position - transform.position).sqrMagnitude;
                if (!(distSqr < nearestDistanceSqr)) continue;
                
                nearestDistanceSqr = distSqr;
                nearestTrash = trash;
            }
            
            // If no trash found, exit hint early
            if (!nearestTrash) {
                break;
            }
            
            // Calculate direction to trash
            Vector3 directionToTrash = (nearestTrash.transform.position - transform.position).normalized;
            
            // Position arrow offset from turtle towards trash
            Vector3 arrowPosition = transform.position + directionToTrash * hintArrowOffset;
            hintArrow.transform.position = arrowPosition;
            
            // Calculate angle to rotate arrow. (Current arrow points right at 0 degrees on Z axis by default)
            var angleToTrash = Mathf.Atan2(directionToTrash.y, directionToTrash.x) * Mathf.Rad2Deg;
            
            // Rotate the arrow on Z axis to point toward trash
            hintArrow.transform.rotation = Quaternion.Euler(0f, 0f, angleToTrash);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Reset state
        hintArrow.SetActive(false);
        timeSinceLastHint = 0f;
        hintCoroutine = null;
    }

    private void HandleSwim() {
        if (isDashing) return;

        // Prevent swimming upward when near surface (only when in water)
        var adjustedMoveDir = moveDir;
        if (!isAboveWater && seaLevelTransform && rb.position.y >= seaLevelTransform.position.y - 0.5f) {
            adjustedMoveDir.y = Mathf.Min(adjustedMoveDir.y, 0f);
        }

        // Reduced movement effectiveness
        var effectivenessMultiplier = isAboveWater ? outOfWaterEffectiveness : 1f;
        var targetVelocity = adjustedMoveDir * (swimSpeed * effectivenessMultiplier);
        var velocityDiff = targetVelocity - rb.linearVelocity;
        rb.AddForce(velocityDiff * (swimForce * effectivenessMultiplier), ForceMode.Force);
        animator.speed = targetVelocity.sqrMagnitude > 0.01f ? 2.5f : 1f;
    }

    private float CalculateTargetYRotation() {
        return lastHorizontalInput < 0 ? -180f : 0f;
    }

    private float CalculateTargetZRotation() {
        // No movement, go back to neutral
        if (!(Mathf.Abs(moveInput.x) > 0.01f) && !(Mathf.Abs(moveInput.y) > 0.01f)) return 0f;

        // Calculate based on last horizontal input direction
        float targetZ = Mathf.Atan2(moveInput.y, lastHorizontalInput > 0 ? moveInput.x : -moveInput.x) * Mathf.Rad2Deg;
        
        // Clamp Z rotation at sea level to prevent upward tilt
        if (IsAtSeaLevel() && targetZ > 0f) {
            targetZ = 0f;
        }
        
        return targetZ;
    }

    private float CalculateDashTargetZRotation() {
        float targetZ = Mathf.Atan2(dashDirection.y, dashFacingDirection > 0 ? dashDirection.x : -dashDirection.x) * Mathf.Rad2Deg;
        
        // Clamp Z rotation at sea level to prevent upward tilt
        if (IsAtSeaLevel() && targetZ > 0f) {
            targetZ = 0f;
        }
        
        return targetZ;
    }

    private float CalculateDashTargetYRotation() {
        return dashFacingDirection < 0 ? -180f : 0f;
    }

    private void HandleRotation() {
        // Reduced rotation effectiveness when above water
        var rotationMultiplier = isAboveWater ? outOfWaterEffectiveness : 1f;
        
        if (isDashing) {
            // Dashing rotation, use the captured facing direction from dash start
            var dashTargetY = CalculateDashTargetYRotation();
            var dashTargetZ = CalculateDashTargetZRotation();
            var targetRot = Quaternion.Euler(0f, dashTargetY, dashTargetZ);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, dashTurnSpeed * rotationMultiplier * Time.fixedDeltaTime));
        }
        else {
            // Normal rotation, update both Y and Z axes based on current input
            var targetY = CalculateTargetYRotation();
            var targetZ = CalculateTargetZRotation();
            var targetRot = Quaternion.Euler(0f, targetY, targetZ);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, turnSpeed * rotationMultiplier * Time.fixedDeltaTime));
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if (!isDashing) return;


        // TODO: Ensure this only runs on terrain. For now nothing else triggers this.

        // Get the average collision normal
        var collisionNormal = Vector3.zero;
        foreach (var contact in collision.contacts) {
            collisionNormal += contact.normal;
        }

        collisionNormal.Normalize();

        // Calculate the angle between dash direction and collision normal
        var dotProduct = Vector3.Dot(dashDirection.normalized, -collisionNormal);

        // If dotProduct > 0.7 (~45 degrees) -> head-on collision, cancel the dash
        // If dotProduct < 0.7, grazing it -> redirect velocity along the surface
        if (dotProduct > surfaceDeflectAngle) {
            CancelDash();
        }
        else {
            // Glancing collision - deflect the velocity along the surface
            var deflectedVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, collisionNormal);
            rb.linearVelocity = deflectedVelocity;

            // Update dash direction to match the new deflected direction
            if (deflectedVelocity.sqrMagnitude > 0.01f) {
                dashDirection = deflectedVelocity.normalized;
            }
        }

    }

    public void OnDashAnimationComplete() {
        // Todo: Trigger logic like sound or particles here
        dashAnimationComplete = true;
    }

    private void OnTriggerEnter(Collider other) {
        
        if (other.TryGetComponent<ICollectible>(out var collectible)) {
            OnTurtleCollectiblePickup?.Invoke(collectible);
            timeSinceLastTrashPickup = 0f;
            collectible.Collect(gameObject);
            
            // Stop hint coroutine if trash was collected (arrow will be cleaned up by the coroutine)
            if (hintCoroutine != null) {
                StopCoroutine(hintCoroutine);
                timeSinceLastHint = 0f;
                hintCoroutine = null;
                hintArrow.SetActive(false);
            }
            
            AudioManager.Instance?.Play("Pickup");
        }
        
        if (other.TryGetComponent<IAirBubble>(out var airBubble)) {
            var airAmount = airBubble.GetAirAmount();
            breathTimer += airAmount;
            breathTimer = Mathf.Min(breathTimer, _maxBreathTime);
            OnTurtleBreathChanged?.Invoke(breathTimer, _maxBreathTime);
            airBubble.PopBubble();
            AudioManager.Instance?.Play("Small_Bubbles");
        }
    }

    public void InflictDamage(float damage) {
        if (invincible || isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);
        OnTurtleHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // TODO: Do damage feedback (particles, sound, camera shake, etc.)

        if (currentHealth <= 0f) {
            isDead = true;
            rb.linearVelocity = Vector3.zero;
            dashParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            OnTurtleDeath?.Invoke();
        }
    }
    
    public float GetCurrentHealth() => currentHealth;
    
    public bool IsAlive()  => !isDead;
    
    public bool IsDead() => isDead;
}
