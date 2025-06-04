using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HolePrevention : MonoBehaviour
{
    public Transform leftBumperPos;
    public Transform rightBumperPos;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball"))
        {
            Ball ball = collision.GetComponent<Ball>();
            if(ball.transform.position.x < 0)
            {
                ball.ApplyJump(rightBumperPos);
            }
            else
            {
                ball.ApplyJump(leftBumperPos);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            Ball ball = collision.gameObject.GetComponent<Ball>();
            if (ball.transform.position.x < 0)
            {
                ball.ApplyJump(rightBumperPos);
            }
            else
            {
                ball.ApplyJump(leftBumperPos);
            }
        }
    }
}
