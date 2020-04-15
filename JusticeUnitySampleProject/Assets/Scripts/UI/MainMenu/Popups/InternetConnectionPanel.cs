// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections;
using AccelByte.Api;
using UnityEngine;

public class InternetConnectionPanel : MonoBehaviour
{
    [SerializeField] private CanvasGroup internetConnectionPanelCanvasGroup;
    
    public void RetryConnection()
    {
        StartCoroutine(pingGoogle(internetOk =>
        {
            InternetConnectionPanelSetVisibility(!internetOk);
        }));
    }

    private IEnumerator pingGoogle(Action<bool> onPingIsSuccess)
    {
        WWW www = new WWW(AccelByteSettings.BaseUrl + "/version");
        yield return www;
        if (www.error != null) {
            onPingIsSuccess(false);
        } else {
            onPingIsSuccess(true);
        }
    }
    
    public void Exit() { Application.Quit(0);}
    
    void Start()
    {
        InternetConnectionPanelSetVisibility(false);
        RetryConnection();
    }

    private void InternetConnectionPanelSetVisibility(bool visible)
    {
        internetConnectionPanelCanvasGroup.alpha = visible ? 1 : 0;
        internetConnectionPanelCanvasGroup.blocksRaycasts = visible;
        internetConnectionPanelCanvasGroup.interactable = visible;
    }
}
