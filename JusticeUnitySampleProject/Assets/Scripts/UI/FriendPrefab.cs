using AccelByte.Core;
using AccelByte.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AccelByte.Core;

public class FriendPrefab : MonoBehaviour
{
    private string userId;
    [SerializeField]
    private Text usernameText;
    [SerializeField]
    private Text lastSeenText;

    private string userID;
    private bool isInviterHasParty;
    private bool isUserOnline;

    private void Awake()
    {
        userID = "";
    }

    // Start is called before the first frame update
    public void SetupFriendUI(string username, string lastSeen, string userId)
    {
        usernameText.text = username;
        lastSeenText.text = lastSeen;
        userID = userId;
    }

    public void InviteFriendToParty()
    {
        //if (!isInviterHasParty)
        //{
        //    AccelByteManager.Instance.LobbyLogic.CreateParty();
        //    StartCoroutine(InviteToPartyDelayed());
        //}
        //else
        //{
            AccelByteManager.Instance.LobbyLogic.InviteToParty(userID, OnInviteParty);
        //}
    }

    public void SetFriendInfo(bool isOnline, bool inParty)
    {
        isInviterHasParty = inParty;
        isUserOnline = isOnline;
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

    IEnumerator InviteToPartyDelayed()
    {
        yield return new WaitForSeconds(2.0f);
        AccelByteManager.Instance.LobbyLogic.InviteToParty(userID, OnInviteParty);
    }
}
