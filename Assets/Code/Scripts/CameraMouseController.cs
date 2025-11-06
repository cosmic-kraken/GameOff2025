using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMouseController : MonoBehaviour
{
    [Header("Speed Settings")]
    public float moveSpeed = 5f;
    public float boostMultiplier = 3f;
    public float zoomSpeed = 10f;

    private PlayerControls controls;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private float zoomInput;
    private bool isLooking;

    private float pitch;
    private float yaw;

    private void OnEnable()
    {
        controls = new PlayerControls();

        controls.Camera.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Camera.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Camera.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Camera.Look.canceled += ctx => lookInput = Vector2.zero;

        controls.Camera.Zoom.performed += ctx => zoomInput = ctx.ReadValue<float>();
        controls.Camera.Zoom.canceled += ctx => zoomInput = 0f;

        controls.Camera.RightClick.started += ctx => isLooking = true;
        controls.Camera.RightClick.canceled += ctx => isLooking = false;

        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }


    private void Update()
    {
        // Mouse look while RMB is held
        if (isLooking)
        {
            yaw += lookInput.x * 0.2f;
            pitch -= lookInput.y * 0.2f;
            pitch = Mathf.Clamp(pitch, -89f, 89f);

            transform.eulerAngles = new Vector3(pitch, yaw, 0f);
        }

        float currentSpeed = moveSpeed;
        if (Keyboard.current.shiftKey.isPressed)
            currentSpeed *= boostMultiplier;

        // Zoom (scroll wheel)
        transform.position += transform.forward * zoomInput * zoomSpeed * Time.deltaTime;

        // WASD movement
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        transform.position += move * currentSpeed * Time.deltaTime;
    }


}
