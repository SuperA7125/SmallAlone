using UnityEngine;
public class Checkpoint : MonoBehaviour
{
    private bool isActivated = false;
    [SerializeField] private InteractableAnimator interactableAnimator;
    [Tooltip("Optional. If left empty, defaults to this object's own position.")]
    [SerializeField] private Transform spawnPoint;

    private void OnEnable()
    {
        if (interactableAnimator != null)
            interactableAnimator.AnimationEvent += RevealPlayer;
    }

    private void OnDisable()
    {
        if (interactableAnimator != null)
            interactableAnimator.AnimationEvent -= RevealPlayer;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isActivated || !collision.CompareTag("Player")) return;
        isActivated = true;

        Vector3 respawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;
        CheckpointManager.Instance.CaptureCheckpoint(interactableAnimator, respawnPosition);
        interactableAnimator?.PlayActivate();
    }

    private void RevealPlayer()
    {
        CheckpointManager.Instance.RevealPlayer();
    }
}