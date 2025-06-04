using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeBallTrigger : MonoBehaviour
{
    [SerializeField] private MainGame game;
    [SerializeField] private SkeletonAnimation freeBallSpine;
    [SerializeField] private string[] animName = new string[] { "close", "open", "idle close", "idle open" };
    //[SerializeField] private GameObject txtFreeBallInfo;
    [SerializeField] private float delayOnHole = 1f;
    [SerializeField] private float delayInfo = 1f;
    public float force = 10f;
    public List<Vector2> bottomDirections = new List<Vector2>
    {
        new Vector2(0, -1),            // Straight down (270 degrees)
        new Vector2(0.258f, -0.966f),  // Slightly to the right (285 degrees)
        new Vector2(0.5f, -0.866f),    // To the right (300 degrees)
        new Vector2(0.707f, -0.707f),  // Diagonal to the right (315 degrees)
        new Vector2(-0.258f, -0.966f), // Slightly to the left (255 degrees)
        new Vector2(-0.5f, -0.866f),   // To the left (240 degrees)
        new Vector2(-0.707f, -0.707f), // Diagonal to the left (225 degrees)
    };
    Ball ball;

    [SerializeField] SkeletonGraphic restOfBallAnim;
    [SerializeField] SkeletonAnimation trackAnim;
    bool isPlayingIdle = true;

    private void Start()
    {
        PlayIdleAnimation();
    }

    void PlayIdleAnimation()
    {
        StartCoroutine(PlayIdleAnimationIE());
    }
    IEnumerator PlayIdleAnimationIE()
    {
        while (isPlayingIdle)
        {
            SpineHelper.PlayAnimation(freeBallSpine, animName[0], false);
            yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(freeBallSpine, animName[0]));
            SpineHelper.PlayAnimation(freeBallSpine, animName[1], false);
            yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(freeBallSpine, animName[1]));
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball"))
        {
            ball = collision.GetComponent<Ball>();
            ball.Stop();
            ball.transform.position = transform.position;
            StartCoroutine(InitiateFreeBallIE());
        }
    }

    IEnumerator InitiateFreeBallIE()
    {
        isPlayingIdle = false;
        GetComponent<Collider2D>().enabled = false;
        SpineHelper.PlayAnimation(freeBallSpine, animName[0], false);
        yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(freeBallSpine, animName[0]));
        SpineHelper.PlayAnimation(freeBallSpine, animName[2], true);
        yield return new WaitForSeconds(delayOnHole);

        Vector2 randomDirection = bottomDirections[Random.Range(0, bottomDirections.Count)];

        game.ActivateFreeBall();
        SpineHelper.PlayAnimation(restOfBallAnim, "activate freeball", true);
        SpineHelper.PlayAnimation(trackAnim, "Unlocked", true);

        SpineHelper.PlayAnimation(freeBallSpine, animName[1], false);
        yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(freeBallSpine, animName[1]));
        SpineHelper.PlayAnimation(freeBallSpine, animName[3], true);
        ball.Restart(randomDirection, force, game.fireEffectOnBall);
        CloseFreeBall();

        yield return new WaitForSeconds(delayInfo);
        SpineHelper.PlayAnimation(restOfBallAnim, "idle", true);
        SpineHelper.PlayAnimation(trackAnim, "idle", true);
        isPlayingIdle = true;
        PlayIdleAnimation();
    }

    public void OpenFreeBall()
    {
        GetComponent<Collider2D>().enabled = true;
        StartCoroutine(OpenFreeBallIE());
    }
    IEnumerator OpenFreeBallIE()
    {
        SpineHelper.PlayAnimation(freeBallSpine, animName[1], false);
        yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(freeBallSpine, animName[1]));
        SpineHelper.PlayAnimation(freeBallSpine, animName[3], true);
    }

    public void CloseFreeBall()
    {
        GetComponent<Collider2D>().enabled = false;
        StartCoroutine(CLoseFreeBallIE());
    }
    IEnumerator CLoseFreeBallIE()
    {
        SpineHelper.PlayAnimation(freeBallSpine, animName[0], false);
        yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(freeBallSpine, animName[0]));
        SpineHelper.PlayAnimation(freeBallSpine, animName[2], true);
    }
}
