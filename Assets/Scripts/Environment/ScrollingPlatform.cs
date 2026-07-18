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
        Vector2 worldTarget = movingToEnd ? GetWorldPosition(EndPos) : GetWorldPosition(StartPos);
        Vector2 newPos = Vector2.MoveTowards(rb.position, worldTarget, MoveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        if (Vector2.Distance(rb.position, worldTarget) <= Tolerance)
            rb.position = movingToEnd ? GetWorldPosition(StartPos) : GetWorldPosition(EndPos);
    }
}
