// Copyright (c) 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UITools;
using AccelByte.Api;
using AccelByte.Models;
using AccelByte.Core;

public class AccelBytePartyLogic : MonoBehaviour
{
    private UILobbyLogicComponent UIHandlerLobbyComponent;
    private AccelByteLobbyLogic lobbyLogic;
    private AccelByteManager accelByteManager;
    
    static bool isReceivedPartyInvitation = false;
    static bool isMemberJoinedParty = false;
    static bool isMemberLeftParty = false;
    static bool isMemberKickedParty = false;
    
    private PartyInvitation abPartyInvitation;
    private static bool isReadyToInviteToParty;
    private static bool isLocalPlayerInParty;
    private PartyInfo abPartyInfo;
    private IDictionary<string, PartyData> partyMemberList;
    private readonly string partyUserId = LightFantasticConfig.PARTY_CHAT;
    
    public bool GetIsLocalPlayerInParty(){return isLocalPlayerInParty;}
    public PartyInfo GetAbPartyInfo(){ return abPartyInfo; }
    public IDictionary<string, PartyData> GetPartyMemberList(){ return partyMemberList; }
    public string GetPartyUserId(){ return partyUserId; }
    
    public void SetIsLocalPlayerInParty(bool value){isLocalPlayerInParty = value;}
    public void SetAbPartyInfo(PartyInfo value){abPartyInfo = value;}

    private void Awake()
    {
        partyMemberList = new Dictionary<string, PartyData>();
    }

    public void Init(UILobbyLogicComponent uiLobbyLogicComponent, AccelByteLobbyLogic lobbyLogic)
    {
        UIHandlerLobbyComponent = uiLobbyLogicComponent;
        this.lobbyLogic = lobbyLogic;
        accelByteManager = lobbyLogic.GetComponent<AccelByteManager>();
    }

    public void AddEventListener()
    {
        UIHandlerLobbyComponent.acceptPartyInvitation.onClick.AddListener(OnAcceptPartyClicked);
        UIHandlerLobbyComponent.declinePartyInvitation.onClick.AddListener(OnDeclinePartyClicked);
        UIHandlerLobbyComponent.closePopupPartyButton.onClick.AddListener(OnPlayerPartyProfileClicked);
        UIHandlerLobbyComponent.closePopupPartyButton.onClick.AddListener(OnClosePartyInfoButtonClicked);
        UIHandlerLobbyComponent.leaderLeavePartyButton.onClick.AddListener(OnLeavePartyButtonClicked);
        UIHandlerLobbyComponent.localLeavePartyButton.onClick.AddListener(OnLeavePartyButtonClicked);
    }

    public void RemoveListener()
    {
        UIHandlerLobbyComponent.acceptPartyInvitation.onClick.RemoveListener(OnAcceptPartyClicked);
        UIHandlerLobbyComponent.declinePartyInvitation.onClick.RemoveListener(OnDeclinePartyClicked);
        UIHandlerLobbyComponent.closePopupPartyButton.onClick.RemoveListener(OnPlayerPartyProfileClicked);
        UIHandlerLobbyComponent.closePopupPartyButton.onClick.RemoveListener(OnClosePartyInfoButtonClicked);
        UIHandlerLobbyComponent.leaderLeavePartyButton.onClick.RemoveListener(OnLeavePartyButtonClicked);
        UIHandlerLobbyComponent.localLeavePartyButton.onClick.RemoveListener(OnLeavePartyButtonClicked);
    }
    
    public void SetupPartyUI()
    {
        ClearPartySlots();
        GetPartyInfo();
    }
    
    public void CleanupPartyUI()
    {
        HidePopUpPartyControl();   
    }
    
    public void SetupPartyCallbacks()
    {
        lobbyLogic.abLobby.InvitedToParty += result => OnInvitedToParty(result);
        lobbyLogic.abLobby.JoinedParty += result => OnMemberJoinedParty(result);
        lobbyLogic.abLobby.KickedFromParty += result => OnKickedFromParty(result);
        lobbyLogic.abLobby.LeaveFromParty += result => OnMemberLeftParty(result);
    }

    public void UnsubscribeAllCallbacks()
    {
        lobbyLogic.abLobby.InvitedToParty -= OnInvitedToParty;
        lobbyLogic.abLobby.JoinedParty -= OnMemberJoinedParty;
        lobbyLogic.abLobby.KickedFromParty -= OnKickedFromParty;
        lobbyLogic.abLobby.LeaveFromParty -= OnMemberLeftParty;
    }
    
    private void Update()
    {
        if (isReceivedPartyInvitation)
        {
            isReceivedPartyInvitation = false;
            AccelBytePlugin.GetUser().GetUserByUserId(abPartyInvitation.from, OnGetUserOnInvite);
        }
        if (isMemberJoinedParty)
        {
            isMemberJoinedParty = false;
            ClearPartySlots();
            GetPartyInfo();
        }
        if (isMemberLeftParty)
        {
            isMemberLeftParty = false;
            ClearPartySlots();
            GetPartyInfo();
        }
        if (isMemberKickedParty)
        {
            isMemberKickedParty = false;
            ClearPartySlots();
            GetPartyInfo();
        }
    }
    
    #region AccelByte Party Functions
    /// <summary>
    /// Create party lobby service
    /// </summary>
    /// <param name="callback"> callback result that includes party info </param>
    private void CreateParty(ResultCallback<PartyInfo> callback)
    {
        lobbyLogic.abLobby.CreateParty(callback);
    }

    /// <summary>
    /// Create party if not in a party yet
    /// then invite a friend to the party
    /// </summary>
    /// <param name="userId"> required userid from the friend list </param>
    public void CreateAndInvitePlayer(string userId)
    {
        if (!GetIsLocalPlayerInParty())
        {
            lobbyLogic.abLobby.CreateParty(OnPartyCreated);
        }
        else
        {
            isReadyToInviteToParty = true;
        }

        StartCoroutine(WaitForInviteToParty(userId));
    }

    IEnumerator WaitForInviteToParty(string userID)
    {
        bool isActive = true;
        while (isActive)
        {
            yield return new WaitForSecondsRealtime(1.0f);
            if (isReadyToInviteToParty)
            {
                InviteToParty(userID, OnInviteParty);
                isReadyToInviteToParty = isActive = false;
            }
        }
    }

    /// <summary>
    /// Invite a friend to a party
    /// </summary>
    /// <param name="id"> required userid to invite </param>
    /// <param name="callback"> callback result </param>
    public void InviteToParty(string id, ResultCallback callback)
    {
        string invitedPlayerId = id;

        lobbyLogic.abLobby.InviteToParty(invitedPlayerId, callback);
    }

    /// <summary>
    /// Kick party member from a party
    /// hide popup party UI
    /// </summary>
    /// <param name="id"> required userid to kick </param>
    public void KickPartyMember(string id)
    {
        lobbyLogic.abLobby.KickPartyMember(id, OnKickPartyMember);
        HidePopUpPartyControl();
    }

    /// <summary>
    /// Leave a party
    /// hide popup party UI clear UI chat
    /// </summary>
    private void LeaveParty()
    {
        lobbyLogic.abLobby.LeaveParty(OnLeaveParty);
        HidePopUpPartyControl();
        lobbyLogic.ClearActivePlayerChat();
        lobbyLogic.OpenEmptyChatBox();
    }

    /// <summary>
    /// Get party info that has party id
    /// Party info holds the party leader user id and the members user id
    /// </summary>
    private void GetPartyInfo()
    {
        lobbyLogic.abLobby.GetPartyInfo(OnGetPartyInfo);
    }

    /// <summary>
    /// Get party member info to get the display name
    /// </summary>
    /// <param name="friendId"></param>
    private void GetPartyMemberInfo(string friendId)
    {
        AccelBytePlugin.GetUser().GetUserByUserId(friendId, OnGetPartyMemberInfo);
    }

    private void ClearPartySlots()
    {
        // Clear the party slot buttons
        for (int i = 0; i < UIHandlerLobbyComponent.partyMemberButtons.Length; i++)
        {
            UIHandlerLobbyComponent.partyMemberButtons[i].GetComponent<PartyPrefab>().OnClearProfileButton();
            UIHandlerLobbyComponent.partyMemberButtons[i].GetComponent<Button>().onClick.AddListener(() => lobbyLogic.UIElementHandler.ShowExclusivePanel(ExclusivePanelType.FRIENDS));
        }

        partyMemberList.Clear();
    }

    private void RefreshPartySlots()
    {
        if (partyMemberList.Count > 0)
        {
            int j = 0;
            foreach (KeyValuePair<string, PartyData> member in partyMemberList)
            {
                Debug.Log("RefreshPartySlots Member names entered: " + member.Value.PlayerName);
                UIHandlerLobbyComponent.partyMemberButtons[j].GetComponent<Button>().onClick.RemoveAllListeners();
                UIHandlerLobbyComponent.partyMemberButtons[j].GetComponent<PartyPrefab>().OnClearProfileButton();
                UIHandlerLobbyComponent.partyMemberButtons[j].GetComponent<PartyPrefab>().SetupPlayerProfile(member.Value, abPartyInfo.leaderID);
                j++;
            }

            if (lobbyLogic.activePlayerChatUserId == partyUserId)
            {
                lobbyLogic.RefreshDisplayNamePartyChatListUI();
            }
        }

        lobbyLogic.friendsLogic.RefreshFriendsUI();
    }

    private void HidePopUpPartyControl()
    {
        UIHandlerLobbyComponent.popupPartyControl.gameObject.SetActive(false);
    }

    private void OnClosePartyInfoButtonClicked()
    {
        HidePopUpPartyControl();
    }

    /// <summary>
    /// Accept party invitation by calling join party
    /// require abPartyInvitation from callback party invitation event
    /// </summary>
    private void OnAcceptPartyClicked()
    {
        if (abPartyInvitation != null)
        {
            lobbyLogic.abLobby.JoinParty(abPartyInvitation.partyID, abPartyInvitation.invitationToken, OnJoinedParty);
        }
        else
        {
            Debug.Log("OnJoinPartyClicked Join party failed abPartyInvitation is null");
        }
    }

    private void OnDeclinePartyClicked()
    {
        Debug.Log("OnDeclinePartyClicked Join party failed");
    }

    private void OnPlayerPartyProfileClicked()
    {
        // If in a party then show the party control menu party leader can invite, kick and leave the party.
        // party member only able to leave the party.
        if (GetIsLocalPlayerInParty())
        {
            // Remove listerner before closing
            if (UIHandlerLobbyComponent.popupPartyControl.gameObject.activeSelf)
            {
                lobbyLogic.memberCommand.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
            }
            UIHandlerLobbyComponent.popupPartyControl.gameObject.SetActive(!UIHandlerLobbyComponent.popupPartyControl.gameObject.activeSelf);
        }
    }

    private List<PartyData> GetMemberPartyData()
    {
        List<PartyData> partyMemberData = new List<PartyData>();

        if (partyMemberList.Count > 0)
        {
            foreach (KeyValuePair<string, PartyData> member in partyMemberList)
            {
                partyMemberData.Add(member.Value);
            }
        }

        return partyMemberData;
    }

    private void OnLeavePartyButtonClicked()
    {
        if (accelByteManager.AuthLogic.GetUserData().userId == abPartyInfo.leaderID)
        {
            lobbyLogic.localLeaderCommand.gameObject.SetActive(false);
        }
        else
        {
            lobbyLogic.localmemberCommand.gameObject.SetActive(false);
        }
        UIHandlerLobbyComponent.popupPartyControl.gameObject.SetActive(false);
        LeaveParty();
    }
    #endregion

    #region AccelByte Party Callbacks
    /// <summary>
    /// Callback on create party and invite a friend
    /// Once the party created toogle the flag to trigger invite to party action
    /// </summary>
    /// <param name="result"> callback result that contains various party info </param>
    private void OnPartyCreated(Result<PartyInfo> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnPartyCreated failed:" + result.Error.Message);
            Debug.Log("OnPartyCreated Response Code::" + result.Error.Code);

        }
        else
        {
            Debug.Log("OnPartyCreated Party successfully created with party ID: " + result.Value.partyID);
            abPartyInfo = result.Value;
            SetIsLocalPlayerInParty(true);
            isReadyToInviteToParty = true;
        }
    }

    /// <summary>
    /// Callback on leave party
    /// clearing the UI related to party
    /// </summary>
    /// <param name="result"> Callback result </param>
    private void OnLeaveParty(Result result)
    {
        if (result.IsError)
        {
            Debug.Log("OnLeaveParty failed:" + result.Error.Message);
            Debug.Log("OnLeaveParty Response Code::" + result.Error.Code);
        }
        else
        {
            Debug.Log("OnLeaveParty Left a party");
            ClearPartySlots();
            SetIsLocalPlayerInParty(false);
            lobbyLogic.friendsLogic.RefreshFriendsUI();

            PopupManager.Instance.ShowPopupWarning("Leave The Party", "You are just left the party!", "OK");
        }
    }

    /// <summary>
    /// Callback from JoinedParty event lobby service
    /// triggered when user accept the party invitation
    /// fill in the UI with current party info
    /// </summary>
    /// <param name="result"> callback result that contains various party info </param>
    private void OnJoinedParty(Result<PartyInfo> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnJoinedParty failed:" + result.Error.Message);
            Debug.Log("OnJoinedParty Response Code::" + result.Error.Code);
        }
        else
        {
            // On joined should change the party slot with newer players info
            Debug.Log("OnJoinedParty Joined party with ID: " + result.Value.partyID + result.Value.leaderID);
            SetIsLocalPlayerInParty(true);
            abPartyInfo = result.Value;
            ClearPartySlots();
            GetPartyInfo();

            PopupManager.Instance.ShowPopupWarning("Join a Party", "You are just joined a party!", "OK");
        }
    }

    /// <summary>
    /// Callback on InviteParty action after a party created
    /// Notify the player if there is any error and when the invitation is successfuly sent
    /// </summary>
    /// <param name="result"> callback result </param>
    private void OnInviteParty(Result result)
    {
        if (result.IsError)
        {
            Debug.Log("OnInviteParty failed:" + result.Error.Message);
            Debug.Log("OnInviteParty Response Code::" + result.Error.Code);

            // If the player already in party then notify the user
            PopupManager.Instance.ShowPopupWarning("Invite to Party Failed", " " + result.Error.Message, "OK");
        }
        else
        {
            Debug.Log("OnInviteParty Succeded on Inviting player to party");
            PopupManager.Instance.ShowPopupWarning("Invite to Party Success", "Waiting for invitee acceptance", "OK");
        }
    }

    /// <summary>
    /// Callback from get user invite
    /// Show popup invitation on success
    /// </summary>
    /// <param name="result"> callback result contains the userdata we are using only the display name in this case </param>
    private void OnGetUserOnInvite(Result<UserData> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnGetUserOnInvite failed:" + result.Error.Message);
            Debug.Log("OnGetUserOnInvite Response Code::" + result.Error.Code);
        }
        else
        {
            Debug.Log("OnGetUserOnInvite UserData retrieved: " + result.Value.displayName);
            PopupManager.Instance.ShowPopup("Party Invitation", "Received Invitation From " + result.Value.displayName, "Accept", "Decline", OnAcceptPartyClicked, OnDeclinePartyClicked);
        }
    }

    /// <summary>
    /// Callback on GetPartyInfo
    /// Update each party member info on success
    /// </summary>
    /// <param name="result"> callback result that contains various party info </param>
    private void OnGetPartyInfo(Result<PartyInfo> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnGetPartyInfo failed:" + result.Error.Message);
            Debug.Log("OnGetPartyInfo Response Code::" + result.Error.Code);
            if (result.Error.Code == ErrorCode.PartyInfoSuccessGetUserPartyInfoEmpty)
            {
                SetIsLocalPlayerInParty(false);
                lobbyLogic.friendsLogic.RefreshFriendsUI();
                lobbyLogic.ClearActivePlayerChat();
                lobbyLogic.OpenEmptyChatBox();
            }
        }
        else
        {
            Debug.Log("OnGetPartyInfo Retrieved successfully");
            abPartyInfo = result.Value;

            for (int i = 0; i < result.Value.members.Length; i++)
            {
                Debug.Log("OnGetPartyInfo adding new party member: " + result.Value.members[i]);
                // Get member info
                GetPartyMemberInfo(result.Value.members[i]);
            }

            if (result.Value.members.Length == 1)
            {
                lobbyLogic.ClearActivePlayerChat();
                lobbyLogic.OpenEmptyChatBox();
            }

            SetIsLocalPlayerInParty(true);
        }
    }

    /// <summary>
    /// Callback on GetPartyMemberInfo
    /// Shorting party member info then update the UI
    /// </summary>
    /// <param name="result"> callback result contains the userdata we are using only the display name in this case </param>
    private void OnGetPartyMemberInfo(Result<UserData> result)
    {
        // add party member to party member list
        if (result.IsError)
        {
            Debug.Log("OnGetPartyMemberInfo failed:" + result.Error.Message);
            Debug.Log("OnGetPartyMemberInfo Response Code: " + result.Error.Code);
            //Show Error Message
        }
        else
        {
            Debug.Log("OnGetPartyMemberInfo sent successfully.");

            // TODO: store userdata locally
            // Add the member info to partymemberlist
            UserData data = AccelByteManager.Instance.AuthLogic.GetUserData();
            string ownId = data.userId;
            if (!partyMemberList.ContainsKey(result.Value.userId) && (result.Value.userId != ownId))
            {
                Debug.Log("OnGetPartyMemberInfo member with id: " + result.Value.userId + " DisplayName: " + result.Value.displayName);
                partyMemberList.Add(result.Value.userId, new PartyData(result.Value.userId, result.Value.displayName, result.Value.emailAddress));
            }
            
            RefreshPartySlots();
        }
    }

    /// <summary>
    /// Kick a party member from the party
    /// Bound to kick from party button on party control UI
    /// </summary>
    /// <param name="userId"></param>
    internal void OnKickFromPartyClicked(string userId)
    {
        Debug.Log("OnKickFromPartyClicked Usertokick userId");
        KickPartyMember(userId);
    }

    /// <summary>
    /// Callback from KickPartyMember
    /// Refresh the related UI with current party info
    /// </summary>
    /// <param name="result"></param>
    private void OnKickPartyMember(Result result)
    {
        if (result.IsError)
        {
            Debug.Log("OnKickPartyMember failed:" + result.Error.Message);
            Debug.Log("OnKickPartyMember Response Code::" + result.Error.Code);
        }
        else
        {
            Debug.Log("OnKickPartyMember Retrieved successfully");
            ClearPartySlots();
            GetPartyInfo();

            PopupManager.Instance.ShowPopupWarning("Kick a Party Member", "You are just kicked one of the party member!", "OK");
        }
    }
    #endregion

    #region AccelByte Party Notification Callbacks
    /// <summary>
    /// Callback InvitedToParty event
    /// Triggered if a friend invite the player to a party
    /// </summary>
    /// <param name="result"> callback result that holding party invitation token </param>
    private void OnInvitedToParty(Result<PartyInvitation> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnInvitedToParty failed:" + result.Error.Message);
            Debug.Log("OnInvitedToParty Response Code::" + result.Error.Code);
        }
        else
        {
            Debug.Log("OnInvitedToParty Received Invitation from " + result.Value.from);

            // Party invitation will be used for accepting the invitation
            abPartyInvitation = result.Value;
            isReceivedPartyInvitation = true;
        }
    }

    /// <summary>
    /// Callback from MemberJoinedParty Event
    /// Triggered if there is a new party member that accepting the invitation
    /// </summary>
    /// <param name="result"> callback result </param>
    private void OnMemberJoinedParty(Result<JoinNotification> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnMemberJoinedParty failed:" + result.Error.Message);
            Debug.Log("OnMemberJoinedParty Response Code::" + result.Error.Code);
        }
        else
        {
            Debug.Log("OnMemberJoinedParty Retrieved successfully");
            isMemberJoinedParty = true;

            // let the player knows that ther is a new party member joined in
            MainThreadTaskRunner.Instance.Run(delegate
            {
                PopupManager.Instance.ShowPopupWarning("A New Party Member", "A new member just joined the party!", "OK");
            });
        }
    }

    /// <summary>
    /// Callback from MemberLeftParty Event
    /// Triggered if a party member just left the party
    /// </summary>
    /// <param name="result"> callback result </param>
    private void OnMemberLeftParty(Result<LeaveNotification> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnMemberLeftParty failed:" + result.Error.Message);
            Debug.Log("OnMemberLeftParty Response Code::" + result.Error.Code);
        }
        else
        {
            Debug.Log("OnMemberLeftParty a party member has left the party" + result.Value.userID);
            isMemberLeftParty = true;
            MainThreadTaskRunner.Instance.Run(delegate
            {
                PopupManager.Instance.ShowPopupWarning("A Member Left The Party", "A member just left the party!", "OK");
            });
        }
    }

    /// <summary>
    /// Callback from KickedFromParty
    /// Triggered if the player kicked out from party by the party leader
    /// </summary>
    /// <param name="result"> callback result </param>
    private void OnKickedFromParty(Result<KickNotification> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnKickedFromParty failed:" + result.Error.Message);
            Debug.Log("OnKickedFromParty Response Code::" + result.Error.Code);
        }
        else
        {
            Debug.Log("OnKickedFromParty party with ID: " + result.Value.partyID);
            isMemberKickedParty = true;
            MainThreadTaskRunner.Instance.Run(delegate 
            {
                PopupManager.Instance.ShowPopupWarning("Kicked from The Party", "You are just kicked from the party!", "OK");
            });
        }
    }
    #endregion
}
