using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using Spine.Unity;

public class BashToyCollider : MonoBehaviour
{
    [SerializeField] private MainGame game;
    public float multiplier;

    Vector3 originalScale;
    Vector3 punchScale;

    int hitCount = 0;
    public int maxHitCount = 1;

    [SerializeField] private AudioSource Sfx;
    [SerializeField] private AudioClip sfxClip;

    private IEnumerator spineCoroutine;
    [SerializeField] private SkeletonAnimation spine;
    [SerializeField] private List<string> animationName = new() { "idle", "hit"};

    void Start()
    {
        originalScale = transform.localScale;
        punchScale = originalScale * 1.1f;
        ResetPowerBar();
    }


    public void ResetPowerBar()
    {
        hitCount = 0;
        SpineHelper.PlayAnimation(spine, animationName[0], true);
        spine.skeleton.SetSkin(hitCount.ToString());
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            Ball ball = collision.gameObject.GetComponent<Ball>();
            ball.PlayHitAnimation();
            game.IncrementFireBall();

            PlayHitAnimation();
            game.fireEffectOnBall += 0.1f;
            if(game.fireEffectOnBall > 1)
            {
                game.fireEffectOnBall = 1;
            }
            
            Sfx.PlayOneShot(sfxClip);
            
            if (hitCount < maxHitCount)
            {
                transform.localScale = originalScale;
                transform.DOScale(punchScale, 0.1f).SetEase(Ease.Linear);

                //hitCount++;
                //spine.skeleton.SetSkin(hitCount.ToString());
                hitCount = maxHitCount;
                spine.skeleton.SetSkin("4");
                if (hitCount == maxHitCount)
                {
                    //game.AddMultiplier();
                    //Debug.Log("Bash Toy Active");
                    game.UnlockDropTargetAndBashToy();
                }
                StartCoroutine(SetScaleBackIE());
            }

            if (game.isForcingJackpot)
            {
                if(hitCount >= maxHitCount)
                {
                    ball.RemoveTarget(transform);
                }
                ball.ApplyJump(Vector2.zero, game.forcingForce, game.fireEffectOnBall, transform);
            }
        }
    }


    IEnumerator SetScaleBackIE()
    {
        yield return new WaitForSeconds(0.1f);
        transform.DOScale(originalScale, 0.1f).SetEase(Ease.Linear);
    }



    void PlayHitAnimation()
    {
        StopHitAnimation();
        spineCoroutine = PlayHitAnimationIE();
        StartCoroutine(spineCoroutine);
    }
    IEnumerator PlayHitAnimationIE()
    {
        SpineHelper.PlayAnimation(spine, animationName[1], false);
        yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(spine, animationName[1]));
        SpineHelper.PlayAnimation(spine, animationName[0], true);
    }
    void StopHitAnimation()
    {
        if (spineCoroutine != null)
        {
            StopCoroutine(spineCoroutine);
            spineCoroutine = null;
        }
    }

}
