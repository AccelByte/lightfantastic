using System;
using System.Collections;
using System.Collections.Generic;
using AccelByte.Core;
using UnityEngine;
using UnityEngine.UI;

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
