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
    public void SetupInvitationPrefab(string username, string id)
    {
        usernameText.text = username;
        userId = id;
    }

    public void AcceptInvite()
    {
        AccelByteManager.Instance.LobbyLogic.AcceptFriendRequest(userId, OnAcceptFriendRequest);
    }
    public void DeclineInvite()
    {
        AccelByteManager.Instance.LobbyLogic.AcceptFriendRequest(userId, OnAcceptFriendRequest);
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
