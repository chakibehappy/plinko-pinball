using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropTargetItemCollider : MonoBehaviour
{
    [SerializeField] private MainGame game;
    [SerializeField] private DropTargetCollider dropTargetCollider;
    [SerializeField] private Sprite[] barSprite;
    private bool isActive = true;

    private IEnumerator spineCoroutineOff, spineCoroutineOn;
    [SerializeField] private SkeletonAnimation spine;
    [SerializeField] private List<string> animationName = new() { "idle off", "hit when off", "idle on", "hit when on" };

    public void ResetDropTargetItem()
    {
        isActive = true;
        GetComponent<SpriteRenderer>().sprite = barSprite[0];
        SpineHelper.PlayAnimation(spine, animationName[0], true);
    }

    public void TargetOnHit()
    {
        PlayHitAnimationWhenOff();
        isActive = false;
        GetComponent<SpriteRenderer>().sprite = barSprite[1];
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball"))
        {
            Ball ball = collision.gameObject.GetComponent<Ball>();
            ball.PlayHitAnimation();;
            game.IncrementFireBall();
            game.fireEffectOnBall += 0.1f;
            if (game.fireEffectOnBall > 1)
            {
                game.fireEffectOnBall = 1;
            }
            if (isActive)
            {
                TargetOnHit();
                dropTargetCollider.DropTargetOnHit();
            }
            else
            {
                PlayHitAnimationWhenOn();
            }

            if (game.isForcingJackpot)
            {
                ball.RemoveTarget(transform);
                ball.ApplyJump(Vector2.zero, game.forcingForce, game.fireEffectOnBall, transform);
            }
        }
    }


    void PlayHitAnimationWhenOff()
    {
        StopHitAnimationWhenOff();
        spineCoroutineOff = PlayHitAnimationWhenOffIE();
        StartCoroutine(spineCoroutineOff);
    }
    IEnumerator PlayHitAnimationWhenOffIE()
    {
        SpineHelper.PlayAnimation(spine, animationName[1], false);
        yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(spine, animationName[1]));
        SpineHelper.PlayAnimation(spine, animationName[2], true);
    }
    void StopHitAnimationWhenOff()
    {
        if (spineCoroutineOff != null)
        {
            StopCoroutine(spineCoroutineOff);
            spineCoroutineOff = null;
        }
    }

    void PlayHitAnimationWhenOn()
    {
        StopHitAnimationWhenOn();
        spineCoroutineOn = PlayHitAnimationWhenOnIE();
        StartCoroutine(spineCoroutineOn);
    }
    IEnumerator PlayHitAnimationWhenOnIE()
    {
        SpineHelper.PlayAnimation(spine, animationName[3], false);
        yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(spine, animationName[3]));
        SpineHelper.PlayAnimation(spine, animationName[2], true);
    }
    void StopHitAnimationWhenOn()
    {
        if (spineCoroutineOn != null)
        {
            StopCoroutine(spineCoroutineOn);
            spineCoroutineOn = null;
        }
    }
}
