using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

/// <summary>
/// Single source of truth for "reveal shot" camera moves. Owns the one
/// reveal Virtual Camera and one reusable CinemachineTargetGroup. Triggers
/// don't touch Cinemachine directly — they just call RequestReveal /
/// RequestRevealGroup on this, and this guarantees only one reveal plays
/// at a time (no overlap).
/// </summary>
public class CameraRevealBrain : MonoBehaviour
{
    public static CameraRevealBrain Instance { get; private set; }

    [Header("Reveal Camera")]
    [SerializeField] private CinemachineCamera revealCamera;
    [Tooltip("Used only for RequestRevealGroup (the 'zoom out' shot). " +
             "Leave its own members empty in the Inspector — they get " +
             "filled in/cleared at runtime per request.")]
    [SerializeField] private CinemachineTargetGroup targetGroup;
    [Tooltip("Must be higher than your gameplay camera's Priority.")]
    [SerializeField] private int activePriority = 20;

    private int originalPriority;
    private bool isRevealing;

    public bool IsRevealing => isRevealing;

    private void Awake()
    {
        Instance = this;
        originalPriority = revealCamera.Priority;
    }

    /// <summary>Look directly at a single point of interest.</summary>
    public void RequestReveal(Transform pointOfInterest, float duration, PlayerInput player)
    {
        if (isRevealing || pointOfInterest == null) return;
        StartCoroutine(RevealSingle(pointOfInterest, duration, player));
      
    }

    /// <summary>Zoom out to frame several points at once (e.g. player, next point, obstacles).</summary>
    public void RequestRevealGroup(IEnumerable<Transform> targets, float duration, PlayerInput player)
    {
        if (isRevealing || targets == null) return;
        StartCoroutine(RevealGroup(targets, duration,player));
    }

    private IEnumerator RevealSingle(Transform pointOfInterest, float duration, PlayerInput player)
    {
        isRevealing = true;

        revealCamera.Target.TrackingTarget = pointOfInterest;
        revealCamera.Priority = activePriority;

        yield return new WaitForSeconds(duration);

        revealCamera.Priority = originalPriority;
        isRevealing = false;
        player.StartInput();
    }

    private IEnumerator RevealGroup(IEnumerable<Transform> targets, float duration, PlayerInput player)
    {
        isRevealing = true;

        var addedTargets = new List<Transform>();
        foreach (Transform t in targets)
        {
            if (t == null) continue;
            targetGroup.AddMember(t, 1f, 1f);
            addedTargets.Add(t);
        }

        revealCamera.LookAt = targetGroup.transform;
        revealCamera.Priority = activePriority;

        yield return new WaitForSeconds(duration);

        revealCamera.Priority = originalPriority;

        // Clear the group so it's empty and ready for the next request.
        foreach (Transform t in addedTargets)
            targetGroup.RemoveMember(t);

        isRevealing = false;
        player.StartInput();
    }
}