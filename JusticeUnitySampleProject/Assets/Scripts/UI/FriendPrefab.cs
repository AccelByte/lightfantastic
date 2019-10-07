using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AccelByte.Core;

public class FriendPrefab : MonoBehaviour
{
    [SerializeField]
    private Text usernameText;
    [SerializeField]
    private Text lastSeenText;

    private string userID;

    // Start is called before the first frame update
    public void SetupFriendUI(string username, string lastSeen, string userId)
    {
        usernameText.text = username;
        lastSeenText.text = lastSeen;
        userID = userId;
    }

    public void InviteFriendToParty()
    {
        AccelByteManager.Instance.LobbyLogic.InviteToParty(userID, OnInviteParty);
    }

    private void OnInviteParty(Result result)
    {
        if (result.IsError)
        {
            Debug.Log("OnInviteParty failed:" + result.Error.Message);
            Debug.Log("OnInviteParty Response Code::" + result.Error.Code);
        }
        else
        {
            Debug.Log("OnInviteParty Succeded on Inviting player to party ID: " + userID);
        }
    }
}
