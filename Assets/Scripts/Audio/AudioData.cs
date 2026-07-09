using UnityEngine;

/// <summary>
/// ScriptableObject asset that holds every audio clip and volume setting
/// in one place. Create one via Assets > Create > Audio > AudioData,
/// then drag it into AudioManager's Inspector field.
/// Every script that needs a clip references AudioManager.Instance.Data
/// rather than holding its own AudioClip fields.
/// </summary>
[CreateAssetMenu(fileName = "AudioData", menuName = "Audio/AudioData")]
public class AudioData : ScriptableObject
{
    [Header("Master Volume")]
    [Range(0f, 1f)] public float MasterVolume = 1f;
    [Range(0f, 1f)] public float SFXVolume = 1f;
    [Range(0f, 1f)] public float MusicVolume = 0.5f;

    [Header("Player SFX")]
    public AudioClip[] FootstepClips;
    public float FootstepInterval = 0.3f;
    public AudioClip JumpClip;
    public AudioClip LandClip;
    public AudioClip DeathClip;
    public AudioClip InteractClip;

    [Header("Player SFX Pitch Variation")]
    [Tooltip("±pitch range. 0.1 = subtle, 0.2 = noticeable. Keep below 0.3 to avoid sounding wrong.")]
    [Range(0f, 0.3f)] public float FootstepPitchVariation = 0.1f;
    [Range(0f, 0.3f)] public float JumpPitchVariation = 0.1f;
    [Range(0f, 0.3f)] public float LandPitchVariation = 0.15f;

    [Header("Interactable SFX")]
    public AudioClip ButtonActivateClip;
    public AudioClip ValveActivateClip;
    public AudioClip CheckpointActivateClip;
    public AudioClip CheckpointRespawnClip;

    [Header("Moveable SFX")]
    public AudioClip DoorMoveClip;
    public AudioClip PlatformMoveClip;

    [Header("Music")]
    public AudioClip GameplayMusic;
    public AudioClip RotationMusic;
    public float CrossfadeDuration = 1f;
}
