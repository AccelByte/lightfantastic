using UnityEngine;
using UnityEngine.UI;

public class MainHUD : BaseHUD
{
    [SerializeField]
    private GameTimer timer_;
    [SerializeField]
    private Button pauseButton_;

    private Game.InGameHudManager hudMgr;

    protected override void Awake()
    {
        base.Awake();
        hudMgr = GetComponentInParent<Game.InGameHudManager>();
        canvas_.sortingLayerName = "UI";
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

    private void ShowPauseScreen()
    {
        hudMgr.ShowPauseScreen();
    }
}
