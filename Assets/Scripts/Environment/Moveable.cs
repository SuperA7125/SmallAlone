using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Moveable : MonoBehaviour, IMoveable, ISaveable
{
    [Header("Moveable Settings")]
    public Vector3 StartPos;
    public Vector3 EndPos;
    public float MoveSpeed;
    public float Tolerance = 0.01f;
    public float WaitTime = 2f;

    public enum MoveableType { Platform, Door }
    [SerializeField] private MoveableType moveableType = MoveableType.Platform;

    [Tooltip("If true, always resets to StartPos and deactivates on death regardless " +
             "of checkpoint state. Use for doors that should close on respawn.")]
    [SerializeField] private bool resetOnDeath = false;

    protected float waitTimer = 0f;
    protected bool isWaiting = false;
    protected bool isActive = false;
    protected bool movingToEnd = true;

    protected Rigidbody2D rb;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    protected virtual void Start()
    {
        rb.position = GetWorldPosition(StartPos);
    }

    // Converts a local position (defined relative to LevelRoot) to world
    // space each time it's called, so it stays correct after LevelRoot rotates.
    protected Vector2 GetWorldPosition(Vector3 localPos)
    {
        Transform levelRoot = LevelManager.Instance.LevelRoot;
        return levelRoot != null
            ? (Vector2)levelRoot.TransformPoint(localPos)
            : (Vector2)localPos;
    }

    protected virtual void FixedUpdate()
    {
        if (!isActive || isWaiting)
        {
            if (isWaiting)
            {
                waitTimer -= Time.fixedDeltaTime;
                if (waitTimer <= 0)
                {
                    movingToEnd = !movingToEnd;
                    waitTimer = 0;
                    isWaiting = false;
                }
            }
            rb.linearVelocity = Vector2.zero;
            return;
        }
        Move();
    }

    protected virtual void Move()
    {
        Vector2 worldTarget = movingToEnd ? GetWorldPosition(EndPos) : GetWorldPosition(StartPos);
        Vector2 newPos = Vector2.MoveTowards(rb.position, worldTarget, MoveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        if (Vector2.Distance(rb.position, worldTarget) <= Tolerance && WaitTime > 0)
        {
            waitTimer = WaitTime;
            isWaiting = true;
        }
    }

    public void ActivateMovement()
    {
        isActive = true;
        if (AudioManager.Instance?.Data != null)
        {
            AudioClip clip = moveableType == MoveableType.Door
                ? AudioManager.Instance.Data.DoorMoveClip
                : AudioManager.Instance.Data.PlatformMoveClip;
            AudioManager.Instance.PlaySFX(clip);
        }
    }

    [System.Serializable]
    private struct MoveableState
    {
        public Vector3 position;
        public bool isActive;
        public bool movingToEnd;
    }

    public virtual object CaptureState()
    {
        // Store position in LevelRoot local space so it remains valid
        // after LevelRoot resets its rotation on death.
        Transform levelRoot = LevelManager.Instance.LevelRoot;
        Vector3 localPos = levelRoot != null
            ? levelRoot.InverseTransformPoint(rb.position)
            : (Vector3)(Vector2)rb.position;

        return new MoveableState
        {
            position = localPos,
            isActive = isActive,
            movingToEnd = movingToEnd
        };
    }

    public virtual void RestoreState(object state)
    {
        if (resetOnDeath)
        {
            rb.position = GetWorldPosition(StartPos);
            isActive = false;
            movingToEnd = true;
        }
        else
        {
            var s = (MoveableState)state;
            rb.position = GetWorldPosition(s.position);
            isActive = s.isActive;
            movingToEnd = s.movingToEnd;
        }

        isWaiting = false;
        waitTimer = 0f;
        rb.linearVelocity = Vector2.zero;
    }
}