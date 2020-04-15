// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections;
using System.Collections.Generic;
using AccelByte.Core;
using UnityEngine;
using UnityEngine.UI;
#pragma warning disable 0649

public class SearchFriendPrefab : MonoBehaviour
{
    [SerializeField]
    private Text usernameText;
    [SerializeField]
    private Button addFriendButton;

    private string userId;
    public void SetupFriendPrefab(string username, string id)
    {
        usernameText.text = username;
        userId = id;
    }

    public void AddFriend()
    {
        AccelByteManager.Instance.LobbyLogic.SendFriendRequest(userId, SendFriendRequestCallback);
    }

    private void SendFriendRequestCallback(AccelByte.Core.Result result)
    {
        if (result.IsError)
        {
            Debug.Log("SendFriendRequest failed:" + result.Error.Message);
            Debug.Log("SendFriendRequest Response Code: " + result.Error.Code);
            //Show Error Message
        }
        else
        {
            Debug.Log("Request sent successfully.");
            addFriendButton.GetComponentInChildren<Text>().text = "Sent";
            addFriendButton.interactable = false;
        }
    }
}
