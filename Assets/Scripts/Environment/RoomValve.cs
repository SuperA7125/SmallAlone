using UnityEngine;
using static RoomRotationController;

/// <summary>
/// Lightweight trigger — no rotation logic lives here. Put one of these on
/// each valve/lever object. Drag in the (single, shared) RoomRotationController
/// in the Inspector, and this valve's own position is used as the pivot.
/// </summary>
public class RoomValve : MonoBehaviour, IInteractable, ISaveable
{
    [Header("Valve Settings")]
    [Tooltip("The object whose position acts as the pivot. Usually this valve itself.")]
    public GameObject Room;

    [Tooltip("The single shared controller that actually performs the rotation.")]
    [SerializeField] private RoomRotationController controller;
    [SerializeField] private ValveHeadRotator headRotator;

  
    public void Interact()
    {
        RoomRotationController activeController = controller != null
            ? controller
            : RoomRotationController.Instance;

        if (activeController == null)
        {
            Debug.LogError($"{name}: no RoomRotationController found.", this);
            return;
        }

        activeController.ActivateRotation(Room.transform);
        headRotator?.PlayRotation();
    }

    public object CaptureState() => new LevelTransformState(Room.transform.position,Room.transform.rotation);

    public void RestoreState(object state)
    {
        var savedState = (LevelTransformState)state;
        Room.transform.position = savedState.position;
        Room.transform.rotation = savedState.rotation;
    }
}