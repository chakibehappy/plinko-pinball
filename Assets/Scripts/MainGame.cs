using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Spine.Unity;
using UnityEngine.Networking;

[System.Serializable]
public class Blockers
{
    public List<PlinkoBlocker> blocker;
}

[System.Serializable]
public class BallIndicator
{
    public int index;
    public string animationToPlay;
    public string multiplier;
    public int colorIndex;
}

public class MainGame : MonoBehaviour
{
    [SerializeField] private MenuManager menu;
    [SerializeField] private APIManager API;
    [SerializeField] private FreeBallTrigger freeBallTrigger;

    public float minJumpForce = 30f;
    public float minJumpForceForFreeBall = 40f;
    public float maxJumpForce = 150f;
    public float forcingForce = 25f;
    float currentJumpPower = 0;

    [SerializeField] private Transform levelObjectParent;

    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform startBallPos;

    [SerializeField] private GameObject springerObj;

    [SerializeField] private Transform springerStartPos;
    [SerializeField] private Transform springerEndPos;


    [SerializeField] private GameObject launchButton;
    [SerializeField] private TextMeshProUGUI launchButtonLabel;

    GameObject inactiveBall;

    bool isHolding = false;
    public bool isBallLaunch = false;

    public List<Blockers> RowBlocker;
    Vector3 lastBlockerPos;

    public bool showBlockerObject = false;

    [SerializeField] private Color activeButtonColor;
    [SerializeField] private Color idleButtonColor;

    [SerializeField] private List<PowerUpBumperCollider> powerUps;
    int powerUpCount = 0;

    //int otherPowerUpCount = 0;
    //int otherPowerUpLimit = 2;

    //[SerializeField] private float powerUpMultiplier = 15;
    [SerializeField] private List<BashToyCollider> bashToy;
    [SerializeField] private List<DropTargetCollider> dropTarget;
    [SerializeField] private List<WallBumper> rocketBumperTarget;

    public float[] multiplierList = {
        0, 1.25f, 2f, 2.5f, 3f, 3f, 2.5f, 2, 1.25f, 0
    };

    List<GameResultEntry> multiplierResultEntry;

    List<GameResultEntry> currentMultiplierResultEntry;


    [SerializeField] private GameObject jumpBlocker;

    [SerializeField] private AudioSource rollingSfx;
    [SerializeField] private AudioClip rollingSfxClip;
    [SerializeField] private AudioSource springSfx;
    [SerializeField] private AudioClip springSfxClip;

    public float plinkoBlockForce = 3;

    bool disableLaunch = false;

    public int maxBallCount = 10;
    public int ballCount = 0;
    int ballOnTargetCount = 0;


    [SerializeField] private List<Transform> slotRows;


    public int jackpotCount = 0;
    List<JackpotMultiplier> jackpotMultipliers = new();

    [SerializeField] List<GameObject> lockedJackpots;
    [SerializeField] List<TextMeshPro> txtNumberJackpots;
    bool[] slotIsJackpot;

    [SerializeField] private GameObject resultScreen;
    [SerializeField] private TextMeshProUGUI txtWinningNumber;

    [SerializeField] private List<GameObject> indikatorObj;

    public float fireEffectOnBall = 0;
    List<Ball> activeBalls = new();

    [SerializeField] private List<SkeletonAnimation> ballDropFx;
    [SerializeField] private string[] skinName = new string[] { "win", "lose", "jackpot" };
    [SerializeField] private List<SpriteRenderer> slotBoxs;
    [SerializeField] private Sprite[] slotSkin;

    public bool isAutoPlay = false;
    public int autoPlayRoundCount = 1;

    [SerializeField] private GameObject indikatorImageGroup;
    [SerializeField] private GameObject[] indikatorNumberGroup;


    List<BallIndicator> ballIndicators;

    [SerializeField] private SkeletonGraphic resultScreenAnim;

    [SerializeField] private Transform[] winningIndicatorResultGroups;

    private List<Vector3> winningIndikatorMultiplierPos = new();
    private List<Vector3> winningIndikatorMultiplierScale = new();

    [SerializeField] private Color[] indikatorTextColor;

    [SerializeField] private AudioSource SFX;
    [SerializeField] private AudioClip[] SfxClip;

    List<HistoryData> historyList = new ();
    float totalBet = 0;
    BetData currentBetData;

    bool isFreeBallActive = false;

    public List<Transform> jackpotObjectTargets;
    public bool isForcingJackpot;
    public GameObject HolePreventionObject;

    public int rocketBumperHitCount = 0;
    public int rocketBumperTargetHitCount = 2;

    private void Start()
    {
        ResetIndikator();
        ResetBallDropFx();
        slotRows.ForEach(slot => slot.gameObject.SetActive(true));
        slotIsJackpot = new bool[slotRows.Count];

        disableLaunch = true;

        RowBlocker.ForEach(row => {
            row.blocker.ForEach(x => x.force = plinkoBlockForce);
        });
        ShowBlocker(showBlockerObject);
        AssignButtons();


#if UNITY_EDITOR
        StartCoroutine(GetLoginDataIE());
#else
        StartCoroutine(API.GetAPIFromConfig(GetUserData));
#endif

    }


    void GetUserData()
    {
        StartCoroutine(GetUserDataIE());
    }

    IEnumerator GetUserDataIE()
    {
        UnityWebRequest request = UnityWebRequest.Get(API.GetDataUserAPI());
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            API.Log("Error: " + request.error);
        }
        else
        {

            string responseJson = request.downloadHandler.text;
            API.Log(responseJson);
            UserDataResponse response = JsonUtility.FromJson<UserDataResponse>(responseJson);
            string modeltexts = responseJson.Split("models")[1].Replace(":", "")
                .Replace("{", "").Replace("}", "").Replace("\"", "").Replace("[[", "[").Replace("]]]", "]]");
            response.data.game.tutorial.models = API.ParseNestedList(modeltexts);
            PassingUserData(response);
        }
    }

    IEnumerator GetLoginDataIE()
    {
        yield return StartCoroutine(API.TriggerLoginIE(PassingUserData));
    }


    void PassingUserData(UserDataResponse response)
    {
        if(response != null)
        {
            menu.SetUserData(response);
            menu.ShowButtonPlayAndTutorial();
            StartCoroutine(API.GetHistoryDataIE(PassingHistoryData));

            //Audio.SaveAudioSetting(response.data.game.sounds);
            //menu.SetSoundSetting(response.data.game.sounds);
        }
        else
        {
            StartCoroutine(GetUserDataIE());
        }
    }

    void PassingHistoryData(HistoryResponse response)
    {
        historyList = response.data;
    }
    public List<HistoryData> GetHistoryList()
    {
        return historyList;
    }


    void ResetBallDropFx()
    {
        foreach (var item in ballDropFx)
        {
            item.gameObject.SetActive(false);
        }
    }

    public void UnlockPowerUpBumber(Ball ball)
    {
        powerUpCount++;
        if (powerUpCount >= powerUps.Count)
        {
            AddMultiplier();
        }
    }

    public void UnlockDropTargetAndBashToy()
    {
        //otherPowerUpCount++;
        //if (otherPowerUpCount >= otherPowerUpLimit)
        //{
        //    AddMultiplier();
        //}
        AddMultiplier();
    }

    public void UnlockRocketBumperJackpot()
    {
        rocketBumperHitCount++;
        if(rocketBumperHitCount >= rocketBumperTargetHitCount)
        {
            AddMultiplier();
        }
    }


    public void AddMultiplier()
    {
        jackpotCount++;

        if (jackpotCount >= jackpotMultipliers.Count)
            return;

        lockedJackpots[jackpotMultipliers[jackpotCount].box].SetActive(true);
        SkeletonAnimation skeleton = lockedJackpots[jackpotMultipliers[jackpotCount].box].transform.GetChild(0).GetComponent<SkeletonAnimation>();
        StartCoroutine(PlayActiveJackpotAnimation(skeleton));
        ActivateIndikatorObject(1 + jackpotCount);


        slotIsJackpot[jackpotMultipliers[jackpotCount].box] = true;
        slotRows[jackpotMultipliers[jackpotCount].box].GetComponent<TextMeshPro>().text = (jackpotMultipliers[jackpotCount].multiplier).ToString();

        if (jackpotCount > 0)
        {
            StartCoroutine(ShowJackpotAnimInfoIE());
        }
    }

    IEnumerator PlayActiveJackpotAnimation(SkeletonAnimation skeleton)
    {

        SpineHelper.PlayAnimation(skeleton, "start", false);
        yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(skeleton, "start"));
        SpineHelper.PlayAnimation(skeleton, "idle", true);
    }

    IEnumerator ShowJackpotAnimInfoIE()
    {
        SFX.PlayOneShot(SfxClip[0]);
        indikatorImageGroup.SetActive(true);
        indikatorNumberGroup[0].SetActive(false);
        indikatorNumberGroup[1].SetActive(false);
        yield return new WaitForSeconds(1.5f);
        indikatorImageGroup.SetActive(false);
        indikatorNumberGroup[maxBallCount > 5 ? 1 : 0].SetActive(true);
    }

    public void ActivateFreeBall()
    {
        isFreeBallActive = false;
        maxBallCount += 1;
        indikatorNumberGroup[0].SetActive(false);
        indikatorNumberGroup[1].SetActive(true);
        if(ballIndicators.Count > 0) { 
            for(int i = 0; i < ballIndicators.Count; i++)
            {
                ChangeIndikatorStatus(ballIndicators[i]);
            }
        }
        menu.ShowRestOfBall(maxBallCount - ballCount);
        if(ballCount >= 5)
        {
            InitBall();
        }
    }

    public void InitBall()
    {
        disableLaunch = false;
        isBallLaunch = false;
        GameObject ball = Instantiate(ballPrefab, startBallPos.position, Quaternion.identity);
        ball.transform.parent = springerObj.transform;
        inactiveBall = ball;
    }

    public void ShowButtonUI(bool isShow)
    {
        menu.ShowButtonUI(isShow);
    }

    public void BallPassJumpBlocker(Ball ball)
    {
        ballCount++;
        menu.ShowRestOfBall(maxBallCount - ballCount);
        jumpBlocker.SetActive(true);
        ShowButtonUI(false);
        if (ballCount < maxBallCount)
            InitBall();
    }

    public void ReInitBall(GameObject ball)
    {
        isBallLaunch = false;
        ball.transform.parent = springerObj.transform;
        inactiveBall = ball;
    }

    void AssignButtons()
    {
        EventTrigger eventTriggerLaunch = launchButton.AddComponent<EventTrigger>();
        EventTrigger.Entry entryLaunchDown = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryLaunchDown.callback.AddListener((data) => { OnLaunchDown(); });
        EventTrigger.Entry entryLaunchUp = new()
        {
            eventID = EventTriggerType.PointerUp
        };
        entryLaunchUp.callback.AddListener((data) => { OnLaunchUp(); });
        eventTriggerLaunch.triggers.Add(entryLaunchDown);
        eventTriggerLaunch.triggers.Add(entryLaunchUp);
    }

    void ShowBlocker(bool isShow)
    {
        RowBlocker.ForEach(row => {
            row.blocker.ForEach(x => x.GetComponent<SpriteRenderer>().color = isShow ? Color.red : new Color32(0, 0, 0, 0));
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !disableLaunch)
        {
            OnLaunchDown();
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            OnLaunchUp();
        }

        if (isHolding)
        {
            springerObj.transform.position = Vector2.MoveTowards(springerObj.transform.position, springerEndPos.position, Time.deltaTime);
            currentJumpPower = ((springerStartPos.position.y - springerObj.transform.position.y) / (springerStartPos.position.y - springerEndPos.position.y)) * maxJumpForce;
        }
    }

    void OnLaunchDown()
    {
        menu.ClosePopUpInfo();

        if (isAutoPlay)
        {
            isAutoPlay = false;
            menu.OpenAutoPlayUI(false);
            menu.ShowLauncHighlightIcon(ballCount < maxBallCount);
            return;
        }

        if (ballCount >= maxBallCount)
            return;

        if (!isBallLaunch)
        {
            menu.ShowLauncHighlightIcon(false);
            launchButton.GetComponent<Image>().color = activeButtonColor;
            launchButtonLabel.color = activeButtonColor;
            currentJumpPower = minJumpForce;
            isHolding = true;
        }
    }

    void OnLaunchUp()
    {
        if (isAutoPlay)
            return;

        LaunchingBall();
    }

    void LaunchingBall()
    {
        if (isHolding)
        {
            if (ballCount == 0)
            {
                indikatorImageGroup.SetActive(false);
                indikatorNumberGroup[maxBallCount > 5 ? 1 : 0].SetActive(true);
            }

            if (jackpotCount == 0)
            {
                ActivateIndikatorObject(1);
            }
            springSfx.PlayOneShot(springSfxClip);
            rollingSfx.PlayOneShot(rollingSfxClip);
            launchButton.GetComponent<Image>().color = idleButtonColor;
            launchButtonLabel.color = idleButtonColor;
            isHolding = false;
            isBallLaunch = true;
            if (inactiveBall == null)
            {
                return;
            }

            bool forceFreeBall = false;
            if (isFreeBallActive)
            {
                if (ballCount == 1)
                    forceFreeBall = true;
                else
                    forceFreeBall = Random.Range(0f, 1f) < 0.5f;
            }

            float minJump = forceFreeBall ? minJumpForceForFreeBall : minJumpForce;
            inactiveBall.transform.parent = levelObjectParent;
            if (currentJumpPower <= minJump)
                currentJumpPower = minJump;
            springerObj.transform.DOMoveY(springerStartPos.position.y, 10f / currentJumpPower).SetEase(Ease.Linear);
            Ball ball = inactiveBall.GetComponent<Ball>();
            activeBalls.Add(ball);

            ball.gameResultEntry = multiplierResultEntry[ballCount];
            ball.ballTarget = multiplierResultEntry[ballCount].box;

            int LayerBall = LayerMask.NameToLayer("Ball" + (ballCount + 1).ToString());
            ball.gameObject.layer = LayerBall;

            ball.ApplyJump(Vector2.up, currentJumpPower, fireEffectOnBall);
            currentJumpPower = minJumpForce;
            inactiveBall = null;

            ball.game = this;
            ball.FillJackpotTargets();
            
            // testing :
            // ForcingJackpot(true);
        }
    }

    public void ForcingJackpot(bool isForcing)
    {
        isForcingJackpot = isForcing;
        HolePreventionObject.SetActive(isForcing);

    }

    public void BallOnTarget(Ball ball)
    {
        activeBalls.Remove(ball);
        ballOnTargetCount++;
        SFX.PlayOneShot(SfxClip[1]);

        currentMultiplierResultEntry.Add(ball.gameResultEntry);
        float result = ball.gameResultEntry.multiplier;

        string animName;
        int colIndex;
        if (slotIsJackpot[ball.ballTarget])
        {
            animName = "jackpot";
            colIndex = 2;
        }
        else
        {
            animName = result <= 0 ? "lose" : "win";
            colIndex = result <= 0 ? 1 : 0;

        }
        string numberResult = result.ToString();
        string finalNumber = numberResult.Contains(".") ? result.ToString("F2") : numberResult;
        if (menu.playerCurrency.ToLower() == "idr")
        {
            finalNumber = finalNumber.Replace('.', ',');
        }
        // int slotFxSkin = result < 1 ? 1 : 0;
        int slotFxSkin = result <= 0 ? 1 : 0;
        int skin = slotIsJackpot[ball.ballTarget] ? 2 : slotFxSkin;
        StartCoroutine(PlaySlotFXAnimationIE(ballDropFx[ball.ballTarget], skin));
        ShowMultiplierText(ball.ballTarget, finalNumber, result);
        BallIndicator indicator = new BallIndicator
        {
            index = ballOnTargetCount,
            animationToPlay = animName,
            multiplier = finalNumber,
            colorIndex = colIndex
        };
        ChangeIndikatorStatus(indicator);
        ballIndicators.Add(indicator);

        if (ballOnTargetCount >= maxBallCount)
        {
            ShowResultScreen();
        }
    }

    IEnumerator PlaySlotFXAnimationIE(SkeletonAnimation skeleton, int skin)
    {
        skeleton.gameObject.SetActive(true);
        skeleton.skeleton.SetSkin(skinName[skin]);
        skeleton.skeleton.SetSlotsToSetupPose();
        SpineHelper.PlayAnimation(skeleton, "animation", true);
        yield return new WaitForSeconds(1f);
        skeleton.gameObject.SetActive(false);
    }

    public void IncrementFireBall()
    {
        foreach (var ball in activeBalls)
        {
            if (ball != null)
            {
                ball.IncrementFireBall();
            }
        }
    }

    void ShowResultScreen()
    {
        StartCoroutine(ShowResultScreenIE());
    }

    IEnumerator ShowResultScreenIE()
    {
        winningIndicatorResultGroups[0].gameObject.SetActive(false);
        winningIndicatorResultGroups[1].gameObject.SetActive(false);
        yield return new WaitForSeconds(2f);
        resultScreen.SetActive(true);
        txtWinningNumber.gameObject.SetActive(false);

        winningIndikatorMultiplierPos = new();
        winningIndikatorMultiplierScale = new();

        Transform parent = winningIndicatorResultGroups[ballIndicators.Count > 5 ? 1 : 0];
        parent.gameObject.SetActive(true);
        for (int i = 0; i < parent.childCount; i++)
        {
            parent.GetChild(i).gameObject.SetActive(false);
            winningIndikatorMultiplierPos.Add(parent.GetChild(i).GetChild(2).transform.position);
            winningIndikatorMultiplierScale.Add(parent.GetChild(i).GetChild(2).transform.localScale);
        }

        float totalWin = 0;

        SpineHelper.PlayAnimation(resultScreenAnim, "start", false);
        float delay = SpineHelper.GetAnimationDuration(resultScreenAnim, "start");

        SFX.PlayOneShot(SfxClip[2]);
        for (int i = 0; i < ballIndicators.Count; i++)
        {

            SkeletonGraphic skeletonGraphic = parent.GetChild(i).GetComponent<SkeletonGraphic>();
            skeletonGraphic.Skeleton.SetSkin(ballIndicators[i].animationToPlay);
            skeletonGraphic.Skeleton.SetToSetupPose();
            skeletonGraphic.gameObject.SetActive(true);
            skeletonGraphic.transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0.15f), 0.5f, 1, 1).SetEase(Ease.Linear);
            SpineHelper.PlayAnimation(skeletonGraphic, "idle", true);
            parent.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text = (i + 1).ToString();
            parent.GetChild(i).GetChild(1).gameObject.SetActive(true);
            string ballMultiplier = ballIndicators[i].multiplier;
            if (menu.playerCurrency.ToLower() == "idr")
            {
                ballMultiplier = ballIndicators[i].multiplier.Replace(".", ",");
            }
            parent.GetChild(i).GetChild(2).GetComponent<TextMeshProUGUI>().text = "x" + ballMultiplier;
            parent.GetChild(i).GetChild(2).GetComponent<TextMeshProUGUI>().color = indikatorTextColor[ballIndicators[i].colorIndex];
            parent.GetChild(i).GetChild(2).gameObject.SetActive(true);

            //yield return new WaitForSeconds(delay / ballIndicators.Count);
            yield return new WaitForSeconds(0.1f);
        }
        SpineHelper.PlayAnimation(resultScreenAnim, "idle", true);

        txtWinningNumber.transform.localPosition = new Vector3(txtWinningNumber.transform.localPosition.x, -150, 0);

        yield return new WaitForSeconds(0.5f);
        //float delayNumberAnim = 0.75f;
        //List<GameObject> multiplierObj = new List<GameObject>();
        for (int i = 0; i < ballIndicators.Count; i++)
        {
            totalWin += currentMultiplierResultEntry[i].amount;
            //parent.GetChild(i).GetChild(2).DOMove(txtWinningNumber.transform.position, delayNumberAnim).SetEase(Ease.Linear);
            //parent.GetChild(i).GetChild(2).DOScale(new Vector3(3.5f, 3.5f, 3.5f), delayNumberAnim).SetEase(Ease.Linear);
            //multiplierObj.Add(parent.GetChild(i).GetChild(2).gameObject);
        }
        //yield return new WaitForSeconds(delayNumberAnim);
        //for (int i = 0; i < multiplierObj.Count; i++)
        //{
        //    multiplierObj[i].SetActive(false);
        //}
        SFX.Stop();
        SFX.PlayOneShot(SfxClip[3]);
        txtWinningNumber.gameObject.SetActive(true);
        txtWinningNumber.transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0.15f), 0.75f, 1).SetEase(Ease.Linear);
        yield return StartCoroutine(AnimateNumberIE(0, totalWin, 0.75f));

        ballCount = 0;
        menu.ResetButtonPlayStatus();
        menu.ChangePlayerBallance(currentBetData.balance);
        menu.AddHistory(currentBetData.total_win, currentMultiplierResultEntry);

        yield return new WaitForSeconds(1f);
        SFX.Stop();

        resultScreen.SetActive(false);

        for (int i = 0; i < ballIndicators.Count; i++)
        {
            parent.GetChild(i).GetChild(2).position = winningIndikatorMultiplierPos[i];
            parent.GetChild(i).GetChild(2).localScale = winningIndikatorMultiplierScale[i];
        }


        if (isAutoPlay)
        {
            autoPlayRoundCount--;
            if (autoPlayRoundCount <= 0)
            {
                ShowButtonUI(true);
                menu.SetCanOpenHistoryAndExit(true);
            }
            else
            {
                menu.ShowRoundCount(autoPlayRoundCount);
                StartSession(5, true, autoPlayRoundCount, totalBet);
            }
        }
        else
        {
            ShowButtonUI(true);
        }
    }

    IEnumerator AnimateNumberIE(float startValue, float endValue, float duration)
    {
        DOTween.To(() => startValue, x => startValue = x, endValue, duration).OnUpdate(() =>
        {
            txtWinningNumber.text = menu.playerCurrency + " " + StringHelper.MoneyFormat(startValue, menu.playerCurrency);
            txtWinningNumber.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = menu.playerCurrency + " " + StringHelper.MoneyFormat(startValue, menu.playerCurrency);
            txtWinningNumber.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = menu.playerCurrency + " " + StringHelper.MoneyFormat(startValue, menu.playerCurrency);
        });
        yield return new WaitForSeconds(duration);
    }

    void ShowMultiplierText(int index, string text, float result)
    {
        txtNumberJackpots[index].transform.GetChild(0).gameObject.SetActive(result > 0);
        txtNumberJackpots[index].text = "x" + text;
        txtNumberJackpots[index].gameObject.SetActive(true);
        txtNumberJackpots[index].transform.DOPunchScale(new Vector3(1.15f, 1.15f, 1.15f), 1.5f, 1, 1).SetEase(Ease.Linear)
            .OnComplete(() => {
                txtNumberJackpots[index].gameObject.SetActive(false);
            });
    }


    public void OnBallEnterPlinkoArea(Vector2 ballPosition, Ball ball)
    {
        ball.isActive = false;
        int layers = -1 << LayerMask.NameToLayer("Ball1");
        ball.GetComponent<Collider2D>().excludeLayers = layers;
        ball.GetComponent<Rigidbody2D>().excludeLayers = layers;

        SetLastRowBlocker(ball.ballTarget);

        bool isRightDirection = true;

        if (lastBlockerPos.x < ballPosition.x)
        {
            isRightDirection = false;
        }

        float blockerWidth = 0.55f;

        for (int i = 0; i < RowBlocker.Count; i++)
        {
            RowBlocker[i].blocker.ForEach(blocker =>
            {
                if (i < RowBlocker.Count - 1)
                {
                    blocker.gameObject.SetActive(true);
                }
                blocker.force = 2;
            });
        }

        if (ball.ballTarget == multiplierList.Length - 1)
        {
            for (int i = 0; i < RowBlocker.Count - 1; i++)
            {
                int j = 0;
                RowBlocker[i].blocker.ForEach(blocker =>
                {
                    blocker.direction = "Right";
                    blocker.gameObject.SetActive(j != RowBlocker[i].blocker.Count - 1);
                    j++;
                });
            }
        }
        else if (ball.ballTarget == 0)
        {
            for (int i = 0; i < RowBlocker.Count - 1; i++)
            {
                int j = 0;
                RowBlocker[i].blocker.ForEach(blocker =>
                {
                    blocker.direction = "Left";
                    blocker.gameObject.SetActive(j != 0);
                    j++;
                });
            }
        }
        else
        {
            for (int i = 0; i < 2; i++)
            {
                RowBlocker[i].blocker.ForEach(blocker =>
                {
                    if (isRightDirection)
                    {
                        blocker.direction = blocker.transform.position.x < ballPosition.x ? "Right" : "Left";
                        blocker.gameObject.SetActive(blocker.transform.position.x < ballPosition.x);
                    }
                    else
                    {
                        blocker.direction = blocker.transform.position.x > ballPosition.x ? "Left" : "Right";
                        blocker.gameObject.SetActive(blocker.transform.position.x > ballPosition.x);
                    }
                });
            }

            for (int i = 1; i < RowBlocker.Count - 1; i++)
            {
                int j = 0;
                RowBlocker[i].blocker.ForEach(blocker =>
                {
                    if (blocker.transform.position.x >= lastBlockerPos.x - blockerWidth && blocker.transform.position.x <= lastBlockerPos.x + blockerWidth)
                        blocker.gameObject.SetActive(false);

                    if (blocker.transform.position.x >= lastBlockerPos.x)
                        blocker.direction = "Left";
                    else
                        blocker.direction = "Right";


                    if (!isRightDirection)
                    {
                        if (j == ball.ballTarget)
                        {
                            blocker.gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        int indexFromRight = RowBlocker[RowBlocker.Count - 1].blocker.Count - ball.ballTarget;
                        if (j == RowBlocker[i].blocker.Count - indexFromRight)
                        {
                            blocker.gameObject.SetActive(false);
                        }
                    }
                    j++;
                });
            }
        }

        // instantiate new blocker sets
        GameObject blockerParent = new GameObject("blockers");
        for (int i = 0; i < RowBlocker.Count; i++)
        {
            RowBlocker[i].blocker.ForEach(blocker =>
            {
                if (blocker.gameObject.activeInHierarchy)
                {
                    int layers = 1 << ball.gameObject.layer;
                    int inverse = ~(layers);
                    blocker.GetComponent<Collider2D>().excludeLayers = inverse;
                    GameObject tempBlock = Instantiate(blocker.gameObject, blockerParent.transform);
                    tempBlock.transform.position = blocker.transform.position;
                    tempBlock.transform.localScale = blocker.transform.parent.localScale;
                    blocker.gameObject.SetActive(false);
                }
            });
        }
        ball.blockerSets = blockerParent;
    }

    void SetLastRowBlocker(int boxTarget)
    {
        for (int i = 0; i < RowBlocker[RowBlocker.Count - 1].blocker.Count; i++)
        {
            PlinkoBlocker lastRowBlocker = RowBlocker[RowBlocker.Count - 1].blocker[i];
            lastRowBlocker.direction = i < boxTarget ? "Right" : "Left";
            lastRowBlocker.gameObject.SetActive(i != boxTarget);
            if (i == boxTarget)
            {
                lastBlockerPos = lastRowBlocker.transform.position;
            }
        }
    }

    void ResetIndikator()
    {
        indikatorImageGroup.SetActive(true);
        indikatorNumberGroup[0].SetActive(false);
        indikatorNumberGroup[1].SetActive(false);

        ballIndicators = new();

        for (int i = 0; i < indikatorNumberGroup.Length; i++)
        {
            for (int j = 0; j < indikatorNumberGroup[i].transform.childCount; j++)
            {
                indikatorNumberGroup[i].transform.GetChild(j).GetComponent<SkeletonAnimation>().skeleton.SetSkin("empty");
                indikatorNumberGroup[i].transform.GetChild(j).GetComponent<SkeletonAnimation>().skeleton.SetToSetupPose();
                indikatorNumberGroup[i].transform.GetChild(j).GetChild(0).GetComponent<TextMeshPro>().text = (j + 1).ToString();
                indikatorNumberGroup[i].transform.GetChild(j).GetChild(0).gameObject.SetActive(false);
                indikatorNumberGroup[i].transform.GetChild(j).GetChild(1).gameObject.SetActive(false);
            }
        }
    }

    void ChangeIndikatorStatus(BallIndicator indicator)
    {
        Transform parentTransform = indikatorNumberGroup[maxBallCount > 5 ? 1 : 0].transform;
        parentTransform.GetChild(indicator.index - 1).GetComponent<SkeletonAnimation>().skeleton.SetSkin(indicator.animationToPlay);
        parentTransform.GetChild(indicator.index - 1).GetComponent<SkeletonAnimation>().skeleton.SetToSetupPose();
        parentTransform.GetChild(indicator.index - 1).GetChild(0).GetComponent<TextMeshPro>().text = indicator.index.ToString();
        parentTransform.GetChild(indicator.index - 1).GetChild(0).gameObject.SetActive(true);
        parentTransform.GetChild(indicator.index - 1).GetChild(1).GetComponent<TextMeshPro>().text = "x" + indicator.multiplier;
        parentTransform.GetChild(indicator.index - 1).GetChild(1).GetComponent<TextMeshPro>().color = indikatorTextColor[indicator.colorIndex];
        parentTransform.GetChild(indicator.index - 1).GetChild(1).gameObject.SetActive(true);
    }

    public void StartSession(int selectedSession, bool autoPlay = false, int roundCount = 1, float totalAmmount = 0)
    {
        totalBet = totalAmmount;
        freeBallTrigger.CloseFreeBall();
        isFreeBallActive = false;
        ResetIndikator();
        isAutoPlay = autoPlay;
        autoPlayRoundCount = roundCount;
        activeBalls = new();
        ActivateIndikatorObject(0);
        fireEffectOnBall = 0;
        ballCount = 0;
        ballOnTargetCount = 0;
        maxBallCount = selectedSession;
        jackpotCount = 0;
        lockedJackpots.ForEach(j => j.SetActive(false));
        for (int i = 0; i < slotIsJackpot.Length; i++)
        {
            slotIsJackpot[i] = false;
        }
        multiplierResultEntry = new();
        currentMultiplierResultEntry = new();

        ForcingJackpot(false);

        menu.ChangeTempPlayerBallance((menu.GetCurrentBalance()-((decimal)totalBet)).ToString());
        StartCoroutine(SendBetRequestIE());
    }

    IEnumerator SendBetRequestIE()
    {
        yield return StartCoroutine(API.SendBetRequest(totalBet, ReadyToLaunch));
    }

    void ReadyToLaunch(BetResponse betResponse)
    {

        //string exampleJson = "{\r\n  \"status\": true,\r\n  \"message\": \"BET CONFIRMED\",\r\n  \"data\": {\r\n    \"round_id\": 1,\r\n    \"game_result\": {\r\n      \"result\": {\r\n        \"multipliers\": [\r\n          2.2,\r\n          1.1,\r\n          0.5,\r\n          0.2,\r\n          0,\r\n          0.15,\r\n          0.95,\r\n          1.2,\r\n          20\r\n        ],\r\n        \"jackpot_multiplier\": [\r\n          {\r\n            \"box\": 0,\r\n            \"multiplier\": 2.2\r\n          },\r\n          {\r\n            \"box\": 7,\r\n            \"multiplier\": 2.2\r\n          },\r\n          {\r\n            \"box\": 8,\r\n            \"multiplier\": 170\r\n          }\r\n        ],\r\n        \"game_results\": [\r\n          {\r\n            \"box\": 4,\r\n            \"multiplier\": 0,\r\n            \"amount\": 0\r\n          },\r\n          {\r\n            \"box\": 5,\r\n            \"multiplier\": 0.15,\r\n            \"amount\": 150\r\n          },\r\n          {\r\n            \"box\": 2,\r\n            \"multiplier\": 0.5,\r\n            \"amount\": 500\r\n          },\r\n          {\r\n            \"box\": 5,\r\n            \"multiplier\": 0.15,\r\n            \"amount\": 150\r\n          },\r\n          {\r\n            \"box\": 4,\r\n            \"multiplier\": 0,\r\n            \"amount\": 0\r\n          }\r\n,\r\n          {\r\n            \"box\": 4,\r\n            \"multiplier\": 0,\r\n            \"amount\": 0\r\n          }\r\n        ],\r\n        \"total_win\": 800,\r\n        \"total_amount\": 5000\r\n      }\r\n    },\r\n    \"total_win\": 800,\r\n    \"total_amount\": 5000,\r\n    \"balance\": 9951018106486.18\r\n  },\r\n  \"type\": \"bet_confirmed\"\r\n}";
        //BetResponse exampledata = JsonUtility.FromJson<BetResponse>(exampleJson);
        //BetData data = exampledata.data;

        if (betResponse.status == false)
        {
            menu.ChangePlayerBallance(menu.GetCurrentBalance().ToString());
            menu.ShowMessageInfo(betResponse.message);
            menu.ResetButtonPlayStatus();
            return;
        }

        BetData data = betResponse.data;

        menu.HideBetMenuButtons();

        currentBetData = data;
        if(data.game_result.result.game_results.Count > 5)
        {
            freeBallTrigger.OpenFreeBall();
            isFreeBallActive = true;
        }

        multiplierList = data.game_result.result.multipliers;

        jackpotMultipliers = data.game_result.result.jackpot_multiplier;

        for (int i = 0; i < data.game_result.result.game_results.Count; i++)
        {
            multiplierResultEntry.Add(data.game_result.result.game_results[i]);
        }

        txtNumberJackpots.ForEach(text => text.gameObject.SetActive(false));

        for (int i = 0; i < multiplierList.Length; i++)
        {
            //slotBoxs[i].sprite = slotSkin[multiplierList[i] < 1 ? 1 : 0];
            slotBoxs[i].sprite = slotSkin[multiplierList[i] <= 0 ? 1 : 0];
        }

        powerUpCount = 0;
        //otherPowerUpCount = 0;
        powerUps.ForEach(p => p.ResetPowerBar());
        bashToy.ForEach(b => b.ResetPowerBar());
        dropTarget.ForEach(d => d.ResetDropTarget());
        rocketBumperTarget.ForEach(r => r.ResetWallBumper());

        // set the default jackpot
        slotIsJackpot[jackpotMultipliers[0].box] = true;
        lockedJackpots[jackpotMultipliers[0].box].SetActive(true);

        for (int i = 0; i < slotRows.Count; i++)
        {
            if (slotIsJackpot[i])
            {
                string strjackpotMultiplier = jackpotMultipliers[0].multiplier.ToString();
                if (menu.playerCurrency.ToLower() == "idr")
                {
                    strjackpotMultiplier = strjackpotMultiplier.Replace(".", ",");
                }
                slotRows[i].GetComponent<TextMeshPro>().text = strjackpotMultiplier;
            }
            else
            {
                string strMultiplier = multiplierList[i].ToString();
                if (menu.playerCurrency.ToLower() == "idr")
                {
                    strMultiplier = strMultiplier.Replace(".", ",");
                }
                slotRows[i].GetComponent<TextMeshPro>().text = strMultiplier;
            }
        }
        InitBall();

        if (isAutoPlay)
        {
            StartCoroutine(StartAutoPlaySessionIE());
        }
    }

    IEnumerator StartAutoPlaySessionIE()
    {
        int totalBall = 0;
        while (totalBall < maxBallCount && isAutoPlay)
        {
            totalBall++;
            isHolding = true;
            float pressingTime = Random.Range(0.5f, 1.5f);
            yield return new WaitForSeconds(pressingTime);
            LaunchingBall();
        }
    }

    void ActivateIndikatorObject(int index)
    {
        indikatorObj.ForEach((obj) => obj.SetActive(false));
        indikatorObj[index].SetActive(true);
    }

    
}
