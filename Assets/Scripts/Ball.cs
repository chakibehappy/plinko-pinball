using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public MainGame game;

    private Rigidbody2D rb;
    public bool isActive = false;
    public float speed = 10f; // Speed of the ball
    public int ballTarget;
    public GameResultEntry gameResultEntry;
    public bool isOnPlinko = false;
    public GameObject blockerSets;
    public SpriteRenderer fireBall;
    public float alphaIncrement = 0.1f;
    private float maxAlpha = 1f;
    public GameObject fireEffect;

    public SkeletonAnimation spineObj;
    IEnumerator animCoroutine;
    [SerializeField] private float ballVelocityDelay = 1.0f;
    private float noMovementThreshold = 0.01f;
    [SerializeField] private float autoPushForce = 10f;

    bool isExecuted = false;
    bool isRightDirection = true;
    bool checkBallSpeed = true;

    List<Transform> jackpotTargets;

    public void FillJackpotTargets()
    {
        jackpotTargets = game.jackpotObjectTargets;
    }

    public void RemoveTarget(Transform target)
    {
        jackpotTargets.Remove(target);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        fireEffect.SetActive(false);
        StartCoroutine(CheckBallVelocityIE());
    }

    public void Stop()
    {
        spineObj.gameObject.SetActive(false);
        fireBall.gameObject.SetActive(false);
        fireEffect.gameObject.SetActive(false);
        checkBallSpeed = false;
        StopCoroutine(CheckBallVelocityIE());
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
    }

    public void Restart(Vector2 direction, float force, float fireEffectValue = 0)
    {
        spineObj.gameObject.SetActive(true);
        fireBall.gameObject.SetActive(true);
        fireEffect.gameObject.SetActive(true);
        rb.isKinematic = false;
        ApplyJump(direction, force, fireEffectValue);
        checkBallSpeed = true;
        StartCoroutine(CheckBallVelocityIE());
    }

    IEnumerator CheckBallVelocityIE()
    {
        while (checkBallSpeed)
        {
            isRightDirection = rb.velocity.x > 0;
            isExecuted = false;
            if (isActive)
            {
                if (rb.velocity.magnitude < noMovementThreshold)
                {
                    //Debug.Log("Ball is STOP! or enter free ball zone!");

                    if (!isExecuted)
                    {
                        isExecuted = true;
                        if (!isRightDirection)
                        {
                            ApplyJump(Vector2.left, autoPushForce);
                        }
                        else
                        {
                            ApplyJump(Vector2.right, autoPushForce);
                        }
                    }
                }
                //Debug.Log(rb.velocity);
            }
            yield return new WaitForSeconds(ballVelocityDelay);
        }
    }

    public void ApplyJump(Transform target)
    {
        Vector2 direction = GetVector2FromTarget(transform, target);
        rb.AddForce(direction * game.forcingForce);
    }

    public void ApplyJump(Vector2 direction, float force, float fireEffectValue = 0, Transform currentTarget = null)
    {
        if(game != null)
        {
            if (game.isForcingJackpot && !isOnPlinko)
            {
                force = game.forcingForce;
                if(game.jackpotCount < 2)
                {
                    if(jackpotTargets.Count > 0)
                    {

                        Transform target = jackpotTargets[Random.Range(0, jackpotTargets.Count)];
                        // make sure its not targetting the current target
                        if (currentTarget != null)
                        {
                            //Debug.Log(currentTarget.name);
                            while (target.name == currentTarget.name && jackpotTargets.Count > 1)
                            {
                                target = jackpotTargets[Random.Range(0, jackpotTargets.Count)];
                            }
                        }

                        Debug.Log("Target Count : " + jackpotTargets.Count);
                        Debug.Log("Go to " + target.name);
                        direction = GetVector2FromTarget(transform, target);
                    }
                }
                else
                {
                    Debug.Log("All jackpot is ON");
                    game.ForcingJackpot(false);
                }
            }
        }
        rb.AddForce(direction * force);

        if(fireEffectValue > 0)
        {
            Color color = fireBall.color;
            color.a = fireEffectValue;
            fireBall.color = color;
            if (fireEffectValue >= 1)
            {
                fireEffect.SetActive(true);
            }
        }
    }


    Vector2 GetVector2FromTarget(Transform from, Transform to)
    {
        return (to.position - from.position).normalized;
    }

    public void IncrementFireBall()
    {
        Color color = fireBall.color;
        color.a = Mathf.Min(color.a + alphaIncrement, maxAlpha); 
        if (color.a >= 1)
        {
            fireEffect.SetActive(true);
        }
        fireBall.color = color;
    }

    public void PlayHitAnimation()
    {
        if(animCoroutine != null)
        {
            StopCoroutine(animCoroutine);
            animCoroutine = null;
        }
        animCoroutine = PlayHitAnimationIE();
        StartCoroutine(animCoroutine);
    }
    IEnumerator PlayHitAnimationIE()
    {
        SpineHelper.PlayAnimation(spineObj, "hit", false);
        yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(spineObj, "hit"));
        SpineHelper.PlayAnimation(spineObj, "idle", true);
    }
}
