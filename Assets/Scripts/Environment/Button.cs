using System.Collections.Generic;
using UnityEngine;
public class Button : MonoBehaviour, IInteractable, ISaveable
{
    [Header("Button Settings")]
    private bool isActivated = false;
    [SerializeField] private InteractableAnimator interactableAnimator;
    [SerializeField] private string playerTag = "Player";
    public List<GameObject> objectsToActivate;

    public void Interact()
    {
        if (isActivated) return;

        Debug.Log("Button Pressed");
        isActivated = true;

        foreach (GameObject obj in objectsToActivate)
        {
            if (obj.TryGetComponent(out IMoveable moveable))
                moveable.ActivateMovement();
        }
        interactableAnimator?.PlayActivate();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            interactableAnimator?.SetNearPlayer(true);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            interactableAnimator?.SetNearPlayer(false);
    }

    // CaptureState is still called by CheckpointManager's generic loop,
    // but the value is intentionally ignored in RestoreState — the button
    // always resets fully on death, same as RoomRotationController.
    // The connected moveables (lifts, doors) have their own ISaveable and
    // restore themselves independently, so re-running Interact() here would
    // double-activate them.
    public object CaptureState() => isActivated;

    public void RestoreState(object state)
    {
        isActivated = false;
        interactableAnimator?.PlayReset();
    }
}