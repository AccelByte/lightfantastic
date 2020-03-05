using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(CanvasGroup))]
public class SettingTabAndCanvas
{
    [SerializeField] public Button tabButton;
    [SerializeField] public Text tabText;
    [SerializeField] public CanvasGroup canvasSettingComponent;
}

public class SettingPanelLogic : MonoBehaviour
{
    [SerializeField] private SettingTabAndCanvas audio;
    [SerializeField] private SettingTabAndCanvas video;
    [SerializeField] private SettingTabAndCanvas network;

    [Header("Audio")]
    [SerializeField] private SettingComponentBoolean settingAudioMasterVolume;
    [SerializeField] private SettingComponentBoolean settingAudioBGMVolume;
    [SerializeField] private SettingComponentBoolean settingAudioSFXVolume;

    private SettingTabAndCanvas[] tabAndCanvases;

    private void Start()
    {
        tabAndCanvases = new []{audio, video, network};
        
        RegisterTabButtonsOnSelectEvent();
        RegisterAudioSettingComponentEvents();
        
        settingAudioMasterVolume.SetTrue();
        
        audio.tabButton.onClick.Invoke();
    }

    private void RegisterTabButtonsOnSelectEvent()
    {
        foreach (var entry in tabAndCanvases)
        {
            entry.tabButton.onClick.AddListener(()=>
            {
                foreach (var i in tabAndCanvases)
                {
                    SetTabActive(i, false);
                }
                SetTabActive(entry, true);
            });
        }
    }

    private void RegisterAudioSettingComponentEvents()
    {
        settingAudioMasterVolume.AddListeners(
            delegate
            {
                settingAudioBGMVolume.SetTrue();
                settingAudioSFXVolume.SetTrue();
            },
            delegate
            {
                settingAudioBGMVolume.SetFalse();
                settingAudioSFXVolume.SetFalse();
                // Disable child
                settingAudioBGMVolume.DisableSetting();
                settingAudioSFXVolume.DisableSetting();
            });

        settingAudioBGMVolume.AddListeners(
            delegate { AudioManager.Instance.ToggleBGMVolume(true); },
            delegate { AudioManager.Instance.ToggleBGMVolume(false); });

        settingAudioSFXVolume.AddListeners(
            delegate { AudioManager.Instance.ToggleSFXVolume(true); },
            delegate { AudioManager.Instance.ToggleSFXVolume(false); });
    }

    private void SetTabActive(SettingTabAndCanvas settingTabAndCanvas, bool active)
    {
        MainThreadTaskRunner.Instance.Run(delegate
        {
            settingTabAndCanvas.canvasSettingComponent.alpha = active ? 1 : 0;
            settingTabAndCanvas.canvasSettingComponent.interactable = active;
            settingTabAndCanvas.canvasSettingComponent.blocksRaycasts = active;
            settingTabAndCanvas.tabButton.interactable = !active;
            settingTabAndCanvas.tabText.color = active ? Color.white : new Color((float) 119/255, (float) 36/255, (float)206/255);
        });
    }
}
