// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections.Generic;
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
    private Tuple<uint, uint>[] videoResolutionList = new[]
    {
        new Tuple<uint, uint>( 1024, 576 ),
        new Tuple<uint, uint>( 1280, 720 ),
        new Tuple<uint, uint>( 1366, 768 ),
        new Tuple<uint, uint>( 1600, 900 ),
        new Tuple<uint, uint>( 1920, 1080 ),
    };
    
    [SerializeField] private SettingTabAndCanvas audio;
    [SerializeField] private SettingTabAndCanvas video;
    [SerializeField] private SettingTabAndCanvas network;

    [Header("Audio")]
    [SerializeField] private SettingComponentBoolean settingAudioMasterVolume;
    [SerializeField] private SettingComponentBoolean settingAudioBGMVolume;
    [SerializeField] private SettingComponentBoolean settingAudioSFXVolume;

    [Header("Video")]
    [SerializeField] private SettingComponentFromList settingVideoResolution;

    private SettingTabAndCanvas[] tabAndCanvases;

    private void Start()
    {
        tabAndCanvases = new []{audio, video, network};
        
        settingVideoResolution.Init(
            VideoResolutionTupleToList(videoResolutionList), OnVideoResolutionSelected, GetCurrentResolutionIndex());
        
        RegisterTabButtonsOnSelectEvent();
        RegisterAudioSettingComponentEvents();
        
        MainThreadTaskRunner.Instance.Run(InitialLoadConfig);
        
        audio.tabButton.onClick.Invoke();
    }

    /// Turn off all audio & load config player preference
    private void InitialLoadConfig()
    {
        bool bgmIsOn = AudioManager.Instance.GetAudioState().BGM_On;
        bool sfxIsOn = AudioManager.Instance.GetAudioState().SFX_On;
        if (bgmIsOn && sfxIsOn)
        {
            settingAudioMasterVolume.SetTrue();
        }
        else if (!bgmIsOn && !sfxIsOn)
        {
            settingAudioMasterVolume.SetFalse();
        }
        else
        {
            settingAudioMasterVolume.SetTrue();

            if (!bgmIsOn)
            {
                settingAudioBGMVolume.SetFalse();
            }

            if (!sfxIsOn)
            {
                settingAudioSFXVolume.SetFalse();
            }
        }
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

    private string[] VideoResolutionTupleToList(Tuple<uint, uint>[] tuples)
    {
        List<string> output = new List<string>(); 

        foreach (var entry in tuples)
        {
            output.Add($"{entry.Item1} X {entry.Item2}");
        }

        return output.ToArray();
    }

    private void OnVideoResolutionSelected(uint index)
    {
        Screen.SetResolution((int) videoResolutionList[index].Item1, (int) videoResolutionList[index].Item2, false);
    }

    private uint GetCurrentResolutionIndex()
    {
        for (uint i = 0; i < videoResolutionList.Length; i++)
        {
            var list = videoResolutionList[i];
            if (Screen.currentResolution.width == list.Item1)
            {
                return i;
            }
        }

        return 0;
    }
}
