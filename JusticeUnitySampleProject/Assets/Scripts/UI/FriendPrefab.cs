#pragma warning disable 0105
#pragma warning disable 0649

using AccelByte.Core;
using AccelByte.Models;
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
    [SerializeField]
    private Transform InviteButton;

    private string userID;
    private bool hasParty;

    private void Awake()
    {
        userID = "";
    }

    // Start is called before the first frame update
    public void SetupFriendUI(string username, string lastSeen, string userId, bool isOnline = false)
    {
        usernameText.text = username;
        lastSeenText.text = lastSeen;
        userID = userId;

        SetInviteButtonVisibility(isOnline);
    }

    public void InviteFriendToParty()
    {
        // Create party first if not in party yet
        // If not yet have a party, create a party then to call invitetoparty
        if (!hasParty)
        {
            AccelByteManager.Instance.LobbyLogic.CreateAndInvitePlayer(userID);
        }
        else
        {
            // Else directly invite to party
            AccelByteManager.Instance.LobbyLogic.InviteToParty(userID, OnInviteParty);
        }
    }

    public void SetInviterPartyStatus(bool inParty)
    {
        hasParty = inParty;
        Debug.Log("SetInviterPartyStatus has party false");
    }

    private void SetInviteButtonVisibility(bool visible)
    {
        InviteButton.gameObject.SetActive(visible);
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
