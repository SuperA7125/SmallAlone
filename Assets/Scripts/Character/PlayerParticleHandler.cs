using UnityEngine;

/// <summary>
/// Controls the player's slime particles for walking, jumping and landing.
/// Attach to the player. Assign child ParticleSystem references in the
/// Inspector — configure all visual settings there, this script only
/// handles direction, emission timing and bursts.
/// </summary>
public class PlayerParticleHandler : MonoBehaviour
{
    [Header("Walk Particles")]
    [SerializeField] private ParticleSystem slimeParticles;
    [SerializeField] private float emissionRate = 15f;
    [SerializeField] private float spreadSpeed = 2f;

    [Header("Jump Particles")]
    [SerializeField] private ParticleSystem jumpParticles;
    [Tooltip("How many particles burst on jump.")]
    [SerializeField] private int jumpBurstCount = 10;

    [Header("Land Particles")]
    [SerializeField] private ParticleSystem landParticles;
    [Tooltip("How many particles burst on landing.")]
    [SerializeField] private int landBurstCount = 20;

    private PlayerInputHandler playerInput;
    private ParticleSystem.EmissionModule emission;
    private ParticleSystem.VelocityOverLifetimeModule velocity;

    private bool wasGrounded;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInputHandler>();

        emission = slimeParticles.emission;
        velocity = slimeParticles.velocityOverLifetime;

        emission.enabled = true;
        emission.rateOverTime = 0f;
        velocity.enabled = true;
    }

    private void Start()
    {
        wasGrounded = playerInput.GroundCheck();
    }

    private void Update()
    {
        bool isGrounded = playerInput.GroundCheck();
        float input = playerInput.HorizontalMoveInput;
        bool isWalking = isGrounded && playerInput.CanMove && Mathf.Abs(input) > 0.1f;

        // Walk particles
        emission.rateOverTime = isWalking ? emissionRate : 0f;
        if (isWalking)
        {
            float direction = -Mathf.Sign(input);
            velocity.x = new ParticleSystem.MinMaxCurve(direction * spreadSpeed);
        }

        // Land burst
        if (!wasGrounded && isGrounded)
            landParticles?.Emit(landBurstCount);

        wasGrounded = isGrounded;
    }

    // Called directly from PlayerInputHandler's OnJumpStart
    // alongside audioHandler?.OnJump()
    public void OnJump()
    {
        jumpParticles?.Emit(jumpBurstCount);
    }
}