using UnityEngine;

/// <summary>
/// Drop this on a trigger collider at the point in the map where a reveal
/// shot should fire. Holds WHAT to show — either a single point of interest,
/// or a list of targets for a "zoom out and frame everyone" shot — and asks
/// CameraRevealBrain to handle the actual camera work.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PointOfInterestTrigger : MonoBehaviour, ISaveable
{
    [Header("What to show")]
    [Tooltip("Look directly at this point. Leave Group Targets empty if using this.")]
    [SerializeField] private Transform pointOfInterest;

    [Tooltip("For a 'zoom out' shot instead: list every object that should be " +
             "framed (player, next point, obstacles...). Leave Point Of Interest empty if using this.")]
    [SerializeField] private Transform[] groupTargets;

    [Header("Timing")]
    [SerializeField] private float revealDuration = 3f;

    [Header("Filtering")]
    [SerializeField] private string playerTag = "Player";

    // Persisted via ISaveable so it doesn't replay after a death/respawn reload.
    private bool hasBeenActivated;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasBeenActivated) return;
        if (!other.CompareTag(playerTag)) return;

        PlayerInput player = other.GetComponent<PlayerInput>();
        if (player == null) return;
        player.StopInput();

        hasBeenActivated = true;

        if (groupTargets != null && groupTargets.Length > 0)
            CameraRevealBrain.Instance.RequestRevealGroup(groupTargets, revealDuration, player);
        else
            CameraRevealBrain.Instance.RequestReveal(pointOfInterest, revealDuration, player);
        
    }

    public object CaptureState() => hasBeenActivated;

    public void RestoreState(object state) => hasBeenActivated = (bool)state;

}