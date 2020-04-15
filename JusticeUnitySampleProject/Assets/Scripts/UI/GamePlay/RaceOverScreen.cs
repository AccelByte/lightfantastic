using UnityEngine;
using UnityEngine.UI;

public class RaceOverScreen : BaseHUD
{
    private readonly float REDIRECTING_COUNTDOWN = 10.0f;
    
    #region Fields and Properties
    [Header("UI Elements")]
    [SerializeField, Tooltip("The button to return to menu")]
    private Button toMenuButton_ = null;

    [SerializeField, Tooltip("WIN canvas group component")]
    private CanvasGroup winCanvasGroup_ = null;
    public CanvasGroup WinCanvasGroup => winCanvasGroup_;

    [SerializeField, Tooltip("LOSE canvas group component")]
    private CanvasGroup loseCanvasGroup_ = null;
    public CanvasGroup LoseCanvasGroup => loseCanvasGroup_;

    [SerializeField, Tooltip("Redirecting countdown text")]
    private Text redirectingText_ = null;
    public Text RedirectingText => redirectingText_;

    private float timeLeft;
    private bool isCountingDown;
    private Game.InGameHudManager hudMgr;
    #endregion //Fields and Properties

    protected override void Awake()
    {
        base.Awake();
        Debug.Log("Race Over Panel Pos Z: " + rectTr_.localPosition.z);
        hudMgr = gameObject.GetComponentInParent<Game.InGameHudManager>();
        canvas_.sortingLayerName = "UI";
    }

    protected override void Start()
    {
        base.Start();
    }

    public override void OnShow()
    {
        Debug.Log("OnShow");
    }

    public override void OnHide()
    {
        Debug.Log("OnHide");
    }

    public override void SetupData(object[] args)
    {
        if ((bool) args[0])
        {
            winCanvasGroup_.alpha = 1;
            loseCanvasGroup_.alpha = 0;
        }
        else
        {
            loseCanvasGroup_.alpha = 1;
            winCanvasGroup_.alpha = 0;
        }

        timeLeft = REDIRECTING_COUNTDOWN;
        isCountingDown = true;
    }

    protected override void AddListeners()
    {
        toMenuButton_.onClick.AddListener(ReturnToMenu);
    }

    private void ReturnToMenu()
    {
        hudMgr.DisconnectPlayer();
    }

    private void Update()
    {
        if (isCountingDown)
        {
            timeLeft -= Time.deltaTime;
            redirectingText_.text = $"Redirecting to main menu within {(int)timeLeft} secs ...";
            if (timeLeft < 0)
            {
                isCountingDown = false;
                timeLeft = REDIRECTING_COUNTDOWN;
                ReturnToMenu();
            }
        }
    }
}
