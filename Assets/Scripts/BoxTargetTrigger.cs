using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxTargetTrigger : MonoBehaviour
{
    [SerializeField] private MainGame game;

    [SerializeField] private AudioSource Sfx;
    [SerializeField] private AudioClip sfxClip;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball"))
        {
            Ball ball = collision.GetComponent<Ball>();
            Destroy(ball.blockerSets);
            Sfx.PlayOneShot(sfxClip);
            //game.InitBall();
            game.BallOnTarget(ball);
            StartCoroutine(DestroyBallIE(collision.gameObject));
        }
    }

    IEnumerator DestroyBallIE(GameObject ball)
    {
        yield return new WaitForSeconds(1f);
        Destroy(ball);
    }
}
