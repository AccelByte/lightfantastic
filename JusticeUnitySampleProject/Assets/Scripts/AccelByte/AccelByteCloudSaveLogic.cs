﻿// Copyright (c) 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections.Generic;
using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using UITools;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AccelByteCloudSaveLogic : MonoBehaviour
{
    private CloudSave abCloudSave;

    private GameObject UIHandler;
    private UISettingLogicComponent UIHandlerSettingComponent;
    private UIElementHandler UIElementHandler;

    private static string settingKey = LightFantasticConfig.AUDIO_SETTING_KEY;
    private static Dictionary<string, object> audioSettingRecord = new Dictionary<string, object>
    {
        {LightFantasticConfig.AudioSettingType.BGM, true },
        {LightFantasticConfig.AudioSettingType.SFX, true }
    };

    private void Start()
    {
        abCloudSave = AccelBytePlugin.GetCloudSave();
    }

    #region UI Listener
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (UIHandler != null)
        {
            RemoveListeners();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshUIHandler();
    }

    public void RefreshUIHandler()
    {
        UIHandler = GameObject.FindGameObjectWithTag("UIHandler");
        if (UIHandler == null)
        {
            Debug.Log("ABCloudSave RefreshUIHandler no reference to UI Handler!");
            return;
        }
        UIHandlerSettingComponent = UIHandler.GetComponent<UISettingLogicComponent>();
        UIElementHandler = UIHandler.GetComponent<UIElementHandler>();

        if (abCloudSave == null) abCloudSave = AccelBytePlugin.GetCloudSave();

        AddEventListeners();
    }

    private void AddEventListeners()
    {
        UIHandlerSettingComponent.backButton.onClick.AddListener(ShowPromptPanel);
        UIHandlerSettingComponent.promptPanelSaveButton.onClick.AddListener(delegate
        {
            SaveUserAudioSettingRecord();
            HidePromptPanel();
            UIElementHandler.ShowExclusivePanel(ExclusivePanelType.MAIN_MENU);
        });

        UIHandlerSettingComponent.promptPanelDontSaveButton.onClick.AddListener(delegate
        {
            GetUserAudioSettingRecord();
            HidePromptPanel();
            UIElementHandler.ShowExclusivePanel(ExclusivePanelType.MAIN_MENU);
        });

        UIHandlerSettingComponent.promptPanelCloseButton.onClick.AddListener(delegate
        {
            HidePromptPanel();
        });
    }

    private void RemoveListeners()
    {
        UIHandlerSettingComponent.backButton.onClick.RemoveListener(ShowPromptPanel);
        UIHandlerSettingComponent.promptPanelSaveButton.onClick.RemoveAllListeners();
        UIHandlerSettingComponent.promptPanelDontSaveButton.onClick.RemoveAllListeners();
        UIHandlerSettingComponent.promptPanelDontSaveButton.onClick.RemoveAllListeners();
        UIHandlerSettingComponent.promptPanelCloseButton.onClick.RemoveAllListeners();
    }

    private void ShowPromptPanel()
    {
        UIElementHandler.ShowNonExclusivePanel(NonExclusivePanelType.SETTING_BACK_PROMPT_PANEL);
    }

    private void HidePromptPanel()
    {
        UIElementHandler.HideNonExclusivePanel(NonExclusivePanelType.SETTING_BACK_PROMPT_PANEL);
    }
    #endregion

    public bool GetAudioSettingValue(string audioSettingString)
    {
        return (bool) audioSettingRecord[audioSettingString];
    }

    public void SetAudioSettingValue(string audioSettingString, bool ON)
    {
         audioSettingRecord[audioSettingString] = ON;
    }

    #region AccelByte CloudSave Functions
    public void SaveUserAudioSettingRecord()
    {
        bool isSFXON = (bool) audioSettingRecord[LightFantasticConfig.AudioSettingType.SFX];
        bool isBGMON = (bool) audioSettingRecord[LightFantasticConfig.AudioSettingType.BGM];

        PlayerPrefs.SetInt(LightFantasticConfig.AudioSettingType.SFX, isSFXON ? 1 : 0);
        PlayerPrefs.SetInt(LightFantasticConfig.AudioSettingType.BGM, isBGMON ? 1 : 0);

        abCloudSave.SaveUserRecord(settingKey, audioSettingRecord, false, OnSaveUserAudioSettingRecord);
    }

    public void GetUserAudioSettingRecord()
    {
        abCloudSave.GetUserRecord(settingKey, OnGetUserAudioSettingRecord);
    }
    #endregion

    #region AccelByte CloudSave Callbacks
    private void OnSaveUserAudioSettingRecord(Result result)
    {
        if (result.IsError)
        {
            Debug.Log("Save user audio record from cloud save failed:" + result.Error.Message);
            Debug.Log("Save user audio record from cloud save Response Code: " + result.Error.Code);
            //Show Error Message
        }
        else
        {
            Debug.Log("Save user audio record from cloud save successful.");
        }
    }

    private void OnGetUserAudioSettingRecord(Result<UserRecord> result)
    {
        if (result.IsError)
        {
            audioSettingRecord = new Dictionary<string, object>
            {
                {LightFantasticConfig.AudioSettingType.BGM, true },
                {LightFantasticConfig.AudioSettingType.SFX, true }
            };

            AudioManager.Instance.ToggleBGMVolume(GetAudioSettingValue(LightFantasticConfig.AudioSettingType.BGM));
            AudioManager.Instance.ToggleSFXVolume(GetAudioSettingValue(LightFantasticConfig.AudioSettingType.SFX));

            Debug.Log("Get user audio record from cloud save failed:" + result.Error.Message);
            Debug.Log("Get user audio record from cloud save Response Code: " + result.Error.Code);
            //Show Error Message
        }
        else
        {
            audioSettingRecord = result.Value.value;
            AudioManager.Instance.ToggleBGMVolume(GetAudioSettingValue(LightFantasticConfig.AudioSettingType.BGM));
            AudioManager.Instance.ToggleSFXVolume(GetAudioSettingValue(LightFantasticConfig.AudioSettingType.SFX));

            Debug.Log("Get user audio record from cloud save successful.");
        }
    }
    #endregion
}
