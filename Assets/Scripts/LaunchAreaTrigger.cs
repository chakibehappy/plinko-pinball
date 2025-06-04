using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchAreaTrigger : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball"))
        {
            Ball ball = collision.GetComponent<Ball>();
            int layers = -1 << LayerMask.NameToLayer("Ball1");
            ball.GetComponent<Collider2D>().excludeLayers = layers;
            ball.GetComponent<Rigidbody2D>().excludeLayers = layers;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball"))
        {
            Ball ball = collision.GetComponent<Ball>();
            int layers = 0;
            ball.GetComponent<Collider2D>().excludeLayers = layers;
            ball.GetComponent<Rigidbody2D>().excludeLayers = layers;
        }
    }
}
