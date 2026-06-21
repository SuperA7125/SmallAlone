using System.Collections;
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
    [Header("Rotation Settings")]
    public float RotationSpeed = 50f;
    public float RotationAmount = 90f;

    private Transform level;
    private Transform levelParent;
    private Transform rotationAnchor;
    private bool isRotating;

    public bool IsRotating => isRotating;

    private void Start()
    {
        level = LevelManager.Instance.LevelRoot;
        levelParent = LevelManager.Instance.LevelParentOfAll;
    }

    /// <summary>
    /// Call this from any valve/trigger's Interact(), passing the transform
    /// of the room that should act as the (stationary) pivot.
    /// </summary>
    public void ActivateRotation(Transform pivot)
    {
        if (isRotating || pivot == null) return;
        StartCoroutine(RotateSequence(pivot));
    }

    private IEnumerator RotateSequence(Transform pivot)
    {
        isRotating = true;

        // Pull the active room out of LevelRoot's hierarchy so it stays
        // completely still (no movement, no spin) while everything else
        // — every other room still nested under LevelRoot — rotates around it.
        Transform pivotOriginalParent = pivot.parent;
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

        // Hand LevelRoot back to its normal parent, keeping the world transform it ended up with.
        level.SetParent(levelParent, true);
        Destroy(rotationAnchor.gameObject);
        rotationAnchor = null;

        // Put the active room back where it was in the hierarchy. Its world
        // transform is unchanged (it never moved), even though LevelRoot has.
        pivot.SetParent(pivotOriginalParent, true);

        isRotating = false;
    }

    public object CaptureState() => new LevelTransformState(level.position, level.rotation);

    public void RestoreState(object state)
    {
        var data = (LevelTransformState)state;
        level.position = data.position;
        level.rotation = data.rotation;
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