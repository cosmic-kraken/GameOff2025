using UnityEngine;

public class TurtleController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float swimSpeed = 5f;
    [SerializeField] private float turnSpeed = 10f;
    
    [Header("Dash")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private ParticleSystem dashParticles;
    
    private Rigidbody rb;
    private PlayerControls controls;
    
    private Vector3 moveInput;
    private Vector3 moveDir => new Vector3(moveInput.x, moveInput.y, 0f).normalized;
    private float lastHorizontalInput = 1f;
    private float targetY;
    private float targetZ;
    
    private bool wasDashPressed;
    private float dashCooldownTimer;

    private bool isDashing;
    private float dashTimer;
    private Vector3 dashDirection = Vector3.zero;
    private Quaternion dashRotation;

    
    private void Awake() {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
        dashParticles.Clear();
        dashParticles.Stop();
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
        HandleInput();
        HandleDashTimers();
    }
    
    private void FixedUpdate() {
        HandleDash();
        HandleSwim();
        HandleRotation();
    }

    private void HandleInput() {
        moveInput = controls.Player.Move.ReadValue<Vector2>();
        
        if (Mathf.Abs(moveInput.x) > 0.01f) {
            lastHorizontalInput = Mathf.Sign(moveInput.x);
        }
        // Dash input
        if (!isDashing && dashCooldownTimer <= 0f && controls.Player.Dash.WasPressedThisDynamicUpdate()) {
            StartDash();
        }
    }

    private void StartDash() {
        isDashing = true;
        dashTimer = dashDuration;
        dashDirection = moveDir.sqrMagnitude > 0.01f ? moveDir : (lastHorizontalInput > 0 ? Vector3.right : Vector3.left);
        dashRotation = rb.rotation;
        dashParticles.Clear();
        dashParticles.Stop();
        dashParticles.Play();
    }

    private void HandleDashTimers() {
        // Update dash timer
        if (isDashing) {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f) {
                // End dash & start cooldown
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
        
        rb.linearVelocity = moveDir * swimSpeed;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, 0f);
    }

    private void HandleDash() {
        if (!isDashing) return;
        
        rb.MoveRotation(dashRotation);
        rb.linearVelocity = dashDirection.normalized * dashSpeed;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, 0f);
    }

    private void CalculateTargetYRotation() {
        targetY = (lastHorizontalInput < 0) ? -180f : 0f;
    }

    private void CalculateTargetZRotation() {
        if (Mathf.Abs(moveInput.x) > 0.01f || Mathf.Abs(moveInput.y) > 0.01f) {
            if (lastHorizontalInput > 0) {
                targetZ = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
            } else {
                targetZ = Mathf.Atan2(moveInput.y, -moveInput.x) * Mathf.Rad2Deg;
            }
        } else {
            targetZ = 0f;
        }
    }

    private void HandleRotation() {
        if (isDashing) return;
        
        CalculateTargetYRotation();
        CalculateTargetZRotation();
        
        Quaternion targetRot = Quaternion.Euler(0f, targetY, targetZ);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, turnSpeed * Time.fixedDeltaTime));
    }
    
    
}
