// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

#pragma warning disable 0649

using System;
using System.Collections;
using System.Collections.Generic;
using AccelByte.Core;
using UnityEngine;
using UnityEngine.UI;

public class InvitationPrefab : MonoBehaviour
{
    [SerializeField]
    private Text usernameText;
    [SerializeField]
    private Button acceptInviteButton;
    [SerializeField]
    private Button declineInviteButton;

    private string userId;
    public void SetupInvitationPrefab(string id)
    {
        AccelByteManager.Instance.LobbyLogic.GetFriendInfo(id, OnGetFriendInfoRequest);
        
        userId = id;
    }

    private void OnGetFriendInfoRequest(Result<AccelByte.Models.UserData> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnGetFriendInfoRequest failed:" + result.Error.Message);
            Debug.Log("OnGetFriendInfoRequest Response Code: " + result.Error.Code);
            //Show Error Message
        }
        else
        {
            Debug.Log("OnGetFriendInfoRequest sent successfully.");
            if (!usernameText.IsDestroyed())
            {
                usernameText.text = result.Value.displayName;
            }
        }
    }

    public void AcceptInvite()
    {
        AccelByteManager.Instance.LobbyLogic.AcceptFriendRequest(userId, OnAcceptFriendRequest);
    }
    public void DeclineInvite()
    {
        AccelByteManager.Instance.LobbyLogic.DeclineFriendRequest(userId, OnAcceptFriendRequest);
    }

    private void OnAcceptFriendRequest(Result result)
    {
        if (result.IsError)
        {
            Debug.Log("AcceptFriendRequest failed:" + result.Error.Message);
            Debug.Log("AcceptFriendRequest Response Code: " + result.Error.Code);
            //Show Error Message
        }
        else
        {
            Debug.Log("AcceptFriendRequest sent successfully.");
            AccelByteManager.Instance.LobbyLogic.LoadFriendsList();
            Destroy(this.gameObject);
        }
    }
    private void OnDeclineFriendRequest(Result result)
    {
        if (result.IsError)
        {
            Debug.Log("AcceptFriendRequest failed:" + result.Error.Message);
            Debug.Log("AcceptFriendRequest Response Code: " + result.Error.Code);
            //Show Error Message
        }
        else
        {
            Debug.Log("AcceptFriendRequest sent successfully.");
            Destroy(this.gameObject);
        }
    }

}
