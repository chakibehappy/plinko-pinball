using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlinkoAreaTrigger : MonoBehaviour
{
    [SerializeField] private MainGame game;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball"))
        {
            Ball ball = collision.gameObject.GetComponent<Ball>();
            if (!ball.isOnPlinko)
            {
                ball.isOnPlinko = true;
                Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                collision.gameObject.GetComponent<TrailRenderer>().enabled = false;
                game.OnBallEnterPlinkoArea(collision.transform.position, ball);
            }
        }
    }
}
