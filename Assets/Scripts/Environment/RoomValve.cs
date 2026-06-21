using UnityEngine;

/// <summary>
/// Lightweight trigger — no rotation logic lives here. Put one of these on
/// each valve/lever object. Drag in the (single, shared) RoomRotationController
/// in the Inspector, and this valve's own position is used as the pivot.
/// </summary>
public class RoomValve : MonoBehaviour, IInteractable
{
    [Header("Valve Settings")]
    [Tooltip("The object whose position acts as the pivot. Usually this valve itself.")]
    public GameObject Room;

    [Tooltip("The single shared controller that actually performs the rotation.")]
    [SerializeField] private RoomRotationController controller;

    public void Interact()
    {
        controller.ActivateRotation(Room.transform);
    }
}