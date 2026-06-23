using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private PlayerInputHandler playerInput;


    private Animator animator;

    private bool wasGrounded => playerInput.IsGrounded;

    private static readonly int Idle = Animator.StringToHash("Idle");
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");
    private static readonly int JumpUp = Animator.StringToHash("JumpUp");
    private static readonly int JumpDown = Animator.StringToHash("JumpDown");
    private static readonly int Land = Animator.StringToHash("Land");

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

}
