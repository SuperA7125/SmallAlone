
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Rendering.ShadowCascadeGUI;


[RequireComponent(typeof(PlayerHealth))]
public class PlayerInputHandler : MonoBehaviour , ISaveable
{

    [Header("Inputs")]
    public InputActionReference Move;
    public InputActionReference Interact;
    public InputActionReference Jump;
    public InputActionReference ToggleCameraZoom;

    [Header("Stats")]

    public float MoveSpeed = 5f;
    public float JumpForce = 5f;
    private bool canMove = true;
    [SerializeField] private int baseCameraZoom = 3;
    [SerializeField] private int zoomedOutCameraZoom = 7;

    [Header("Ground Check")]
    public LayerMask GroundLayer;
    public Vector2 GroundCheckBoxSize = new Vector2(0.5f, 0.1f);
    public float CastDistance = 0.1f;

    [Header("Interactables Check")]
    public LayerMask InteractablesLayer;
    public Vector2 InteractablesCheckBoxSize = new Vector2(0.5f, 0.5f);
    public float InteractBoxYOffset = 0.5f;

    public bool IsGrounded => coyoteTimeCounter > 0;

    //Private stats
    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    private bool isFacingRight = true;
    [Header("References")]

    private Rigidbody2D rb;
    

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void OnEnable()
    {
        Move.action.Enable();
        Interact.action.Enable();
        Jump.action.Enable();
        ToggleCameraZoom.action.Enable();

        RoomRotationController.Instance.RotationStarted += StopInput;
        RoomRotationController.Instance.RotationEnded += StartInput;

        Interact.action.performed += OnInteract;
        ToggleCameraZoom.action.started += ZoomOut;
        ToggleCameraZoom.action.canceled += ZoomIn;
        Jump.action.started += OnJumpStart;
        Jump.action.canceled += OnJumpEnd;
    }


    private void OnDisable()
    {
        Interact.action.performed -= OnInteract;
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
    }

 
    private void FixedUpdate()
    {
        if (!canMove) return;
        HandleMovement();

        if (GroundCheck())
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
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
        float newX = Mathf.MoveTowards(rb.linearVelocity.x, targetX, MoveSpeed * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
        Flip(input);
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (coyoteTimeCounter > 0)
        {
            Debug.Log("Interacting with object");
            Vector3 castPos = new Vector3(transform.position.x, transform.position.y + InteractBoxYOffset, transform.position.z);
            RaycastHit2D hit = Physics2D.BoxCast(castPos, InteractablesCheckBoxSize, 0f, Vector2.zero, 0f, InteractablesLayer);
            if (hit.collider != null)
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact();
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
        }
    }

    private void OnJumpEnd(InputAction.CallbackContext context)
    {
        if (rb.linearVelocity.y > 0)
        { 
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); 
        }

        coyoteTimeCounter = 0; 
    }


    private void ZoomOut(InputAction.CallbackContext context)
    {
        GlobalCameraBrain.Instance.ZoomCamera.Lens.OrthographicSize = zoomedOutCameraZoom;
        GlobalCameraBrain.Instance.ZoomCamera.Priority = 18;
    }
    private void ZoomIn(InputAction.CallbackContext context)
    {
        GlobalCameraBrain.Instance.GameplayCamera.Lens.OrthographicSize = baseCameraZoom;
        GlobalCameraBrain.Instance.ZoomCamera.Priority = 0;
    }


    public void StartInput()
    {
        canMove = true;
    }

    public void StopInput()
    {
        canMove = false;
    }


    #endregion

    public object CaptureState() => transform.position;

    public void RestoreState(object state)
    {
        transform.position = (Vector3)state;
        rb.linearVelocity = Vector2.zero;
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
