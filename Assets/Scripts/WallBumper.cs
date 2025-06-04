using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallBumper : MonoBehaviour
{
    [SerializeField] private MainGame game;
    [SerializeField] private CameraShake camShake;
    [SerializeField] private Color normalColor;
    [SerializeField] private Color bumpColor;

    public List<Vector2> rightDirections = new List<Vector2>
    {
        new Vector2(1, 0),           // 0 degrees
        new Vector2(0.966f, 0.259f), // 15 degrees
        new Vector2(0.866f, 0.5f),   // 30 degrees
        new Vector2(0.707f, 0.707f), // 45 degrees
        new Vector2(0.5f, 0.866f),   // 60 degrees
        new Vector2(0.258f, 0.966f), // 75 degrees
        new Vector2(0, 1),           // 90 degrees
        new Vector2(0.966f, -0.259f), // -15 degrees
        new Vector2(0.866f, -0.5f),   // -30 degrees
        new Vector2(0.707f, -0.707f), // -45 degrees
        new Vector2(0.5f, -0.866f),   // -60 degrees
        new Vector2(0.258f, -0.966f)  // -75 degrees
    };

    public List<Vector2> upDirections = new List<Vector2>
    {
        new Vector2(0, 1),           // Straight up (90 degrees)
        new Vector2(0.258f, 0.966f), // Slightly to the right (75 degrees)
        new Vector2(0.5f, 0.866f),   // To the right (60 degrees)
        new Vector2(0.707f, 0.707f), // Diagonal to the right (45 degrees)
        new Vector2(-0.258f, 0.966f), // Slightly to the left (105 degrees)
        new Vector2(-0.5f, 0.866f),   // To the left (120 degrees)
        new Vector2(-0.707f, 0.707f), // Diagonal to the left (135 degrees)
    };

    public List<Vector2> leftDirections = new List<Vector2>();

    public bool isLeftWall = true;
    public bool isGoingUp = false;
    public float force = 10f;


    [SerializeField] private AudioSource Sfx;
    [SerializeField] private AudioClip sfxClip;

    private IEnumerator spineCoroutine;
    [SerializeField] private SkeletonAnimation spine;
    [SerializeField] private List<string> animationName = new();

    public bool isJackpotTrigger;
    public int hitTargetCount = 1;
    int hitCount = 0;

    private void Start()
    {
        // Generate left directions by negating x-components of rightDirections
        if (!isGoingUp)
        {
            foreach (var dir in rightDirections)
            {
                leftDirections.Add(new Vector2(-dir.x, dir.y));
            }
        }
        if (isJackpotTrigger)
        {
            spine.skeleton.SetSkin("triangle off");
        }
        SpineHelper.PlayAnimation(spine, animationName[0], true);
    }

    public void ResetWallBumper()
    {
        if (isJackpotTrigger)
        {
            spine.skeleton.SetSkin("triangle off");
        }
        hitCount = 0;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ball")) 
        {
            hitCount++;
            Sfx.PlayOneShot(sfxClip);
            PlayHitAnimation();
            camShake.Shake();

            if (isJackpotTrigger && hitCount == hitTargetCount)
            {
                spine.skeleton.SetSkin("triangle on");
                game.UnlockRocketBumperJackpot();
            }

            //GetComponent<SpriteRenderer>().color = bumpColor;
            spine.skeleton.SetColor(bumpColor);
            Vector2 randomDirection = upDirections[Random.Range(0, upDirections.Count)];
            if (!isGoingUp)
            {
                if (isLeftWall)
                {
                    randomDirection = rightDirections[Random.Range(0, rightDirections.Count)];
                }
                else
                {
                    randomDirection = leftDirections[Random.Range(0, rightDirections.Count)];
                }
            }

            ///Debug.Log(randomDirection);
            Ball ball = collision.gameObject.GetComponent<Ball>();
            ball.PlayHitAnimation();
            ball.ApplyJump(randomDirection, force, game.fireEffectOnBall);
            StartCoroutine(SetBackColorIE());
        }
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
        if(spineCoroutine != null)
        {
            StopCoroutine(spineCoroutine);
            spineCoroutine = null;
        }
    }

    IEnumerator SetBackColorIE()
    {
        yield return new WaitForSeconds(0.1f);
        //GetComponent<SpriteRenderer>().color = normalColor;
        spine.skeleton.SetColor(normalColor);
    }
}
