using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerAudioHandler : MonoBehaviour
{
    private PlayerInputHandler playerInput;
    private PlayerHealth playerHealth;
    private bool wasGrounded;
    private float footstepTimer;

    // Dedicated AudioSource for footsteps so each new step interrupts
    // the previous one instead of stacking on a fresh pool source.
    private AudioSource footstepSource;

    [SerializeField][Range(0f, 1f)] private float footstepVolume = 0.4f;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInputHandler>();
        playerHealth = GetComponent<PlayerHealth>();

        footstepSource = gameObject.AddComponent<AudioSource>();
        footstepSource.playOnAwake = false;
        footstepSource.loop = false;
    }

    private void OnEnable() => playerHealth.Died += OnDied;
    private void OnDisable() => playerHealth.Died -= OnDied;

    private void Start() => wasGrounded = playerInput.GroundCheck();

    private void Update()
    {
        AudioData data = AudioManager.Instance?.Data;
        if (data == null) return;

        bool isGrounded = playerInput.GroundCheck();

        if (!wasGrounded && isGrounded)
            AudioManager.Instance.PlaySFX(data.LandClip, 1f, data.LandPitchVariation);

        if (isGrounded && playerInput.CanMove &&
            Mathf.Abs(playerInput.HorizontalMoveInput) > 0.1f)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                PlayFootstep(data);
                footstepTimer = data.FootstepInterval;
            }
        }
        else
        {
            footstepTimer = 0f;
        }

        wasGrounded = isGrounded;
    }

    private void PlayFootstep(AudioData data)
    {
        if (data.FootstepClips == null || data.FootstepClips.Length == 0) return;

        AudioClip clip = data.FootstepClips[Random.Range(0, data.FootstepClips.Length)];
        if (clip == null) return;

        // Stop the previous footstep before playing the new one —
        // this is what prevents overlap on rapid direction changes.
        footstepSource.Stop();
        footstepSource.clip = clip;
        footstepSource.volume = footstepVolume * data.SFXVolume * data.MasterVolume;
        footstepSource.pitch = 1f + Random.Range(-data.FootstepPitchVariation, data.FootstepPitchVariation);
        footstepSource.Play();
    }

    public void OnJump() => AudioManager.Instance.PlaySFX(
        AudioManager.Instance.Data?.JumpClip, 1f,
        AudioManager.Instance.Data?.JumpPitchVariation ?? 0f);

    public void OnInteract() => AudioManager.Instance.PlaySFX(AudioManager.Instance.Data?.InteractClip);

    private void OnDied() => AudioManager.Instance.PlaySFX(AudioManager.Instance.Data?.DeathClip);
}