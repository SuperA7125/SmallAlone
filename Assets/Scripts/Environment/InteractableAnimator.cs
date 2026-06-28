using UnityEngine;

/// <summary>
/// Generic interactableAnimator wrapper for any interactable object with a simple
/// Before -> During -> After lifecycle (a valve turning, a lever flipping,
/// a door opening, etc). Attach alongside whatever script implements
/// IInteractable for that object, and have it call PlayActivate() once,
/// at the moment the interaction happens.
///
/// "Before" is just the Animator's default state - no trigger needed.
/// "After" is reached automatically once the "During" clip finishes
/// playing, via a Has Exit Time transition in the Animator Controller -
/// no extra code needed for that step either.
/// </summary>
public class InteractableAnimator : MonoBehaviour
{
    private Animator animator;

    private static readonly int Activate = Animator.StringToHash("Activate");
    private static readonly int IsNearPlayer = Animator.StringToHash("IsPlayerNear");

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayActivate()
    {
        animator.SetTrigger(Activate);
    }

    public void SetNearPlayer(bool isNear)
    {
        animator.SetBool(IsNearPlayer, isNear);
    }
}