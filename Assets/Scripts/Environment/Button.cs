using System.Collections.Generic;
using UnityEngine;

public class Button : MonoBehaviour, IInteractable
{

    [Header("Button Settings")]

    public List<GameObject> objectsToActivate;

    public void Interact()
    {
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj.TryGetComponent(out IMoveable moveable))
            {
                moveable.ActivateMovement();
            }
        }
    }

}
