using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool isActivated = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isActivated || !collision.CompareTag("Player")) return;

        isActivated = true;
        CheckpointManager.Instance.CaptureCheckpoint();
    }
}
