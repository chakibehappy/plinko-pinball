using DG.Tweening;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private APIManager API;
    [SerializeField] private MainGame game;
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private GameObject TitleScreen;
    [SerializeField] private GameObject buttonPlay;
    [SerializeField] private GameObject buttonTutorial;
    [SerializeField] private GameObject fadeScreen;

    [SerializeField] private Color[] buttonColor;
    [SerializeField] private Sprite[] chipButtonSkin;

    int selectedSession = 0;

    [SerializeField] private long[] chipValues = new long[] {
        1000,
        2000,
        5000,
        10000,
        20000,
        50000,
        100000,
        500000,
        1000000
    };
    [SerializeField] private List<GameObject> chipButtons;
    //[SerializeField] private List<TextMeshProUGUI> chipButtonsText;
    int selectedChipValueIndex = 0;

    [SerializeField] private GameObject decreaseBetButton, increaseBetButton;
    [SerializeField] private TextMeshProUGUI txtBet;
    [SerializeField] private TextMeshProUGUI txtTotalBet;
    [SerializeField] private TextMeshPro txtTotalBetMiddle;
    [SerializeField] private TextMeshPro txtTotalBalanceMiddle;
    private float currentBet = 0;
    
    [SerializeField] private TextMeshProUGUI txtTotalBalance;
    [SerializeField] private decimal totalPlayerBalance = 1000000000;
    
    [SerializeField] private GameObject autoPlayButton;
    [SerializeField] private GameObject startRoundButton;

    [SerializeField] private GameObject btnUIObj;
    [SerializeField] private Transform[] btnUIPos;

    [SerializeField] private TextMeshProUGUI txtBallCount;
    [SerializeField] private TextMeshProUGUI txtBallLeft;

    [SerializeField] private GameObject popUpBetButton;
    [SerializeField] private Transform popUpBetInfo;
    [SerializeField] private Transform[] popUpBetInfoPos;
    [SerializeField] private TextMeshProUGUI txtTotalBalanceInfo, txtBetInfo;
    [SerializeField] private GameObject popUpBetCloseButton;

    bool showPopUpBet = false;

    [Header("Auto Play UI")]
    [SerializeField] private GameObject autoPlayUI;
    [SerializeField] private GameObject closeAutoPlayButton;
    [SerializeField] private GameObject confirmAutoPlayButton;
    [SerializeField] private List<GameObject> autoPlayRoundNumberButton;
    [SerializeField] private List<int> roundNumbers = new List<int>() { 10, 25, 50, 100, 999 };
    bool isAutoPlay;
    int selectedAutoPlayRoundsIndex = 0;

    [SerializeField] private Image launchButtonImage;
    [SerializeField] private Sprite[] launchButtonSprite;
    [SerializeField] private TextMeshProUGUI txtRoundCount;
    [SerializeField] private TextMeshProUGUI txtLaunchBall;

    [SerializeField] private Transform launchHighlightIcon;
    [SerializeField] private Transform[] launchHighlightIconPos;
    [SerializeField] private float launchHighlightDelay = 1f;

    bool showMenu = false;
    [SerializeField] private GameObject menuButton;
    [SerializeField] private Transform[] menuButtonPos;
    [SerializeField] private Transform menuButtonGroup;
    [SerializeField] private GameObject[] subMenuButtons;

    [Header("Audio Setting UI")]
    [SerializeField] private GameObject AudioMenuUI;
    [SerializeField] private GameObject bgmToogle, sfxToogle;

    [Header("Tutorial UI")]
    [SerializeField] private GameObject TutorialUI;
    [SerializeField] private GameObject tutorialNextButton;
    [SerializeField] private GameObject tutorialPrevButton;
    [SerializeField] private GameObject[] tutorialContents;
    [SerializeField] private GameObject[] tutorialPageIndikator;
    [SerializeField] private Sprite[] tutorialIndikatorSkin;
    int currentTutorialPage = 0;
    [SerializeField] private TextMeshProUGUI txtMinBet, txtMaxBet, txtMultiplier1, txtMultiplier2, txtRTP;
    [SerializeField] private GameObject[] multiplierSets;

    [Header("Tutorial UI Webview")]
    [SerializeField] private TutorialDisplay tutorialDisplay;

    [Header("History UI")]
    [SerializeField] private GameObject HistoryUI;
    [SerializeField] private List<Transform> historyRows;
    private List<HistoryRecord> historyRecords = new();
    [SerializeField] private Color32[] historyRowColor;

    [SerializeField] private GameObject historyDetailTop;
    [SerializeField] private GameObject historyDetailRow;
    [SerializeField] private Transform historyContent;
    private List<GameObject>[] detailedHistoryObjects;

    [Header("SFX")]
    [SerializeField] private AudioSource SFX;
    [SerializeField] private AudioClip[] sfxClips;

    [SerializeField] private SkeletonGraphic titleSpine;

    bool canPressPlay = true;

    [SerializeField] private GameObject messageInfoUI;
    [SerializeField] private TextMeshProUGUI txtMessageInfo;
    [SerializeField] private GameObject txt_no_connection;

    float minBet, maxBet;
    bool canOpenHistoryAndExit = true;

    public string playerCurrency = "";
    List<TextMeshProUGUI> tutorialTextToChange;

    [Serializable]
    public class HistoryRecord
    {
        public string created_date;
        public float bet;
        public float result;
        public List<GameResultEntry> game_results;
    }

    public void SetUserData(UserDataResponse response)
    {
        tutorialTextToChange = new();

        chipValues = response.data.game.chip_base;
        minBet = response.data.game.limit_bet.minimal;
        maxBet = response.data.game.limit_bet.maximal;
        playerCurrency = response.data.player.player_currency;
        ChangePlayerBallance(response.data.player.player_balance);

        string playerLanguage = response.data.player.player_language.ToLower();
        LanguageManager.Instance.SetLanguage(playerLanguage);

        string bulletPoint = "\u2022 ";
        string lineBreak = "\r\n";

        txtMinBet.text = "Min Bet: " + GetNumberLabel(minBet);
        txtMaxBet.text = "Max Bet: " + GetNumberLabel(maxBet);

        List<List<string>> model = response.data.game.tutorial.models;

        for (int i = 0; i < multiplierSets.Length; i++)
        {
            for (int j = 0; j < multiplierSets[i].transform.childCount; j++)
            {
                multiplierSets[i].transform.GetChild(j).GetComponent<TextMeshProUGUI>().text = model[i][j].Split(",")[0];
                tutorialTextToChange.Add(multiplierSets[i].transform.GetChild(j).GetComponent<TextMeshProUGUI>());
            }
        }

        string multiplierText1 = 
            bulletPoint + " Set 1: " + model[0][7].Split(",")[0] + "x upgrades to " + model[0][7].Split(",")[1] + "x " + 
            lineBreak +
            bulletPoint + " Set 2: " + model[1][7].Split(",")[0] + "x upgrades to " + model[2][7].Split(",")[1] + "x " +
            lineBreak +
            bulletPoint + " Set 3: " + model[2][7].Split(",")[0] + "x upgrades to " + model[2][7].Split(",")[1] + "x";
        
        txtMultiplier1.text = multiplierText1;

        string multiplierText2 =
            bulletPoint + " Set 1: " + model[0][8].Split(",")[0] + "x upgrades to " + model[0][8].Split(",")[1] + "x " +
            lineBreak +
            bulletPoint + " Set 2: " + model[1][8].Split(",")[0] + "x upgrades to " + model[1][8].Split(",")[1] + "x " +
            lineBreak +
            bulletPoint + " Set 3: " + model[2][8].Split(",")[0] + "x upgrades to " + model[2][8].Split(",")[1] + "x " +
            lineBreak;
        txtMultiplier2.text = multiplierText2;

        txtRTP.text = response.data.game.tutorial.rtp;

        tutorialTextToChange.Add(txtMultiplier1);
        tutorialTextToChange.Add(txtMultiplier2);
        tutorialTextToChange.Add(txtRTP);

        if (playerCurrency.ToLower() == "idr")
        {
            tutorialTextToChange.ForEach(t => t.text = t.text.Replace(".", ","));
        }
        else
        {
            tutorialTextToChange.ForEach(t => t.text = t.text.Replace(",", "."));
        }

        ChangeBet(false, false);

        selectedSession = 5;
        //OnSessionSelected(selectedSession);
        ShowRestOfBall(selectedSession);

    }


    public void ShowButtonPlayAndTutorial(bool isShow = true)
    {
        buttonPlay.SetActive(isShow);
        buttonTutorial.SetActive(isShow);
    }

    private void Start()
    {
        TitleScreen.SetActive(true);
        StartCoroutine(PlayAlienAnimationIE());
        AssignMenuButtons();
        ShowButtonPlayAndTutorial(false);

        SetBetButtons();
        SetPopUpBetButton();
        SetMenuButton();
        SetupTutorialWindow();
        SetTutorialButtons();

        btnUIObj.transform.position = btnUIPos[0].position;
        txtBet.text = playerCurrency + " " + StringHelper.MoneyFormat(currentBet, playerCurrency);
        txtTotalBet.text = LanguageManager.Instance.GetLabel("txt_total") + ": " + playerCurrency + " " + StringHelper.MoneyFormat(currentBet * 5, playerCurrency);
        txtBetInfo.text = playerCurrency + " " + StringHelper.MoneyFormat(currentBet, playerCurrency);
        txtTotalBetMiddle.text = playerCurrency + " " + StringHelper.MoneyFormat(currentBet * 5, playerCurrency);

        SetAutoPlayButton();
        SetStartLaunchButton();
        SetAutoPlayRoundsButton();
        OnAutoPlayRoundSelected(0);
        messageInfoUI.SetActive(false);
        OpenAutoPlayUI(false);

        StartCoroutine(AnimateLaunchHighlightIconIE());

        ClosePopUpInfo();
        SetAudioSetting();
        TutorialUI.SetActive(false);
        AudioMenuUI.SetActive(false);
        HistoryUI.SetActive(false);
    }

    public void AddHistory(float result, List<GameResultEntry> entries)
    {
        string formattedDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        HistoryRecord record = new HistoryRecord{ 
            created_date = formattedDateTime, 
            bet = currentBet * 5, 
            result = result,
            game_results = entries
        };
        historyRecords.Add(record);
        historyRecords.Reverse();
        if(historyRecords.Count > 10)
        {
            historyRecords.RemoveAt(10);
        }
    }

    IEnumerator PlayAlienAnimationIE()
    {
        SpineHelper.PlayAnimation(titleSpine, "start", false);
        yield return new WaitForSeconds(SpineHelper.GetAnimationDuration(titleSpine, "start"));
        SpineHelper.PlayAnimation(titleSpine, "idle", true);
    }

    void SetMenuButton()
    {
        EventTrigger eventTriggerMenu = menuButton.AddComponent<EventTrigger>();
        EventTrigger.Entry entryMenu = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryMenu.callback.AddListener((data) => {
            menuButton.transform.localScale = Vector3.one;
            menuButton.transform.DOPunchScale(new Vector3(0.15f, 0.15f, 0.15f), 0.15f, 1, 1).SetEase(Ease.Linear);
            showMenu = !showMenu;
            ShowMenuButtons();
        });
        eventTriggerMenu.triggers.Add(entryMenu);

        for (int i = 0; i < subMenuButtons.Length; i++)
        {
            int index = i;
            EventTrigger eventTrigger = subMenuButtons[index].AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new()
            {
                eventID = EventTriggerType.PointerDown
            };
            entry.callback.AddListener((data) => { OnSubMenuButton(index); });
            eventTrigger.triggers.Add(entry);
        }

        EventTrigger eventTriggerBgm = bgmToogle.AddComponent<EventTrigger>();
        EventTrigger.Entry entryBgm = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryBgm.callback.AddListener((data) => {
            audioManager.ToogleBgm();
            SetAudioSetting();
        });
        eventTriggerBgm.triggers.Add(entryBgm);


        EventTrigger eventTriggerSfx = sfxToogle.AddComponent<EventTrigger>();
        EventTrigger.Entry entrySfx = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entrySfx.callback.AddListener((data) => {
            audioManager.ToogleSfx();
            SetAudioSetting();
        });
        eventTriggerSfx.triggers.Add(entrySfx);
    }

    void SetAudioSetting()
    {
        bgmToogle.transform.GetChild(0).gameObject.SetActive(!audioManager.enableBGM);
        bgmToogle.transform.GetChild(1).gameObject.SetActive(audioManager.enableBGM);
        sfxToogle.transform.GetChild(0).gameObject.SetActive(!audioManager.enableSFX);
        sfxToogle.transform.GetChild(1).gameObject.SetActive(audioManager.enableSFX);
    }

    public void ShowMenuButtons()
    {
        menuButtonGroup.DOMoveY(menuButtonPos[showMenu ? 1 : 0].position.y, 0.15f).SetEase(Ease.Linear);
    }

    void AssignHistoryDetailButton(GameObject obj, List<GameResultEntry> data, int index)
    {
        if (obj.GetComponent<EventTrigger>() != null)
        {
            Destroy(obj.GetComponent<EventTrigger>());
        }
        EventTrigger eventTrigger = obj.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entry.callback.AddListener((x) => {
            ShowHistoryDetail(obj, data, index);
        });

        eventTrigger.triggers.Add(entry);
    }

    void ShowHistoryDetail(GameObject obj, List<GameResultEntry> data, int historyIndex)
    {
        if (obj.transform.localScale.y < 0)
        {

            List<GameObject> list = new List<GameObject>();
            // add detail
            obj.transform.localScale = new Vector3(obj.transform.localScale.x, obj.transform.localScale.y * -1, obj.transform.localScale.z);
            GameObject top = Instantiate(historyDetailTop, historyContent);
            list.Add(top);

            int index = historyRows[historyIndex].GetSiblingIndex();
            top.transform.SetSiblingIndex(index + 1);
            top.name = "History Detail " + historyIndex;
            for (int i = 0; i < data.Count; i++)
            {
                GameObject row = Instantiate(historyDetailRow, historyContent);
                row.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = (i+1).ToString();
                string formattedMultiplier = data[i].multiplier.ToString();
                if (playerCurrency.ToLower() == "idr")
                {
                    formattedMultiplier = formattedMultiplier.Replace('.', ',');
                }
                row.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = formattedMultiplier + "x";
                row.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = playerCurrency + " " + StringHelper.MoneyFormat(data[i].amount, playerCurrency); //data[i].amount.ToString("G30");
                row.transform.SetSiblingIndex(index + (2 + i));
                row.name = "History Detail " + historyIndex;
                list.Add(row);
            }
            detailedHistoryObjects[historyIndex] = list;

        }
        else
        {
            obj.transform.localScale = new Vector3(obj.transform.localScale.x, obj.transform.localScale.y * -1, obj.transform.localScale.z);
            for (int i = 0; i < detailedHistoryObjects[historyIndex].Count; i++)
            {
                Destroy(detailedHistoryObjects[historyIndex][i]);
            }
            detailedHistoryObjects[historyIndex].Clear();
        }
    }

    void OpenHistory()
    {
        if(!canOpenHistoryAndExit || game.ballCount > 0)
            return;

        if (detailedHistoryObjects != null)
        {
            for (int i = 0; i < detailedHistoryObjects.Length; i++)
            {
                if (detailedHistoryObjects[i] != null)
                {
                    for (int j = 0; j < detailedHistoryObjects[i].Count; j++)
                    {
                        Destroy(detailedHistoryObjects[i][j]);
                    }
                    detailedHistoryObjects[i].Clear();
                }
            }
        }

        for (int i = 0; i < historyRows.Count; i++)
        {
            historyRows[i].GetChild(3).transform.localScale = new Vector3(1, -1, 1);
        }
        detailedHistoryObjects = new List<GameObject>[10];

        StartCoroutine(API.GetHistoryDataIE(PassingHistoryData));
    }

    void OpenTutorial()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        tutorialDisplay.OpenTutorial();
#else
        currentTutorialPage = 0;
        SetupTutorialWindow();
        TutorialUI.SetActive(true);
        //tutorialDisplay.OpenTutorial();
#endif
    }

    void ExitGame()
    {
        if (!canOpenHistoryAndExit || game.ballCount > 0)
            return;

        if (fadeScreen.activeInHierarchy)
        {
            TitleScreen.SetActive(true);
            showMenu = false;
            ShowMenuButtons();
            StartCoroutine(PlayAlienAnimationIE());
        }
    }

    void PassingHistoryData(HistoryResponse response)
    {
        historyRows.ForEach(x => x.gameObject.SetActive(false));
        List<HistoryData> historyList = response.data;


        /* old code that display directly from API */
        //if (historyList.Count > 0)
        //{
        //    for (int i = 0; i < historyList.Count; i++)
        //    {
        //        int index = i;
        //        HistoryData data = historyList[i];
        //        historyRows[i].GetComponent<Image>().color = historyRowColor[i % 2 == 0 ? 1 : 0];
        //        historyRows[i].GetChild(0).GetComponent<TextMeshProUGUI>().text = data.created_date.Replace(" ", "\n");
        //        historyRows[i].GetChild(1).GetComponent<TextMeshProUGUI>().text = playerCurrency + " " + data.data.total_amount.ToString("G30");
        //        //historyRows[i].GetChild(2).GetComponent<TextMeshProUGUI>().text = playerCurrency + " " + data.data.total_win.ToString("G30");
        //        historyRows[i].GetChild(2).GetComponent<TextMeshProUGUI>().text = playerCurrency + " " + data.result.total_win.ToString("G30");
        //        AssignHistoryDetailButton(historyRows[i].GetChild(3).gameObject, data.result.game_results, index);
        //        historyRows[i].gameObject.SetActive(true);
        //    }
        //}


        // checking if its first time history opening or not is by empty runtime HistoryRecord
        if (historyRecords.Count == 0)
        {
            //  pass all historyList to historyRecord
            for (int i = 0; i < historyList.Count; i++)
            {
                HistoryData data = historyList[i];
                AddHistory(data.result.total_win, data.result.game_results);
            }
        }

        foreach (var item in historyRows)
        {
            item.gameObject.SetActive(false);
        }
        for (int i = 0; i < historyRecords.Count; i++)
        {
            int index = i;
            historyRows[i].GetComponent<Image>().color = historyRowColor[i % 2 == 0 ? 1 : 0];
            historyRows[i].GetChild(0).GetComponent<TextMeshProUGUI>().text = historyRecords[i].created_date;
            historyRows[i].GetChild(1).GetComponent<TextMeshProUGUI>().text = playerCurrency + " " + StringHelper.MoneyFormat(historyRecords[i].bet, playerCurrency); //historyRecords[i].bet.ToString("G30");
            historyRows[i].GetChild(2).GetComponent<TextMeshProUGUI>().text = playerCurrency + " " + StringHelper.MoneyFormat(historyRecords[i].result, playerCurrency); //historyRecords[i].result.ToString("G30");
            AssignHistoryDetailButton(historyRows[i].GetChild(3).gameObject, historyRecords[i].game_results, index);
            historyRows[i].gameObject.SetActive(true);
        }

        HistoryUI.SetActive(true);
    }

    string GetNumberLabel(float number)
    {

        string text = number.ToString("G30");
        if (number >= 1000)
        {
            float dividedNum = number / 1000;
            text = dividedNum.ToString("G30") + "K";
            if (dividedNum >= 1000)
            {
                text = (dividedNum / 1000).ToString("G30").Replace(".", ",") + "M";
            }
        }

        if(playerCurrency.ToLower() != "idr")
        {
            text = text.Replace(",", ".");
        }

        return playerCurrency + " " + text;
    }

    void OnSubMenuButton(int index, bool playSound = true)
    {
        if (playSound) PlayUISfx();

        if (index == 0)
        {
            OpenTutorial();
        }
        else if (index == 1)
        {
            AudioMenuUI.SetActive(true);
        }
        else if (index == 2)
        {
            OpenHistory();
        }
        else if (index == 3)
        {
            ExitGame();
        }
        else
        {
            if (InternetConnectivity.IsConnected)
            {
                TutorialUI.SetActive(false);
                AudioMenuUI.SetActive(false);
                HistoryUI.SetActive(false);
                messageInfoUI.SetActive(false);
            }
        }
    }

    IEnumerator AnimateLaunchHighlightIconIE()
    {
        launchHighlightIcon.transform.position = launchHighlightIconPos[0].position;
        while (true)
        {
            launchHighlightIcon.transform.DOMove(launchHighlightIconPos[1].position, launchHighlightDelay).SetEase(Ease.Linear);
            yield return new WaitForSeconds(launchHighlightDelay * 1.5F);
            launchHighlightIcon.transform.DOMove(launchHighlightIconPos[0].position, launchHighlightDelay).SetEase(Ease.Linear);
            yield return new WaitForSeconds(launchHighlightDelay);
        }
    }

    void SetAutoPlayRoundsButton()
    {
        for (int i = 0; i < autoPlayRoundNumberButton.Count; i++)
        {
            autoPlayRoundNumberButton[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = roundNumbers[i].ToString();
            int index = i;

            EventTrigger eventTrigger = autoPlayRoundNumberButton[index].AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new()
            {
                eventID = EventTriggerType.PointerDown
            };
            entry.callback.AddListener((data) => { OnAutoPlayRoundSelected(index); });
            eventTrigger.triggers.Add(entry);
        }
    }

    void OnAutoPlayRoundSelected(int index)
    {
        selectedAutoPlayRoundsIndex = index;
        ShowRoundCount(roundNumbers[selectedAutoPlayRoundsIndex]);
        for (int i = 0; i < autoPlayRoundNumberButton.Count; i++)
        {
            autoPlayRoundNumberButton[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.white;
        }
        autoPlayRoundNumberButton[index].transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = Color.yellow;
    }

    public void ShowRoundCount(int number)
    {
        txtRoundCount.text = number.ToString();
    }

    void SetStartLaunchButton()
    {
        EventTrigger eventTrigger = startRoundButton.AddComponent<EventTrigger>();
        EventTrigger.Entry entryStart = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryStart.callback.AddListener((data) => { StartRound(); });
        eventTrigger.triggers.Add(entryStart);
    }

    void SetAutoPlayButton()
    {
        EventTrigger eventTrigger = autoPlayButton.AddComponent<EventTrigger>();
        EventTrigger.Entry entryStart = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryStart.callback.AddListener((data) => { OpenAutoPlayUI(true); });
        eventTrigger.triggers.Add(entryStart);

        EventTrigger closeEventTrigger = closeAutoPlayButton.AddComponent<EventTrigger>();
        EventTrigger.Entry entryExit = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryExit.callback.AddListener((data) => { OpenAutoPlayUI(false); });
        closeEventTrigger.triggers.Add(entryExit);

        EventTrigger confirmEventTrigger = confirmAutoPlayButton.AddComponent<EventTrigger>();
        EventTrigger.Entry entryConfirm = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryConfirm.callback.AddListener((data) => { StartRound(); });
        confirmEventTrigger.triggers.Add(entryConfirm);
    }

    public void OpenAutoPlayUI(bool isOpen) 
    {
        launchButtonImage.sprite = launchButtonSprite[isOpen ? 1 : 0];
        ShowLauncHighlightIcon(!isOpen);
        txtRoundCount.gameObject.SetActive(isOpen);
        txtLaunchBall.gameObject.SetActive(!isOpen);
        isAutoPlay = isOpen;
        autoPlayUI.SetActive(isOpen);
    }

    public void ShowLauncHighlightIcon(bool isShow)
    {
        launchHighlightIcon.gameObject.SetActive(isShow);
    }


    public void ShowButtonUI(bool isShow)
    {
        btnUIObj.SetActive(true);
        fadeScreen.SetActive(isShow);
        btnUIObj.transform.DOMoveY(btnUIPos[isShow ? 0 : 1].position.y, 0.5f).SetEase(Ease.Linear);
    }

    void SetPopUpBetButton()
    {
        popUpBetInfo.transform.position = popUpBetInfoPos[1].position;
        EventTrigger eventTrigger = popUpBetButton.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entry.callback.AddListener((data) => {
            showPopUpBet = !showPopUpBet;
            ShowPopUpBetInfo(showPopUpBet);
        });
        eventTrigger.triggers.Add(entry);

        EventTrigger eventTriggerClose = popUpBetCloseButton.AddComponent<EventTrigger>();
        EventTrigger.Entry entryClose = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryClose.callback.AddListener((data) => { ClosePopUpInfo(); });
        eventTriggerClose.triggers.Add(entryClose);

    }

    public void ClosePopUpInfo()
    {
        if (showPopUpBet)
        {
            showPopUpBet = false;
            ShowPopUpBetInfo(false);
        }
    }

    public void ShowPopUpBetInfo(bool isShow)
    {
        popUpBetInfo.DOMoveX(popUpBetInfoPos[isShow ? 0 : 1].position.x, 0.5f).SetEase(Ease.Linear);
    }

    void AssignMenuButtons()
    {
        EventTrigger eventTrigger = buttonPlay.AddComponent<EventTrigger>();
        EventTrigger.Entry entryStart = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryStart.callback.AddListener((data) => { GoToGameScreen(); });
        eventTrigger.triggers.Add(entryStart);

        EventTrigger eventTriggerTutorial = buttonTutorial.AddComponent<EventTrigger>();
        EventTrigger.Entry entryTutorial = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryTutorial.callback.AddListener((data) => { OnSubMenuButton(0); });
        eventTriggerTutorial.triggers.Add(entryTutorial);
    }

    void SetBetButtons()
    {
        EventTrigger triggerDecrease = decreaseBetButton.AddComponent<EventTrigger>();
        EventTrigger.Entry entryDecrease = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryDecrease.callback.AddListener((data) => { ChangeBet(false); });
        triggerDecrease.triggers.Add(entryDecrease);

        EventTrigger triggerIncrease = increaseBetButton.AddComponent<EventTrigger>();
        EventTrigger.Entry entryIncrease = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryIncrease.callback.AddListener((data) => { ChangeBet(true); });
        triggerIncrease.triggers.Add(entryIncrease);
    }

    void SetTutorialButtons()
    {
        EventTrigger triggerNext = tutorialNextButton.AddComponent<EventTrigger>();
        EventTrigger.Entry entryNext = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryNext.callback.AddListener((data) => { 
            currentTutorialPage++;
            if(currentTutorialPage >= tutorialContents.Length) currentTutorialPage = 0;
            SetupTutorialWindow();
        });
        triggerNext.triggers.Add(entryNext);

        EventTrigger triggerPrev = tutorialPrevButton.AddComponent<EventTrigger>();
        EventTrigger.Entry entryPrev = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entryPrev.callback.AddListener((data) => {
            currentTutorialPage--;
            if (currentTutorialPage < 0) currentTutorialPage = tutorialContents.Length - 1;
            SetupTutorialWindow();
        });
        triggerPrev.triggers.Add(entryPrev);

        for (int i = 0; i < tutorialPageIndikator.Length; i++)
        {
            int index = i;
            EventTrigger triggerPage = tutorialPageIndikator[index].AddComponent<EventTrigger>();
            EventTrigger.Entry entryPage = new()
            {
                eventID = EventTriggerType.PointerDown
            };
            entryPage.callback.AddListener((data) => {
                currentTutorialPage = index;
                SetupTutorialWindow();
            });
            triggerPage.triggers.Add(entryPage);
        }
    }


    void ChangeBet(bool isIncrease, bool playSound = true)
    {
        int prevChip = selectedChipValueIndex;
        selectedChipValueIndex += isIncrease ? 1 : -1;
        selectedChipValueIndex = Mathf.Clamp(selectedChipValueIndex, 0, chipValues.Length - 1);
        currentBet = chipValues[selectedChipValueIndex];
        if (currentBet * 5 < minBet || currentBet * 5 > maxBet)
        {
            selectedChipValueIndex = prevChip;
            currentBet = chipValues[selectedChipValueIndex];
            return;
        }

        if (playSound) PlayUISfx();

        currentBet = Mathf.Max(currentBet, 0);
        txtBet.text = playerCurrency + " " + StringHelper.MoneyFormat(currentBet, playerCurrency);
        txtBetInfo.text = playerCurrency + " " + StringHelper.MoneyFormat(currentBet, playerCurrency);
        txtTotalBet.text = LanguageManager.Instance.GetLabel("txt_total") + ": " + playerCurrency + " " + StringHelper.MoneyFormat(currentBet * 5, playerCurrency);
        txtTotalBetMiddle.text = playerCurrency + " " + StringHelper.MoneyFormat(currentBet * 5, playerCurrency);

        EnableStartRoundButton(currentBet > 0);
    }

    void EnableStartRoundButton(bool isEnable)
    {
        startRoundButton.GetComponent<Image>().color = buttonColor[isEnable ? 1 : 0];
    }

    //void OnSessionSelected(int index)
    //{
    //    for (int i = 0; i < chipButtonsText.Count; i++)
    //    {
    //        chipButtonsText[i].text = (chipValues[i] * selectedSession).ToString() + "K";
    //    }
    //    ShowRestOfBall(selectedSession);
    //}


    public void ShowRestOfBall(int ballCount)
    {
        txtBallCount.text = ballCount.ToString();
        txtBallLeft.text = ballCount <= 1 ? 
            LanguageManager.Instance.GetLabel("txt_ball_left_single") : LanguageManager.Instance.GetLabel("txt_ball_left_plural");
    }

    void GoToGameScreen()
    {
        SFX.PlayOneShot(sfxClips[0]);
        TitleScreen.SetActive(false);
        ShowButtonUI(true);
    }

    void StartRound()
    {
        float totalBet = currentBet * 5;
        if (totalBet <= 0)
            return;

        if (!canPressPlay)
            return;
        canPressPlay = false;
        SetCanOpenHistoryAndExit(false);
        ShowRoundCount(roundNumbers[selectedAutoPlayRoundsIndex]);
        game.StartSession(selectedSession, isAutoPlay, roundNumbers[selectedAutoPlayRoundsIndex], totalBet);
    }

    public void HideBetMenuButtons()
    {
        PlayUISfx();
        ShowRestOfBall(selectedSession);
        ShowButtonUI(false);
    }

    public void ResetButtonPlayStatus()
    {
        canPressPlay = true;
        if (!game.isAutoPlay)
        {
            SetCanOpenHistoryAndExit(true);
        }
    }

    public float GetSingleBallBet()
    {
        return currentBet;
    }

    public void ChangePlayerBallance(string balance)
    {
        totalPlayerBalance = decimal.Parse(balance);
        txtTotalBalance.text = playerCurrency + " " + StringHelper.MoneyFormat(totalPlayerBalance, playerCurrency);
        txtTotalBalanceMiddle.text = txtTotalBalanceInfo.text = playerCurrency + " " + StringHelper.MoneyFormat(totalPlayerBalance, playerCurrency);
    }

    void PlayUISfx()
    {
        SFX.PlayOneShot(sfxClips[UnityEngine.Random.Range(1, sfxClips.Length)]);
    }

    void SetupTutorialWindow()
    {
        for (int i = 0; i < tutorialPageIndikator.Length; i++)
        {
            tutorialContents[i].SetActive(i == currentTutorialPage);
            tutorialPageIndikator[i].GetComponent<Image>().sprite = tutorialIndikatorSkin[i == currentTutorialPage ? 0 : 1];
        }
    }

    public void ShowMessageInfo(string message = "")
    {
        txtMessageInfo.text = message;
        messageInfoUI.SetActive(true);
    }

    public decimal GetCurrentBalance()
    {
        return totalPlayerBalance;
    }

    public void SetCanOpenHistoryAndExit(bool canOpen)
    {
        canOpenHistoryAndExit = canOpen;
        subMenuButtons[2].GetComponent<Image>().color = canOpenHistoryAndExit ? Color.white : new Color32(143, 143, 143, 255);
        subMenuButtons[3].GetComponent<Image>().color = canOpenHistoryAndExit ? Color.white : new Color32(143, 143, 143, 255);
    }

    public void ChangeTempPlayerBallance(string balance)
    {
        decimal tempBalance = decimal.Parse(balance);
        txtTotalBalance.text = playerCurrency + " " + StringHelper.MoneyFormat(tempBalance, playerCurrency);
        txtTotalBalanceMiddle.text =  txtTotalBalanceInfo.text = playerCurrency + " " + StringHelper.MoneyFormat(tempBalance, playerCurrency);
    }

    private void OnEnable()
    {
        InternetConnectivity.OnConnectivityChanged += OnInternetChange;
        txt_no_connection.SetActive(!InternetConnectivity.IsConnected);
        if (!InternetConnectivity.IsConnected)
        {
            ShowMessageInfo("");
        }
    }

    private void OnDisable()
    {
        InternetConnectivity.OnConnectivityChanged -= OnInternetChange;
    }

    private void OnInternetChange(bool isOnline)
    {
        API.Log(isOnline ? "Connected to Internet" : "Disconnected from Internet");
        txt_no_connection.SetActive(!isOnline);
        if (!isOnline)
        {
            Time.timeScale = 0f;
            ShowMessageInfo("");
        }
        else
        {
            Time.timeScale = 1f;
            OnSubMenuButton(4, false); // close all menu windows including message box
        }
    }
}
