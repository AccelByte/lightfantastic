// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using Button = UnityEngine.UI.Button;
using TMPro;
using UITools;
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
    
    private UIElementHandler uiElementHandler;

    private void Start()
    {
        uiElementHandler = GameObject.FindGameObjectWithTag("UIHandler").GetComponent<UIElementHandler>();
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
        
        uiElementHandler.inventoryButton.onClick.AddListener(UnselectAll);
        uiElementHandler.leaderboardButton.onClick.AddListener(UnselectAll);
        uiElementHandler.settingsButton.onClick.AddListener(UnselectAll);
        uiElementHandler.logoutButton.onClick.AddListener(UnselectAll);
        uiElementHandler.exitButton.onClick.AddListener(UnselectAll);
        uiElementHandler.chatButton.onClick.AddListener(UnselectAll);
    }

    private void _OnOnlineButtonClicked()
    {
        GlowHeader();
        ShowMatchmakingPanel();
        GlowTextMesh(onlineButton);
        DimTextMesh(localButton);
    }
    
    private void _OnLocalButtonClicked()
    {
        GlowHeader();
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
            uiElementHandler.ShowNonExclusivePanel(NonExclusivePanelType.MATCHMAKING);
            matchmakingPanelUiComponent.gameObject.GetComponent<TweenComponent>().AnimateSwipeRight();
        });
    }

    private void HideMatchmakingPanel()
    {
        MainThreadTaskRunner.Instance.Run(() =>
        {
            uiElementHandler.HideNonExclusivePanel(NonExclusivePanelType.MATCHMAKING);
            matchmakingPanelUiComponent.gameObject.GetComponent<TweenComponent>().AnimateSwipeToOriginalLocation();
        });
    }
}
