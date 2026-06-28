using System.Linq;
using System.Collections.Generic;
using UnityEngine;
public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }
    private Dictionary<ISaveable, object> savedStates = new Dictionary<ISaveable, object>();
    private InteractableAnimator activeCheckpointAnimator;
    private PlayerInputHandler playerInputHandler;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    public void CaptureCheckpoint(InteractableAnimator checkpointAnimator, Vector3 respawnPosition)
    {
        activeCheckpointAnimator = checkpointAnimator;

        savedStates.Clear();
        foreach (ISaveable saveable in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISaveable>())
        {
            if (saveable is PlayerInputHandler player)
            {
                // Respawn at the checkpoint's own position, not wherever the
                // player happened to be standing when they triggered it.
                playerInputHandler = player;
                savedStates[saveable] = respawnPosition;
            }
            else
            {
                savedStates[saveable] = saveable.CaptureState();
            }
        }
    }
    public void Respawn()
    {
        // Hide the player before the teleport happens, so the position jump
        // is invisible. The checkpoint's own Respawn animation is responsible
        // for revealing them again via RevealPlayer(), called as an
        // Animation Event near the end of that clip.
        playerInputHandler?.StopInput();
        playerInputHandler?.SetVisible(false);

        foreach (var kvp in savedStates)
            kvp.Key.RestoreState(kvp.Value);

        activeCheckpointAnimator?.PlayRespawn();
    }

    // Called via Checkpoint.RevealPlayer(), which is itself called as an
    // Animation Event on the checkpoint's Respawn clip.
    public void RevealPlayer()
    {
        playerInputHandler?.SetVisible(true);
        playerInputHandler?.StartInput();
    }
}