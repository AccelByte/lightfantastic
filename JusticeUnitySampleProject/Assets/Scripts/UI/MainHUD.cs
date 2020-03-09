using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class MainHUD : BaseHUD
{
    [SerializeField]
    private Button pauseButton_;
    [SerializeField]
    public Button leftRunButton_;
    [SerializeField]
    public Button rightRunButton_;
    [SerializeField]
    private MainHUDTimerPrefab raceTimerPrefab_ = null;
    // countdown
    [SerializeField]
    private TextMeshProUGUI countDownText_ = null;

    private Game.InGameHudManager hudMgr;
    private Game.BaseGameManager gameMgr;
    private GameTimer gameTimer_;
    private CountDown countDownTimer_;

    protected override void Awake()
    {
        base.Awake();
        hudMgr = GetComponentInParent<Game.InGameHudManager>();
        gameMgr = GameObject.FindGameObjectWithTag("GameManager").GetComponent<Game.BaseGameManager>();
        canvas_.sortingLayerName = "UI";
        TouchButtonInit();
    }

    protected override void Start()
    {
        base.Start();

    }

    public override void OnShow()
    {
    }

    public override void OnHide()
    {
    }

    public override void SetupData(object[] args)
    {
    }

    protected override void AddListeners()
    {
        pauseButton_.onClick.AddListener(ShowPauseScreen);
        gameMgr.onAllplayerReady += OnAllPlayerConnected;
        gameMgr.onGameStart += OnGameStart;
        gameMgr.onGameEnd += OnGameEnd;
    }

    protected void RemoveListeners()
    {
        pauseButton_.onClick.RemoveListener(ShowPauseScreen);

        if (gameMgr != null)
        {
            gameMgr.onAllplayerReady -= OnAllPlayerConnected;
            gameMgr.onGameStart -= OnGameStart;
            gameMgr.onGameEnd -= OnGameEnd;
        }

        if (gameTimer_ != null)
        {
            gameTimer_.timerUpdated -= OnTimerUpdated;
            gameTimer_.onTimerExpired -= OnGameTimerExpired;
        }

        if (countDownTimer_ != null)
        {
            countDownTimer_.timerUpdated -= OnTimerUpdated;
            countDownTimer_.onTimerExpired += OnCountDownStartExpired;
        }
    }

    public void AttachTimer(GameTimer gameTimer)
    {
        gameTimer_ = gameTimer;
        gameTimer.timerUpdated += OnTimerUpdated;
        gameTimer.onTimerExpired += OnGameTimerExpired;
    }

    // countdown
    public void AttachCountDown(CountDown countDownTimer)
    {
        countDownTimer_ = countDownTimer;
        countDownTimer.timerUpdated += OnCountDownUpdated;
        countDownTimer.onTimerExpired += OnCountDownStartExpired;
    }

    private void OnTimerUpdated(int newValue)
    {
        MainThreadTaskRunner.Instance.Run(() =>
        {
            raceTimerPrefab_.SetTime(newValue);
        });
    }

    // countdown
    private void OnCountDownUpdated(int newValue)
    {
        MainThreadTaskRunner.Instance.Run(() =>
        {
            if (newValue < 1)
            {
                countDownText_.text =  "GO";
            }
            else
            {
                countDownText_.text =  newValue.ToString();
            }
        });
    }

    private void OnGameTimerExpired()
    {
        // after game timer expired then show result screen
        Debug.Log("MainHUD OnGameTimerExpired");
        gameMgr.GameTimeOver();
    }

    private void OnCountDownStartExpired()
    {
        Debug.Log("MainHUD OnCountDownStartExpired");
        gameTimer_.StartGameTimer();
        gameMgr.StartGame();
        countDownText_.gameObject.SetActive(false);
    }

    private void ShowPauseScreen()
    {
        hudMgr.ShowPauseScreen();
    }

    private void TouchButtonInit()
    {
        bool touchInteraction = false;
#if UNITY_ANDROID || UNITY_SWITCH
        touchInteraction = true;
#endif
        leftRunButton_.gameObject.SetActive(touchInteraction);
        rightRunButton_.gameObject.SetActive(touchInteraction);
    }

    private void OnDestroy()
    {
        if (gameMgr != null)
        {
            gameMgr.onAllplayerReady -= OnAllPlayerConnected;
        }

        if (gameTimer_ != null)
        {
            gameTimer_.timerUpdated -= OnTimerUpdated;
        }
    }

    private void OnAllPlayerConnected()
    {
        countDownTimer_.StartCountDown();
    }

    private void OnGameStart()
    {
        countDownText_.gameObject.SetActive(false);
    }

    private void OnGameEnd()
    {
        // remove all listeners
        RemoveListeners();
    }
}
