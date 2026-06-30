using System;
using UnityEngine;

public class ScrollingPlatform : Moveable
{


    private void Awake()
    {
        isActive = true;
    }


    public override void Move()
    {
        if (movingToEnd)
        {
            Vector3 target = EndPos;
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, target, MoveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.localPosition, target) <= Tolerance)
            {
                waitTimer = WaitTime;
                isWaiting = true;
            }
        }
        else
        {
            TeleportToStart();
        }
    }

    private void TeleportToStart()
    {
        transform.localPosition = StartPos;
        movingToEnd = true;
    }
}