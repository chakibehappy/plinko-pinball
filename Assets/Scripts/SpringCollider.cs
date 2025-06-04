using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringCollider : MonoBehaviour
{
    [SerializeField] MainGame game;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Ball"))
        {
            Ball ball = collision.gameObject.GetComponent<Ball>();
            ball.isActive = false;
            game.ReInitBall(collision.gameObject);
        }
    }
}
