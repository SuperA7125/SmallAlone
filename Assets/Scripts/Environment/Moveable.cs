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
        rb.position = transform.TransformPoint(StartPos);
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
        Vector3 worldTarget = transform.parent != null
            ? transform.parent.TransformPoint(movingToEnd ? EndPos : StartPos)
            : (movingToEnd ? EndPos : StartPos);

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

    public virtual object CaptureState() => new MoveableState
    {
        position = transform.localPosition,
        isActive = isActive,
        movingToEnd = movingToEnd
    };

    public virtual void RestoreState(object state)
    {
        var s = (MoveableState)state;
        rb.position = transform.parent != null
            ? transform.parent.TransformPoint(s.position)
            : (Vector3)(Vector2)s.position;
        isActive = s.isActive;
        movingToEnd = s.movingToEnd;
        isWaiting = false;
        waitTimer = 0f;
        rb.linearVelocity = Vector2.zero;
    }
}