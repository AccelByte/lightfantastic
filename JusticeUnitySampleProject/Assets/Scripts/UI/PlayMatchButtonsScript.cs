using System;
using Button = UnityEngine.UI.Button;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct PlayMatchTextButton
{
    public Button button;
    public TextMeshProUGUI text;
}

public class PlayMatchButtonsScript : MonoBehaviour
{
    [SerializeField] private Text header;
    [SerializeField] private PlayMatchTextButton onlineButton;
    [SerializeField] private PlayMatchTextButton localButton;
    [SerializeField] private UIMatchmakingPanelComponent matchmakingPanelUiComponent;
    [SerializeField] private TMP_FontAsset glowFontMaterial;
    [SerializeField] private TMP_FontAsset normalFontMaterial;
    public Button.ButtonClickedEvent OnOnlineButtonClicked => onlineButton.button.onClick;
    public Button.ButtonClickedEvent OnLocalButtonClicked => localButton.button.onClick;
    public Button.ButtonClickedEvent On1VS1ButtonClicked => matchmakingPanelUiComponent.Mode1VS1Button.onClick;
    public Button.ButtonClickedEvent On4FFAButtonClicked => matchmakingPanelUiComponent.Mode4FFAButton.onClick;

    private void Start()
    {
        UnselectAll();
        RegisterButtonUIEvent();
    }

    public void UnselectAll()
    {
        DimTextMesh(onlineButton);
        DimTextMesh(localButton);
        DimHeader();
        HideMatchmakingPanel();
    }

    public enum ButtonList
    {
        OnlineButton,
        LocalButton
    }
    
    public void SetInteractable(ButtonList button, bool interactable)
    {
        switch (button)
        {
            case ButtonList.OnlineButton:
                onlineButton.button.interactable = interactable;
                break;
            case ButtonList.LocalButton:
                localButton.button.interactable = interactable;
                break;
            default:
                return;
        }
    }

    public void DeregisterAllButton()
    {
        OnOnlineButtonClicked.RemoveAllListeners();
        OnLocalButtonClicked.RemoveAllListeners();
        On1VS1ButtonClicked.RemoveAllListeners();
        On4FFAButtonClicked.RemoveAllListeners();
    }

    private void RegisterButtonUIEvent()
    {
        
        onlineButton.button.onClick.RemoveListener(_OnOnlineButtonClicked);
        onlineButton.button.onClick.AddListener(_OnOnlineButtonClicked);
        
        localButton.button.onClick.RemoveListener(_OnLocalButtonClicked);
        localButton.button.onClick.AddListener(_OnLocalButtonClicked);
        
        matchmakingPanelUiComponent.ClosePanelButton.onClick.RemoveAllListeners();
        matchmakingPanelUiComponent.ClosePanelButton.onClick.AddListener(UnselectAll);
    }

    private void _OnOnlineButtonClicked()
    {
        GlowHeader();
        HideMatchmakingPanel();
        ShowMatchmakingPanel();
        GlowTextMesh(onlineButton);
        DimTextMesh(localButton);
    }
    
    private void _OnLocalButtonClicked()
    {
        GlowHeader();
        HideMatchmakingPanel();
        ShowMatchmakingPanel();
        GlowTextMesh(localButton);
        DimTextMesh(onlineButton);
    }

    /// <summary>
    /// Enable glow to the text
    /// </summary>
    /// <param name="buttonText"></param>
    private void GlowTextMesh(PlayMatchTextButton playMatchTextButton)
    {
        MainThreadTaskRunner.Instance.Run(() =>
        {
            playMatchTextButton.text.font = glowFontMaterial;
            playMatchTextButton.text.UpdateFontAsset();
        });
    }

    /// <summary>
    /// Disable glow to the text
    /// </summary>
    /// <param name="buttonText"></param>
    private void DimTextMesh(PlayMatchTextButton playMatchTextButton)
    {
        MainThreadTaskRunner.Instance.Run(() =>
        {
            playMatchTextButton.text.font = normalFontMaterial;
            playMatchTextButton.text.UpdateFontAsset();
        });
    }

    private void GlowHeader()
    {
        MainThreadTaskRunner.Instance.Run(()=> { header.color = Color.cyan; });
    }

    private void DimHeader()
    {
        MainThreadTaskRunner.Instance.Run(()=> { header.color = Color.magenta; });
    }

    private void ShowMatchmakingPanel()
    {
        MainThreadTaskRunner.Instance.Run(() =>
        {
            matchmakingPanelUiComponent.gameObject.GetComponent<TweenComponent>()?.AnimateSwipeRight();
            matchmakingPanelUiComponent.canvasGroup.alpha = 1;
            matchmakingPanelUiComponent.canvasGroup.interactable = true;
            matchmakingPanelUiComponent.canvasGroup.blocksRaycasts = true;
        });
    }

    private void HideMatchmakingPanel()
    {
        MainThreadTaskRunner.Instance.Run(() =>
        {
            matchmakingPanelUiComponent.gameObject.GetComponent<TweenComponent>().AnimateSwipeToOriginalLocation();
            matchmakingPanelUiComponent.canvasGroup.alpha = 0;
            matchmakingPanelUiComponent.canvasGroup.interactable = false;
            matchmakingPanelUiComponent.canvasGroup.blocksRaycasts = false;
        });
    }
}
