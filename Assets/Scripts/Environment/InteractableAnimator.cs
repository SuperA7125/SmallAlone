using UnityEngine;


public class InteractableAnimator : MonoBehaviour
{
    public Animator animator;

    private static readonly int Activate = Animator.StringToHash("Activate");
    private static readonly int IsNearPlayer = Animator.StringToHash("IsPlayerNear");
    private static readonly int Respawn = Animator.StringToHash("Respawn");

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
}