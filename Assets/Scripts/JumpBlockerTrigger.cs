using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpBlockerTrigger : MonoBehaviour
{
    [SerializeField] private MainGame game;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball"))
        {
            Ball ball = collision.GetComponent<Ball>();
            if (!ball.isActive)
            {
                ball.isActive = true;
                game.BallPassJumpBlocker(ball);
            }
        }
    }
}
