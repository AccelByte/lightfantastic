using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RaceOverScreen : BaseHUD
{
    #region Fields and Properties
    [Header("UI Elements")]
    [SerializeField, Tooltip("The button to return to menu")]
    private Button toMenuButton_ = null;
    [SerializeField]
    private Button dummyButton_ = null;

    [SerializeField, Tooltip("Race over announcer text")]
    private TextMeshProUGUI announcerText_ = null;
    public TextMeshProUGUI AnnouncerText { get { return announcerText_; } }

    [SerializeField, Tooltip("The text to announce race winner")]
    private TextMeshProUGUI winnerText_ = null;
    public TextMeshProUGUI WinnerText { get { return winnerText_; } }
    private Game.BaseGameManager gameMgr;
    private InGameHudManager hudMgr;
    #endregion //Fields and Properties

    protected override void Awake()
    {
        base.Awake();
        Debug.Log("Race Over Panel Pos Z: " + rectTr_.localPosition.z);
        //gameMgr = GameObject.FindGameObjectWithTag("GameManager").GetComponent<Game.BaseGameManager>();
        hudMgr = gameObject.GetComponentInParent<InGameHudManager>();
    }

    private void Start()
    {
        AddListeners();
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
        //string winAnnouncement = args[0] as string;
        //winnerText_.text = winAnnouncement;
    }

    protected override void AddListeners()
    {
        toMenuButton_.onClick.AddListener(() => { hudMgr.HideRaceOverScreen(); });
        dummyButton_.onClick.AddListener(() => { hudMgr.ShowRaceOverScreen(); });
    }

    private void ReturnToMenu()
    {
        gameMgr.InstanceNetworker.Disconnect(false);
    }
}
