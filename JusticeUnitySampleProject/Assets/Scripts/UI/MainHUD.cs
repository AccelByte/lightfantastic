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
    private TextMeshProUGUI timerText_ = null;

    private Game.InGameHudManager hudMgr;
    private Game.BaseGameManager gameMgr;
    private GameTimer gameTimer_;

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
    }

    public void AttachTimer(GameTimer gameTimer)
    {
        gameTimer_ = gameTimer;
        gameTimer.timerUpdated += OnTimerUpdated;
    }

    private void OnTimerUpdated(int newValue)
    {
        MainThreadTaskRunner.Instance.Run(() => { timerText_.text = "Time Remaining: " + newValue.ToString(); });
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
        if (gameTimer_ != null)
        {
            gameTimer_.timerUpdated -= OnTimerUpdated;
        }
    }
}
