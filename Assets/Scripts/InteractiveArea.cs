using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveArea : MonoBehaviour
{
    public Color idleColor;
    public Color activeColor;
    public float delay = 1f;

    void Start()
    {
        GetComponent<SpriteRenderer>().color = idleColor;   
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball"))
        {
            StartCoroutine(BackToIdleColorIE());
        }
    }

    IEnumerator BackToIdleColorIE()
    {
        GetComponent<SpriteRenderer>().color = activeColor;
        yield return new WaitForSeconds(delay);
        GetComponent<SpriteRenderer>().color = idleColor;
    }
}
