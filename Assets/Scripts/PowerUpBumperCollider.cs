using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Spine.Unity;

public class PowerUpBumperCollider : MonoBehaviour
{
    public Sprite ballSprite;
    [SerializeField] private MainGame game;
    //public SpriteRenderer centerCircle;
    //public List<GameObject> powerBarObj;
    public Color activeColor;
    public Transform circleObject;
    public float multiplier;
    Vector3 originalScale;
    Vector3 pumchScale;

    int hitCount = 0;
    public int maxHitCount = 1;

    [SerializeField] private AudioSource Sfx;
    [SerializeField] private AudioClip sfxClip;

    private IEnumerator spineCoroutine;
    [SerializeField] private SkeletonAnimation spine;
    [SerializeField] private List<string> animationName = new();

    void Start()
    {
        originalScale = circleObject.localScale;
        pumchScale = originalScale * 1.1f;
        ResetPowerBar();
    }

    public void ResetPowerBar()
    {
        hitCount = 0;
        //centerCircle.color = Color.white;
        //powerBarObj.ForEach(x => x.SetActive(true));
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
            if (game.fireEffectOnBall > 1)
            {
                game.fireEffectOnBall = 1;
            }

            //if (!Sfx.isPlaying)
            //{
                Sfx.PlayOneShot(sfxClip);
            //}

            if (hitCount < maxHitCount)
            {
                circleObject.transform.localScale = originalScale;
                circleObject.DOScale(pumchScale, 0.1f).SetEase(Ease.Linear);
                //powerBarObj[hitCount].SetActive(false);
                hitCount++;
                //spine.skeleton.SetSkin(hitCount.ToString());
                spine.skeleton.SetSkin("4");
                //hitCount = maxHitCount;
                if (hitCount == maxHitCount)
                {
                    //powerBarObj.ForEach(x => x.SetActive(false));
                    //Debug.Log("Power Bumper Active");
                    //centerCircle.color = activeColor;
                    //game.AddMultiplier(multiplier);
                    game.UnlockPowerUpBumber(collision.gameObject.GetComponent<Ball>());
                }
                StartCoroutine(SetScaleBackIE());
            }


            if (game.isForcingJackpot)
            {
                if (hitCount >= maxHitCount)
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
        circleObject.DOScale(originalScale, 0.1f).SetEase(Ease.Linear);
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
