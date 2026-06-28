using System.Collections;
using UnityEngine;

/// <summary>
/// Spins a valve's head/handle over the same duration the level rotation
/// takes, reading RotationSpeed/RotationAmount straight from the shared
/// RoomRotationController - but runs as its own independent coroutine, with
/// no live connection to the actual room rotation. Call PlayRotation()
/// directly from the valve's own Interact(), so only the valve actually
/// being used ever spins (no global event, no filtering needed).
/// </summary>
public class ValveHeadRotator : MonoBehaviour
{
    [SerializeField] private Transform headTransform;
    [Tooltip("How far the head itself rotates over one activation (independent of the level's own rotation amount).")]
    [SerializeField] private float headRotationAmount = 360f;
    [Tooltip("Optional manual override. Leave empty to use RoomRotationController.Instance.")]
    [SerializeField] private RoomRotationController controller;

    public void PlayRotation()
    {
        Debug.Log($"ValveHeadRotator: PlayRotation() called on {name}. Using controller: {(controller != null ? controller.name : "RoomRotationController.Instance")}");
        RoomRotationController activeController = controller != null ? controller : RoomRotationController.Instance;
        if (activeController == null || headTransform == null) return;

        StartCoroutine(RotateHead(activeController.RotationSpeed, activeController.RotationAmount));
    }

    private IEnumerator RotateHead(float speed, float levelRotationAmount)
    {
        float duration = levelRotationAmount / speed;
        float elapsed = 0f;
        Quaternion startRot = headTransform.localRotation;
        Quaternion endRot = startRot * Quaternion.Euler(0f, 0f, headRotationAmount);

        while (elapsed < duration)
        {
            headTransform.localRotation = Quaternion.Slerp(startRot, endRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        headTransform.localRotation = endRot;
    }
}