using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour, IInteractable , ISaveable
{

    [Header("Button Settings")]

    private bool isActived = false;
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

        isActived = true;
    }

    public object CaptureState() => isActived;

    public void RestoreState(object state)
    {
        isActived = (bool)state;
        if (isActived)
            Interact(); 
    }
}
