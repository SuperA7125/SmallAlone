using UnityEngine;


public class InteractableAnimator : MonoBehaviour
{
    public Animator animator;

    private static readonly int Activate = Animator.StringToHash("Activate");
    private static readonly int IsNearPlayer = Animator.StringToHash("IsPlayerNear");
    private static readonly int Respawn = Animator.StringToHash("Respawn");
    private static readonly int Reset = Animator.StringToHash("Reset");

    public event System.Action AnimationEvent;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayActivate()
    {
        animator.SetTrigger(Activate);
    }

    public void PlayRespawn()
    {
        animator.SetTrigger(Respawn);
    }

    public void SetNearPlayer(bool isNear)
    {
        animator.SetBool(IsNearPlayer, isNear);
    }

    public void RaiseAnimationEvent()
    {
        AnimationEvent?.Invoke();
    }

    public void PlayReset()
    {
        animator.SetBool(IsNearPlayer, false);
        animator.ResetTrigger(Activate);
        animator.SetTrigger(Reset);
    }
}