using System.Linq;
using System.Collections.Generic;
using UnityEngine;
public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    private Dictionary<ISaveable, object> savedStates = new Dictionary<ISaveable, object>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void CaptureCheckpoint()
    {
        savedStates.Clear();
        foreach (ISaveable saveable in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISaveable>())
        {
            savedStates[saveable] = saveable.CaptureState();
        }
    }

    public void Respawn()
    {
        foreach (var kvp in savedStates)
            kvp.Key.RestoreState(kvp.Value);
    }
}
