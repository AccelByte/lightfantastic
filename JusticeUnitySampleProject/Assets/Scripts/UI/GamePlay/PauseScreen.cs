using UnityEngine;
using UnityEngine.UI;

public class PauseScreen : BaseHUD
{
    #region Fields and Properties
    [SerializeField]
    private Button backButton_;
    [SerializeField]
    private Button disconnectButton_;
    private Game.InGameHudManager hudMgr_;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        hudMgr_ = GetComponentInParent<Game.InGameHudManager>();
        canvas_.sortingLayerName = "UI";
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
        disconnectButton_.onClick.AddListener(DisconnectPlayer);
        backButton_.onClick.AddListener(HidePauseScreen);
    }

    private void DisconnectPlayer()
    {
        hudMgr_.DisconnectPlayer();
    }

    private void HidePauseScreen()
    {
        hudMgr_.HidePauseScreen();
    }
}