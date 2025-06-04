using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlinkoBlocker : MonoBehaviour
{
    public bool isExecuted = false;
    public string direction = "Left";
    public float force = 10;


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            if (!isExecuted)
            {
                isExecuted = true;
                Ball ball = collision.gameObject.GetComponent<Ball>();
                if (direction == "Left")
                {
                    ball.ApplyJump(Vector2.left, force);
                }
                else
                {
                    ball.ApplyJump(Vector2.right, force);
                }
                StartCoroutine(ReactivatePushIE());
            }
        }
    }

    IEnumerator ReactivatePushIE()
    {
        yield return new WaitForSeconds(0.1f);
        isExecuted = false;
    }
}
