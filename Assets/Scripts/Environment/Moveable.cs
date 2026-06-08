using UnityEngine;

public class Moveable : MonoBehaviour , IMoveable
{
    [Header("Moveable Settings")]

    public Vector3 StartPos;
    public Vector3 EndPos;

    public float MoveSpeed;
    public float Tolerance = 0.01f;
    public float WaitTime = 2f;


    //Private variables
    
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private bool isActive = false;
    private bool movingToEnd = true;


    private void Awake()
    {
        isActive = false;
    }

    private void Start()
    {
        transform.position = StartPos;
    }

    private void Update()
    {
        if (!isActive || isWaiting)
        {
            if (isWaiting)
            {
                waitTimer -= Time.deltaTime;
                if (waitTimer <= 0)
                {
                    movingToEnd = !movingToEnd;
                    waitTimer = 0;
                    isWaiting = false;
                }
            }
            return;
        }

        Move();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
            collision.transform.SetParent(transform);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
            collision.transform.SetParent(null);
    }
    void Move()
    {
        Vector3 target = movingToEnd ? EndPos : StartPos;
        transform.position = Vector3.MoveTowards(transform.position, target, MoveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) <= Tolerance && WaitTime > 0)
        {
            waitTimer = WaitTime;
            isWaiting = true;
        }
    }

    public void ActivateMovement()
    {
        isActive = true;
    }
}
