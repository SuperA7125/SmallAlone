using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler playerInput;
    private Animator animator;

    // Cached state, updated once per frame
    private bool wasGrounded;
    private bool wasAscending;

    private static readonly int Idle = Animator.StringToHash("Idle");
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int JumpUp = Animator.StringToHash("JumpUp");
    private static readonly int JumpDown = Animator.StringToHash("JumpDown");
    private static readonly int Land = Animator.StringToHash("Land");

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        wasGrounded = playerInput.GroundCheck();
    }

    private void Update()
    {
        // GroundCheck() = raw physical contact, used here on purpose instead
        // of IsGrounded, which has coyote time baked in and would delay the
        // jump-up animation by up to coyoteTime seconds after actually leaving
        // the ground.
        bool isGrounded = playerInput.GroundCheck();
        bool isAscending = playerInput.VerticalVelocity > 0f;

        if (!isGrounded)
        {
            if (wasGrounded)
            {
                // Edge: grounded -> airborne, this frame.
                animator.SetTrigger(JumpUp);
            }
            else if (wasAscending && !isAscending)
            {
                // Edge: reached the apex this frame, now falling.
                animator.SetTrigger(JumpDown);
            }
        }
        else if (!wasGrounded)
        {
            // Edge: airborne -> grounded, this frame.
            animator.SetTrigger(Land);
        }

        // CanMove gates this so the walk animation doesn't play while input
        // is frozen (e.g. during a room rotation), even if the player is
        // still holding a direction.
        bool isWalking = playerInput.CanMove && isGrounded &&
                          Mathf.Abs(playerInput.HorizontalMoveInput) > 0.1f;
        animator.SetBool(IsWalking, isWalking);

        wasGrounded = isGrounded;
        wasAscending = isAscending;
    }
}