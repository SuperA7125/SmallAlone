using UnityEngine;
using System.Collections;
using System;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler playerInput;
    private Animator animator;
    private PlayerHealth playerHealth;

    [Header("Landing Impact")]
    [Tooltip("Total time movement stays locked after landing (should be >= hit stop duration).")]
    [SerializeField] private float landingMoveLockDuration = 0.2f;

    // Cached state, updated once per frame
    private bool wasGrounded;
    private bool wasAscending;

    private static readonly int Idle = Animator.StringToHash("Idle");
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int JumpUp = Animator.StringToHash("JumpUp");
    private static readonly int JumpDown = Animator.StringToHash("JumpDown");
    private static readonly int Land = Animator.StringToHash("Land");
    private static readonly int Die = Animator.StringToHash("Death");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void OnEnable()
    {
        playerHealth.Died += HandleDeath;
        playerHealth.Respawned += HandleRespawn;
    }
    private void OnDisable()
    {
        playerHealth.Died -= HandleDeath;
        playerHealth.Respawned -= HandleRespawn;
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
                // Just left the ground this frame. If we're actually moving
                // upward, it was a jump -> JumpUp. If not (walked off a ledge,
                // gravity hasn't built up downward speed yet either), it's
                // already a fall -> JumpDown.
                animator.SetTrigger(isAscending ? JumpUp : JumpDown);
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
            StartCoroutine(LandingImpactSequence());
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


private void HandleDeath()
    {
        animator.SetTrigger(Die);
        playerInput.EndMovement();
        playerInput.StopInput();
    }

private void HandleRespawn()
    {
        animator.SetTrigger(Idle);
        playerInput.StartInput();
    }

    private IEnumerator LandingImpactSequence()
    {
        playerInput.StopInput();
        yield return new WaitForSeconds(landingMoveLockDuration);
        playerInput.StartInput();
    }
}