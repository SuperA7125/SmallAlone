using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        SubscribeToRotation();
        PlayGameplayMusic();
    }

    private void OnEnable() => SubscribeToRotation();
    private void OnDisable() => UnsubscribeFromRotation();

    private void SubscribeToRotation()
    {
        if (RoomRotationController.Instance == null) return;
        RoomRotationController.Instance.RotationStarted -= OnRotationStarted;
        RoomRotationController.Instance.RotationEnded -= OnRotationEnded;
        RoomRotationController.Instance.RotationStarted += OnRotationStarted;
        RoomRotationController.Instance.RotationEnded += OnRotationEnded;
    }

    private void UnsubscribeFromRotation()
    {
        if (RoomRotationController.Instance == null) return;
        RoomRotationController.Instance.RotationStarted -= OnRotationStarted;
        RoomRotationController.Instance.RotationEnded -= OnRotationEnded;
    }

    public void PlayGameplayMusic() =>
        AudioManager.Instance.PlayMusic(AudioManager.Instance.Data?.GameplayMusic);

    private void OnRotationStarted() =>
        AudioManager.Instance.PlayMusic(AudioManager.Instance.Data?.RotationMusic);

    private void OnRotationEnded() => PlayGameplayMusic();
}