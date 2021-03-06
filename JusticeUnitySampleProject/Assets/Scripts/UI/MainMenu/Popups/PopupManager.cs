﻿// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PopupManager : MonoBehaviour
{
    private static PopupManager instance;
    public static PopupManager Instance { get { return instance; } }

    [Header("Popup Template")]
    [SerializeField]
    private GameObject prefabPopup;

    [SerializeField]
    private Transform popupRoot;

    private GameObject currentPopup;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }

        DontDestroyOnLoad(gameObject);
    }


    public void ShowPopup(string header, string desc, string btnText01, string btnText02, UnityAction btnCallback01 = null, UnityAction btnCallback02 = null)
    {
        Debug.Log("Popupmanager InitPopup Header: " + header);
        PopupPrefab popup = null;

        if (currentPopup == null)
        {
            Debug.Log("Popupmanager ShowPopup Header is NULL create a new one: " + header);
            currentPopup = Instantiate(prefabPopup, popupRoot);
        }
        
        popup = currentPopup.GetComponent<PopupPrefab>();
        if (popup != null)
        {
            popup.header = header;
            popup.description = desc;

            popup.primaryButtonText = btnText01;
            if (btnCallback01 != null)
            {
                Debug.Log("Popupmanager ShowPopup btnCallback01: " + header);
                popup.primaryButtonAction = btnCallback01;
            }
            popup.primaryButtonAction += PopupClosed;

            popup.secondaryButtonText = btnText02;
            if (btnCallback02 != null)
            {
                Debug.Log("Popupmanager ShowPopup btnCallback02: " + header);
                popup.secondaryButtonAction = btnCallback02;
            }
            popup.secondaryButtonAction += PopupClosed;

            popup.showExitButton = false;
            popup.SelectPopupType(E_PopupType.Popup_Default);

            if (currentPopup == null)
            {
                Debug.Log("Popupmanager ShowPopup currentPopup is NULL : " + header);
            }
            currentPopup.GetComponent<PopupPrefab>().Show();
        }
    }

    public void ShowPopupWarning(string header, string desc, string btnText01, UnityAction btnCallback01 = null)
    {
        Debug.Log("Popupmanager InitPopup Header: " + header);
        PopupPrefab popup = null;

        if (currentPopup == null)
        {
            Debug.Log("Popupmanager ShowPopupWarning Header is NULL create a new one: " + header);
            currentPopup = Instantiate(prefabPopup, popupRoot);
        }
        
        popup = currentPopup.GetComponent<PopupPrefab>();
        if (popup != null)
        {
            popup.header = header;
            popup.description = desc;

            popup.primarySingleButtonText = btnText01;

            if (btnCallback01 != null)
            {
                popup.primarySingleButtonAction = btnCallback01;
            }
            popup.primarySingleButtonAction += PopupClosed;
            popup.showExitButton = false;
            popup.SelectPopupType(E_PopupType.Popup_SingleButton);

            currentPopup.GetComponent<PopupPrefab>().Show();
        }
    }

    public void ShowPopup()
    {
        Debug.Log("Popupmanager ShowPopup");
        if (currentPopup != null)
        {
            currentPopup.GetComponent<PopupPrefab>().Show();
        }
    }

    public void HidePopup()
    {
        Debug.Log("Popupmanager HidePopup");
        if (currentPopup != null)
        {
            currentPopup.GetComponent<PopupPrefab>().Hide();
            ClearPopup();
        }
    }

    private void ClearPopup()
    {
        Debug.Log("Popupmanager ClearPopup");
        if (currentPopup != null)
        {
            PopupPrefab popup = currentPopup.GetComponent<PopupPrefab>();
            popup.header = "";
            popup.description = "";
            popup.primaryButtonText = "";
            popup.primaryButtonAction = null;
            popup.secondaryButtonText = "";
            popup.secondaryButtonAction = null;
            popup.primarySingleButtonText = "";
            popup.primarySingleButtonAction = null;
            popup.showExitButton = false;

            Destroy(currentPopup);
            currentPopup = null;
        }
    }

    private void PopupClosed()
    {
        Debug.Log("Popupmanager PopupClosed");
        ClearPopup();
    }
}
