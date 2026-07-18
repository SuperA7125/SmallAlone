using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerInputHandler : MonoBehaviour, ISaveable
{
    [Header("Inputs")]
    public InputActionReference Move;
    public InputActionReference Interact;
    public InputActionReference Jump;
    public InputActionReference ToggleCameraZoom;
    public InputActionReference ResetLevel;
    public InputActionReference CheatMove;

    [Header("Stats")]
    public float MoveSpeed = 8f;
    public float Acceleration = 20f;
    [Tooltip("How fast the player decelerates when no input is held. Higher than acceleration for snappier stops.")]
    public float Deceleration = 30f;
    public float JumpForce = 10f;
    [Tooltip("Multiplier applied to gravity when falling, for a less floaty feel.")]
    public float FallGravityMultiplier = 2.5f;
    [Tooltip("How much vertical velocity is cut when jump is released early.")]
    [Range(0f, 1f)] public float JumpCutMultiplier = 0.5f;
    [Tooltip("How much horizontal control the player has while airborne (0 = none, 1 = full ground control).")]
    [Range(0f, 1f)] public float AirControlMultiplier = 0.5f;
    [Tooltip("Maximum downward velocity — prevents infinite fall speed.")]
    public float MaxFallSpeed = 20f;
    private bool canMove = true;
    [SerializeField] private int baseCameraZoom = 3;
    [SerializeField] private int zoomedOutCameraZoom = 7;

    [Header("Cheat Move (Showcase)")]
    [Tooltip("Movement speed while noclipping via CheatMove.")]
    public float CheatMoveSpeed = 15f;
    private bool isCheating = false;

    [Header("Ground Check")]
    public LayerMask GroundLayer;
    public Vector2 GroundCheckBoxSize = new Vector2(0.5f, 0.1f);
    public float CastDistance = 0.1f;

    [Header("Interactables Check")]
    public LayerMask InteractablesLayer;
    public Vector2 InteractablesCheckBoxSize = new Vector2(0.5f, 0.5f);
    public float InteractBoxYOffset = 0.5f;

    [Header("Animation Stats")]
    public bool IsGrounded => coyoteTimeCounter > 0;
    public bool CanMove => canMove;
    public float VerticalVelocity => rb.linearVelocity.y;
    public float HorizontalMoveInput => Move.action.ReadValue<Vector2>().x;

    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    private bool isFacingRight = true;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isZoomedOut = false;
    private PlayerAudioHandler audioHandler;
    private PlayerParticleHandler particleHandler;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        audioHandler = GetComponent<PlayerAudioHandler>();
        particleHandler = GetComponent<PlayerParticleHandler>();
    }

    private void OnEnable()
    {
        Move.action.Enable();
        Interact.action.Enable();
        Jump.action.Enable();
        ToggleCameraZoom.action.Enable();
        ResetLevel.action.Enable();
        CheatMove?.action.Enable();

        RoomRotationController.Instance.RotationStarted += StopInput;
        RoomRotationController.Instance.RotationEnded += StartInput;

        Interact.action.performed += OnInteract;
        ResetLevel.action.performed += ResetScene;
        ToggleCameraZoom.action.started += ZoomOut;
        ToggleCameraZoom.action.canceled += ZoomIn;
        Jump.action.started += OnJumpStart;
        Jump.action.canceled += OnJumpEnd;
    }

    private void OnDisable()
    {
        Interact.action.performed -= OnInteract;
        ResetLevel.action.performed -= ResetScene;
        ToggleCameraZoom.action.started -= ZoomOut;
        ToggleCameraZoom.action.canceled -= ZoomIn;
        Jump.action.started -= OnJumpStart;
        Jump.action.canceled -= OnJumpEnd;

        if (RoomRotationController.Instance != null)
        {
            RoomRotationController.Instance.RotationStarted -= StopInput;
            RoomRotationController.Instance.RotationEnded -= StartInput;
        }

        Move.action.Disable();
        Interact.action.Disable();
        Jump.action.Disable();
        ToggleCameraZoom.action.Disable();
        CheatMove?.action.Disable();
    }

    private void FixedUpdate()
    {
        HandleCheatMove();
        if (isCheating) return; // physics fully bypassed while noclipping

        if (!canMove) return;
        HandleMovement();
        ApplyFallGravity();

        if (GroundCheck())
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.fixedDeltaTime;
    }

    private void ApplyFallGravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (FallGravityMultiplier - 1f) * Time.fixedDeltaTime;
            if (rb.linearVelocity.y < -MaxFallSpeed)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -MaxFallSpeed);
        }
    }

    private void Flip(float dir)
    {
        if (isFacingRight && dir < 0 || !isFacingRight && dir > 0)
        {
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    public bool GroundCheck()
    {
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, GroundCheckBoxSize, 0f, Vector2.down, CastDistance, GroundLayer);
        return hit.collider != null;
    }

    #region INPUT HANDLERS
    private void HandleMovement()
    {
        float input = Move.action.ReadValue<Vector2>().x;
        float targetX = input * MoveSpeed;

        bool isGrounded = GroundCheck();
        float airMultiplier = isGrounded ? 1f : AirControlMultiplier;

        float delta = input != 0
            ? Acceleration * airMultiplier * Time.fixedDeltaTime
            : Deceleration * airMultiplier * Time.fixedDeltaTime;

        float newX = Mathf.MoveTowards(rb.linearVelocity.x, targetX, delta);
        rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
        Flip(input);
    }

    /// <summary>
    /// Showcase-only noclip movement. While CheatMove reports non-zero input,
    /// this disables the Rigidbody2D so the player can pass through walls and
    /// geometry freely. Releasing the input restores normal physics.
    /// </summary>
    private void HandleCheatMove()
    {
        if (CheatMove == null) return;

        Vector2 cheatInput = CheatMove.action.ReadValue<Vector2>();

        if (cheatInput.sqrMagnitude > 0.0001f)
        {
            if (!isCheating)
            {
                isCheating = true;
                rb.simulated = false; // disables collisions + gravity for this body
            }

            transform.position += (Vector3)(cheatInput.normalized * CheatMoveSpeed * Time.fixedDeltaTime);
        }
        else if (isCheating)
        {
            isCheating = false;
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero; // avoid inheriting stale velocity on re-entry
            coyoteTimeCounter = coyoteTime;   // don't immediately fall through a "not grounded" state
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (coyoteTimeCounter > 0)
        {
            Vector3 castPos = new Vector3(transform.position.x, transform.position.y + InteractBoxYOffset, transform.position.z);
            RaycastHit2D hit = Physics2D.BoxCast(castPos, InteractablesCheckBoxSize, 0f, Vector2.zero, 0f, InteractablesLayer);
            if (hit.collider != null)
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact();
                    audioHandler?.OnInteract();
                }
            }
        }
    }

    private void OnJumpStart(InputAction.CallbackContext context)
    {
        if (!canMove) return;
        if (coyoteTimeCounter > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpForce);
            audioHandler?.OnJump();
            particleHandler?.OnJump();
        }
    }

    private void OnJumpEnd(InputAction.CallbackContext context)
    {
        if (rb.linearVelocity.y > 0)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * JumpCutMultiplier);
        coyoteTimeCounter = 0;
    }

    private void ZoomOut(InputAction.CallbackContext context)
    {
        isZoomedOut = true;
        GlobalCameraBrain.Instance.ZoomCamera.Lens.OrthographicSize = zoomedOutCameraZoom;
        GlobalCameraBrain.Instance.ZoomCamera.Priority = 18;
    }

    private void ZoomIn(InputAction.CallbackContext context)
    {
        isZoomedOut = false;
        GlobalCameraBrain.Instance.GameplayCamera.Lens.OrthographicSize = baseCameraZoom;
        GlobalCameraBrain.Instance.ZoomCamera.Priority = 0;
    }

    private void ResetZoom()
    {
        isZoomedOut = false;
        GlobalCameraBrain.Instance.GameplayCamera.Lens.OrthographicSize = baseCameraZoom;
        GlobalCameraBrain.Instance.ZoomCamera.Priority = 0;
    }

    private void ResetScene(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void StartInput() => canMove = true;
    public void StopInput() => canMove = false;

    public void EndMovement()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    public void SetVisible(bool visible)
    {
        if (spriteRenderer != null)
            spriteRenderer.enabled = visible;
    }
    #endregion

    public object CaptureState() => transform.position;

    public void RestoreState(object state)
    {
        transform.position = (Vector3)state;
        rb.linearVelocity = Vector2.zero;
        ResetZoom();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            transform.position + Vector3.down * CastDistance,
            GroundCheckBoxSize
        );
        Vector3 castPos = new Vector3(transform.position.x, transform.position.y + InteractBoxYOffset, transform.position.z);
        Gizmos.DrawWireCube(castPos, InteractablesCheckBoxSize);
    }
}