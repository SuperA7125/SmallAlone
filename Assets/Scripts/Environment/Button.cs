using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour, IInteractable , ISaveable
{

    [Header("Button Settings")]

    private bool isActived = false;
    [SerializeField] private InteractableAnimator interactableAnimator;
    [SerializeField] private string playerTag = "Player";
    public List<GameObject> objectsToActivate;

    public void Interact()
    {
        Debug.Log("Button Pressed");
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj.TryGetComponent(out IMoveable moveable))
            {
                moveable.ActivateMovement();
            }
        }
        interactableAnimator?.PlayActivate();
        isActived = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            interactableAnimator.SetNearPlayer(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            interactableAnimator.SetNearPlayer(false);
    }

    public object CaptureState() => isActived;

    public void RestoreState(object state)
    {
        isActived = (bool)state;
        if (isActived)
            Interact(); 
    }
}
