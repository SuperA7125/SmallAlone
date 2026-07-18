using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }
    private Dictionary<ISaveable, object> savedStates = new Dictionary<ISaveable, object>();
    private InteractableAnimator activeCheckpointAnimator;
    private PlayerInputHandler playerInputHandler;
    private Vector3 respawnLocalPosition;
    private bool hasRespawnPosition = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        foreach (ISaveable saveable in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISaveable>())
        {
            if (saveable is PlayerInputHandler player)
            {
                playerInputHandler = player;
                Transform levelRoot = LevelManager.Instance.LevelRoot;
                respawnLocalPosition = levelRoot.InverseTransformPoint(player.transform.position);
                hasRespawnPosition = true;
                savedStates[saveable] = player.CaptureState();
            }
            else
            {
                savedStates[saveable] = saveable.CaptureState();
            }
        }
    }

    public void CaptureCheckpoint(InteractableAnimator checkpointAnimator, Vector3 respawnPosition)
    {
        activeCheckpointAnimator = checkpointAnimator;

        Transform levelRoot = LevelManager.Instance.LevelRoot;
        respawnLocalPosition = levelRoot.InverseTransformPoint(respawnPosition);
        hasRespawnPosition = true;

        savedStates.Clear();
        foreach (ISaveable saveable in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISaveable>())
        {
            if (saveable is PlayerInputHandler player)
            {
                playerInputHandler = player;
                savedStates[saveable] = player.CaptureState();
            }
            else
            {
                savedStates[saveable] = saveable.CaptureState();
            }
        }
    }

    public void Respawn()
    {
        playerInputHandler?.SetVisible(false);
        playerInputHandler?.StopInput();

        // RoomRotationController MUST run first so LevelRoot is already
        // reset before any Moveable.RestoreState calls GetWorldPosition.
        // Without this ordering, doors/platforms convert their StartPos
        // using the pre-reset LevelRoot rotation and end up in the wrong place.
        foreach (var kvp in savedStates.OrderBy(x => x.Key is RoomRotationController ? 0 : 1))
            kvp.Key.RestoreState(kvp.Value);

        // Apply player position after LevelRoot has been reset.
        if (hasRespawnPosition && playerInputHandler != null)
        {
            Transform levelRoot = LevelManager.Instance.LevelRoot;
            playerInputHandler.transform.position = levelRoot.TransformPoint(respawnLocalPosition);
            playerInputHandler.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }

        activeCheckpointAnimator?.PlayRespawn();
    }

    public void RevealPlayer()
    {
        playerInputHandler?.SetVisible(true);
        playerInputHandler?.StartInput();
    }
}