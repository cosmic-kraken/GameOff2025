using System;
using UnityEngine;

public class TurtleController : MonoBehaviour
{
    public static Action OnTurtleDeath = delegate { };
    public static Action OnTurtleDash = delegate { };
    public static Action<ICollectible> OnTurtleCollectiblePickup = delegate { };

    private readonly int SwimAnimHash = Animator.StringToHash("Swim");
    private readonly int DashAnimHash = Animator.StringToHash("Dash");

    [Header("Breathing")]
    [SerializeField] private float _maxBreathTime = 20f;
    [SerializeField] private bool _disableBreathing;

    [Header("Movement")]
    [SerializeField] private float swimSpeed = 5f;
    [SerializeField] private float swimForce = 50f;
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private float dashTurnSpeed = 15f;
    [Range(0f, 1f)]
    [SerializeField] private float surfaceDeflectAngle = 0.7f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private ParticleSystem dashParticles;

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
    private float dashTimer;
    private float breathTimer;
    private Vector3 dashDirection = Vector3.zero;
    private float dashFacingDirection;


    private void Awake() {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        // Move on X and Y only. Rotation is done manually, so ensure no physics-based rotation occurs.
        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        dashParticles.Clear();
        dashParticles.Stop();
        breathTimer = _maxBreathTime;
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
    }

    private void HandleInput() {
        moveInput = controls.Player.Move.ReadValue<Vector2>();

        // Latest horizontal input. Only update when input is given.
        if (Mathf.Abs(moveInput.x) > 0.01f) {
            lastHorizontalInput = Mathf.Sign(moveInput.x);
        }

        // Dash input
        if (!isDashing && dashCooldownTimer <= 0f && controls.Player.Dash.WasPressedThisDynamicUpdate()) {
            StartDash();
        }
    }

    private void StartDash() {
        OnTurtleDash?.Invoke();
        isDashing = true;
        dashTimer = dashDuration;
        dashDirection = moveDir.sqrMagnitude > 0.01f ? moveDir : (lastHorizontalInput > 0 ? Vector3.right : Vector3.left);
        dashFacingDirection = lastHorizontalInput;

        rb.linearVelocity = dashDirection.normalized * dashSpeed;

        dashParticles.Clear();
        dashParticles.Stop();
        dashParticles.Play();

        dashAnimationComplete = false;
        animator.Play(DashAnimHash);
    }

    private void CancelDash() {
        if (!isDashing) return;

        isDashing = false;
        dashCooldownTimer = dashCooldown;
        dashParticles.Clear();
        dashParticles.Stop();
        rb.linearVelocity = Vector3.zero;
    }

    private void HandleTimers() {

        // Breathing timer
        breathTimer -= Time.deltaTime;
        if (breathTimer <= 0f && !_disableBreathing) {
            isDead = true;
            rb.linearVelocity = Vector3.zero;
            dashParticles.Clear();
            dashParticles.Stop();
            OnTurtleDeath?.Invoke();
            return;
        }

        // Update dash timer
        if (isDashing) {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f) {
                isDashing = false;
                dashCooldownTimer = dashCooldown;
                dashParticles.Clear();
                dashParticles.Stop();
            }
        }

        // Update dash cooldown timer
        if (dashCooldownTimer > 0f) {
            dashCooldownTimer -= Time.deltaTime;
        }
    }

    private void HandleSwim() {
        if (isDashing) return;

        var targetVelocity = new Vector3(moveDir.x * swimSpeed, moveDir.y * swimSpeed, 0f);
        var velocityDiff = targetVelocity - rb.linearVelocity;
        rb.AddForce(velocityDiff * swimForce, ForceMode.Force);
    }

    private float CalculateTargetYRotation() {
        return lastHorizontalInput < 0 ? -180f : 0f;
    }

    private float CalculateTargetZRotation() {
        // No movement, go back to neutral
        if (!(Mathf.Abs(moveInput.x) > 0.01f) && !(Mathf.Abs(moveInput.y) > 0.01f)) return 0f;

        // Calculate based on last horizontal input direction
        return Mathf.Atan2(moveInput.y, lastHorizontalInput > 0 ? moveInput.x : -moveInput.x) * Mathf.Rad2Deg;
    }

    private float CalculateDashTargetZRotation() {
        return Mathf.Atan2(dashDirection.y, dashFacingDirection > 0 ? dashDirection.x : -dashDirection.x) * Mathf.Rad2Deg;
    }

    private float CalculateDashTargetYRotation() {
        return dashFacingDirection < 0 ? -180f : 0f;
    }

    private void HandleRotation() {
        if (isDashing) {
            // Dashing rotation, use the captured facing direction from dash start
            var dashTargetY = CalculateDashTargetYRotation();
            var dashTargetZ = CalculateDashTargetZRotation();
            var targetRot = Quaternion.Euler(0f, dashTargetY, dashTargetZ);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, dashTurnSpeed * Time.fixedDeltaTime));
        }
        else {
            // Normal rotation, update both Y and Z axes based on current input
            var targetY = CalculateTargetYRotation();
            var targetZ = CalculateTargetZRotation();
            var targetRot = Quaternion.Euler(0f, targetY, targetZ);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, turnSpeed * Time.fixedDeltaTime));
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
        dashAnimationComplete = true;
    }

    private void OnTriggerEnter(Collider other) {
        
        if (other.TryGetComponent<ICollectible>(out var collectible)) {
            OnTurtleCollectiblePickup?.Invoke(collectible);
            collectible.Collect(gameObject);
        }
    }
    
}
