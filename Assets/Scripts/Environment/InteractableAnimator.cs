using UnityEngine;

public class InteractableAnimator : MonoBehaviour
{
    private Animator animator;

    private static readonly int Activate = Animator.StringToHash("Activate");
    private static readonly int IsNearPlayer = Animator.StringToHash("IsPlayerNear");
    private static readonly int Respawn = Animator.StringToHash("Respawn");
    private static readonly int Reset = Animator.StringToHash("Reset");

    public enum InteractableType { Button, Valve, Checkpoint }

    [Tooltip("Determines which clips from AudioData are used for this interactable.")]
    [SerializeField] private InteractableType interactableType;

    public event System.Action AnimationEvent;

    private void Awake() => animator = GetComponent<Animator>();

    public void PlayActivate()
    {
        animator.SetTrigger(Activate);
        AudioData data = AudioManager.Instance?.Data;
        if (data == null) return;
        AudioClip clip = interactableType switch
        {
            InteractableType.Button => data.ButtonActivateClip,
            InteractableType.Valve => data.ValveActivateClip,
            InteractableType.Checkpoint => data.CheckpointActivateClip,
            _ => null
        };
        AudioManager.Instance.PlaySFX(clip);
    }

    public void PlayRespawn()
    {
        animator.SetTrigger(Respawn);
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.Data?.CheckpointRespawnClip);
    }

    public void SetNearPlayer(bool isNear) => animator.SetBool(IsNearPlayer, isNear);
    public void RaiseAnimationEvent() => AnimationEvent?.Invoke();

    public void PlayReset()
    {
        animator.SetBool(IsNearPlayer, false);
        animator.ResetTrigger(Activate);
        animator.SetTrigger(Reset);
    }
}