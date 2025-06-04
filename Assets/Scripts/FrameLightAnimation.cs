using DG.Tweening;
using System.Collections;
using UnityEngine;

public class FrameLightAnimation : MonoBehaviour
{
    private SpriteRenderer image;
    public float blinkingDelay = 1f;
    public float brightDelay = 1.5f;

    void Start()
    {
        image = GetComponent<SpriteRenderer>();
        StartCoroutine(PlayLightAnimationIE());
    }

    IEnumerator PlayLightAnimationIE()
    {
        while (true)
        {
            image.DOFade(0, blinkingDelay).SetEase(Ease.Linear);
            yield return new WaitForSeconds(blinkingDelay);
            image.DOFade(1, blinkingDelay).SetEase(Ease.Linear);
            yield return new WaitForSeconds(blinkingDelay);
            yield return new WaitForSeconds(brightDelay);
        }
    }
}
