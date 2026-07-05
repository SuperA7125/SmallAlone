using UnityEngine;
public class ScrollingPlatform : Moveable
{
    protected override void Awake()
    {
        base.Awake();
        isActive = true;
    }

    protected override void Move()
    {
        Vector3 worldTarget = transform.parent != null
            ? transform.parent.TransformPoint(movingToEnd ? EndPos : StartPos)
            : (movingToEnd ? EndPos : StartPos);

        Vector2 newPos = Vector2.MoveTowards(rb.position, worldTarget, MoveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        if (Vector2.Distance(rb.position, worldTarget) <= Tolerance)
        {
            // Teleport instantly to the opposite end and keep moving —
            // seamless loop instead of reversing.
            rb.position = transform.parent != null
                ? transform.parent.TransformPoint(movingToEnd ? StartPos : EndPos)
                : (movingToEnd ? StartPos : EndPos);
        }
    }
}
