using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Single source of truth for rotating the level around an external pivot.
/// Attach this ONCE — e.g. to your Room container, or a dedicated empty
/// manager object — and have every valve/trigger call ActivateRotation()
/// on it directly, instead of each valve owning its own copy of this logic.
///
/// Expected hierarchy: every Room (and everything inside it) lives as a
/// child of LevelRoot, including the room that contains the valve currently
/// being activated. That active room is automatically pulled out of
/// LevelRoot for the duration of the rotation (so it stays completely
/// still) and slotted back in afterwards.
/// </summary>
public class RoomRotationController : MonoBehaviour, ISaveable
{

    private Transform level;
    private Transform levelParent;  

    [Header("Rotation Settings")]
    public float RotationSpeed = 50f;
    public float RotationAmount = 90f;

    /// <summary>Global access point, e.g. RoomRotationController.Instance.IsRotating</summary>
    public static RoomRotationController Instance { get; private set; }

    /// <summary>Fires the instant a rotation begins.</summary>
    public event System.Action RotationStarted;

    /// <summary>Fires the instant a rotation finishes.</summary>
    public event System.Action RotationEnded;

    [Header("Rooms")]
    [Tooltip("Every room that can act as a rotation pivot. Their initial " +
             "local transforms are captured automatically at Start.")]
    [SerializeField] private List<Transform> rooms = new List<Transform>();

    // Initial local transforms captured once at Start, before any rotation.
    // Used to reset all rooms on respawn, not just the last one used.
    private readonly Dictionary<Transform, (Vector3 localPos, Quaternion localRot)> initialRoomTransforms
        = new Dictionary<Transform, (Vector3, Quaternion)>();
    private Transform rotationAnchor;
    private bool isRotating;

    // Captured once, the very first frame, before any rotation can happen.
    // RestoreState always resets to this on death — rooms are meant to
    // always reset to their default orientation, not persist whatever
    // rotation existed when the last checkpoint was touched.
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    // Tracked so RestoreState can force-clean a rotation that gets
    // interrupted mid-flight (e.g. player dies while a room is rotating).
    private Transform activePivot;
    private Transform activePivotOriginalParent;
    private Coroutine activeRotationCoroutine;

    public bool IsRotating => isRotating;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        level = LevelManager.Instance.LevelRoot;
        levelParent = LevelManager.Instance.LevelParentOfAll;

        initialPosition = level.position;
        initialRotation = level.rotation;

        // Capture every registered room's local transform now, before any
        // rotation happens. RestoreState uses this to reset all of them,
        // not just the last one that was used.
        foreach (Transform room in rooms)
        {
            if (room != null)
                initialRoomTransforms[room] = (room.localPosition, room.localRotation);
        }
    }

    /// <summary>
    /// Call this from any valve/trigger's Interact(), passing the transform
    /// of the room that should act as the (stationary) pivot.
    /// </summary>
    public void ActivateRotation(Transform pivot)
    {
        if (isRotating || pivot == null) return;
        activeRotationCoroutine = StartCoroutine(RotateSequence(pivot));
    }

    private IEnumerator RotateSequence(Transform pivot)
    {
        isRotating = true;
        RotationStarted?.Invoke();

        // Pull the active room out of LevelRoot's hierarchy so it stays
        // completely still (no movement, no spin) while everything else
        // — every other room still nested under LevelRoot — rotates around it.
        activePivot = pivot;
        activePivotOriginalParent = pivot.parent;
        pivot.SetParent(levelParent, true); // true = keep its current world transform

        rotationAnchor = new GameObject("RotationAnchor (temp)").transform;
        rotationAnchor.SetParent(levelParent, false);
        rotationAnchor.position = pivot.position;
        level.SetParent(rotationAnchor, true); // true = keep LevelRoot's current world transform

        float elapsedTime = 0f;
        float duration = RotationAmount / RotationSpeed;
        Quaternion startRot = rotationAnchor.localRotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, 0, RotationAmount);

        while (elapsedTime < duration)
        {
            rotationAnchor.localRotation = Quaternion.Slerp(startRot, endRot, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        rotationAnchor.localRotation = endRot;

        FinishRotationCleanup();

        isRotating = false;
        RotationEnded?.Invoke();
    }

    /// <summary>
    /// Reparents the active room back and destroys the temp anchor. Shared
    /// between the coroutine's normal completion and RestoreState's forced
    /// cleanup, so both paths leave the hierarchy in the same valid state.
    /// </summary>
    private void FinishRotationCleanup()
    {
        if (rotationAnchor != null)
        {
            if (level != null)
                level.SetParent(levelParent, true);
            Destroy(rotationAnchor.gameObject);
            rotationAnchor = null;
        }

        if (activePivot != null)
        {
            activePivot.SetParent(activePivotOriginalParent, true);
            activePivot = null;
        }
    }

    // Still implemented so CheckpointManager's generic ISaveable loop doesn't
    // need a special case — the captured value just isn't used below, since
    // rotation always resets to its original state instead of whatever was
    // saved at the last checkpoint.
    public object CaptureState() => new LevelTransformState(level.position, level.rotation);

    public void RestoreState(object state)
    {
        // If a rotation was interrupted (e.g. player died mid-rotation),
        // the coroutine is still running and the active room may still be
        // detached from LevelRoot. Force-stop and clean that up first,
        // otherwise the coroutine keeps fighting the reset value next frame,
        // and/or a room is left permanently detached — either of which can
        // softlock future valve interactions (isRotating would also never
        // reset).
        if (isRotating)
        {
            if (activeRotationCoroutine != null)
                StopCoroutine(activeRotationCoroutine);

            FinishRotationCleanup();
            isRotating = false;
            RotationEnded?.Invoke();
        }

        // Always reset to the room's original orientation on death/respawn,
        // not whatever rotation existed when the checkpoint was touched.
        level.position = initialPosition;
        level.rotation = initialRotation;

        // Reset every registered room's local transform so none of them
        // keep their post-rotation offset inside LevelRoot.
        foreach (var kvp in initialRoomTransforms)
        {
            if (kvp.Key != null)
            {
                kvp.Key.localPosition = kvp.Value.localPos;
                kvp.Key.localRotation = kvp.Value.localRot;
            }
        }
    }

    [System.Serializable]
    public struct LevelTransformState
    {
        public Vector3 position;
        public Quaternion rotation;

        public LevelTransformState(Vector3 position, Quaternion rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }
    }
}