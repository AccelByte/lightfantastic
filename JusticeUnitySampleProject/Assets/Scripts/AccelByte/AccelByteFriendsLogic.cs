// Copyright (c) 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections;
using System.Collections.Generic;
using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using UITools;
using UnityEngine;
using UnityEngine.UI;

public class AccelByteFriendsLogic : MonoBehaviour
{
    private UILobbyLogicComponent UIHandlerLobbyComponent;
    private AccelByteLobbyLogic lobbyLogic;
    private AccelByteManager accelByteManager;

    private static IDictionary<string, FriendData> friendList;
    private string lastFriendUserId;
    private FriendsStatusNotif friendsStatusNotif;
    static bool isLoadFriendDisplayName = false;
    static bool isFriendStatusChanged = false;
    static bool isFriendAcceptRequest = false;

    public IDictionary<string, FriendData> GetFriendList() { return friendList; }
    public void ClearFriendList(){ friendList.Clear(); }

    private void Awake()
    {
        friendList = new Dictionary<string, FriendData>();
    }
    
    public void Init(UILobbyLogicComponent uiLobbyLogicComponent, AccelByteLobbyLogic lobbyLogic)
    {
        UIHandlerLobbyComponent = uiLobbyLogicComponent;
        this.lobbyLogic = lobbyLogic;
        accelByteManager = lobbyLogic.GetComponent<AccelByteManager>();
    }
    
    public void AddEventListener()
    {
        UIHandlerLobbyComponent.friendsTabButton.onClick.AddListener(ListFriendsStatus);
        UIHandlerLobbyComponent.friendsTabButton.onClick.AddListener(LoadFriendsList);
        UIHandlerLobbyComponent.invitesTabButton.onClick.AddListener(GetIncomingFriendsRequest);
        UIHandlerLobbyComponent.invitesTabButton.onClick.AddListener(GetOutgoingFriendsRequest);
        UIHandlerLobbyComponent.invitesTabButton.onClick.AddListener(ClearFriendsUIPrefabs);
        UIHandlerLobbyComponent.searchFriendButton.onClick.AddListener(FindAFriendRequest);
        UIHandlerLobbyComponent.partyMember1stButton.onClick.AddListener(LoadFriendsList);
        UIHandlerLobbyComponent.partyMember1stButton.onClick.AddListener(() => lobbyLogic.UIElementHandler.ShowExclusivePanel(ExclusivePanelType.FRIENDS));
        UIHandlerLobbyComponent.partyMember2ndButton.onClick.AddListener(LoadFriendsList);
        UIHandlerLobbyComponent.partyMember2ndButton.onClick.AddListener(() => lobbyLogic.UIElementHandler.ShowExclusivePanel(ExclusivePanelType.FRIENDS));
        UIHandlerLobbyComponent.partyMember3rdButton.onClick.AddListener(LoadFriendsList);
        UIHandlerLobbyComponent.partyMember3rdButton.onClick.AddListener(() => lobbyLogic.UIElementHandler.ShowExclusivePanel(ExclusivePanelType.FRIENDS));
    }

    public void RemoveListener()
    {
        UIHandlerLobbyComponent.friendsTabButton.onClick.RemoveAllListeners();
        UIHandlerLobbyComponent.invitesTabButton.onClick.RemoveAllListeners();
        UIHandlerLobbyComponent.searchFriendButton.onClick.RemoveListener(FindAFriendRequest);
        UIHandlerLobbyComponent.partyMember1stButton.onClick.RemoveAllListeners();
        UIHandlerLobbyComponent.partyMember2ndButton.onClick.RemoveAllListeners();
        UIHandlerLobbyComponent.partyMember3rdButton.onClick.RemoveAllListeners();
    }

    public void SetupFriendCallbacks()
    {
        lobbyLogic.abLobby.OnIncomingFriendRequest += result => OnIncomingFriendsRequest(result);
        lobbyLogic.abLobby.FriendRequestAccepted += result => OnFriendRequestAccepted(result);
        lobbyLogic.abLobby.FriendsStatusChanged += result => OnFriendsStatusChanged(result);
    }

    public void UnsubscribeAllCallbacks()
    {
        lobbyLogic.abLobby.OnIncomingFriendRequest -= OnIncomingFriendsRequest;
        lobbyLogic.abLobby.FriendRequestAccepted -= OnFriendRequestAccepted;
        lobbyLogic.abLobby.FriendsStatusChanged -= OnFriendsStatusChanged;
    }

    void Update()
    {
        if (isLoadFriendDisplayName)
        {
            isLoadFriendDisplayName = false;
            foreach (var data in friendList.Keys)
            {
                GetFriendInfo(friendList[data].UserId, OnGetFriendInfoRequest);
            }
        }
        if (isFriendStatusChanged)
        {
            isFriendStatusChanged = false;
            string friendName = friendList[friendsStatusNotif.userID].DisplayName;
            friendList[friendsStatusNotif.userID] = new FriendData(friendsStatusNotif.userID, friendName, friendsStatusNotif.lastSeenAt, friendsStatusNotif.availability);
            RefreshFriendsUI();
            if (lobbyLogic.chatLogic.activePlayerChatUserId == lobbyLogic.partyLogic.GetPartyUserId())
                lobbyLogic.chatLogic.RefreshDisplayNamePartyChatListUI();
            else
                lobbyLogic.chatLogic.RefreshDisplayNamePrivateChatListUI();
        }
        if (isFriendAcceptRequest)
        {
            isFriendAcceptRequest = false;
            LoadFriendsList();
        }
    }
    
    #region AccelByte Friend Functions
    /// <summary>
    /// Load friendlist from lobby social service
    /// </summary>
    public void LoadFriendsList()
    {
        lobbyLogic.abLobby.LoadFriendsList(OnLoadFriendsListRequest);
        UIHandlerLobbyComponent.friendsTabButton.interactable = false;
        UIHandlerLobbyComponent.invitesTabButton.interactable = true;
    }

    /// <summary>
    /// Getting the list of friend status
    /// Returns availability, last seen at and their activity
    /// </summary>
    private void ListFriendsStatus()
    {
        lobbyLogic.abLobby.ListFriendsStatus(OnListFriendsStatusRequest);
    }

    /// <summary>
    /// Getting the friend public userdata from lobby service
    /// </summary>
    /// <param name="friendId"> required friend id </param>
    /// <param name="callback"> result callback that contains public user data </param>
    public void GetFriendInfo(string friendId, ResultCallback<UserData> callback)
    {
        AccelBytePlugin.GetUser().GetUserByUserId(friendId, callback);
    }

    /// <summary>
    /// Search a friend using their email or their display name
    /// </summary>
    private void FindAFriendRequest()
    {
        AccelBytePlugin.GetUser().SearchUsers(UIHandlerLobbyComponent.emailToFind.text, OnFindAFriendRequest);
    }

    /// <summary>
    /// Send a friend request
    /// </summary>
    /// <param name="friendId"> required friend id </param>
    /// <param name="callback"> result callback to be bound to a button onclick event </param>
    public void SendFriendRequest(string friendId, ResultCallback callback)
    {
        lobbyLogic.abLobby.RequestFriend(friendId, callback);
    }

    /// <summary>
    /// Accept a friend request 
    /// </summary>
    /// <param name="friendId"> required friend id </param>
    /// <param name="callback"> result callback to be bound to a button onclick event </param>
    public void AcceptFriendRequest(string friendId, ResultCallback callback)
    {
        lobbyLogic.abLobby.AcceptFriend(friendId, callback);
    }

    /// <summary>
    /// Decline a friend request
    /// </summary>
    /// <param name="friendId"> required friend id </param>
    /// <param name="callback"> result callback to be bound to a button onclick event </param>
    public void DeclineFriendRequest(string friendId, ResultCallback callback)
    {
        lobbyLogic.abLobby.RejectFriend(friendId, callback);
    }

    /// <summary>
    /// Getting the incoming friend request(s)
    /// </summary>
    private void GetIncomingFriendsRequest()
    {
        lobbyLogic.abLobby.ListIncomingFriends(OnGetIncomingFriendsRequest);
    }

    /// <summary>
    /// Getting sent friend request(s)
    /// </summary>
    private void GetOutgoingFriendsRequest()
    {
        lobbyLogic.abLobby.ListOutgoingFriends(OnGetOutgoingFriendsRequest);
    }

    #endregion

    #region AccelByte Friend Callbacks
    /// <summary>
    /// Callback from load friend list
    /// on success, need to getting the display name of each friend
    /// that will be handled by OnGetFriendInfoRequest on update
    /// </summary>
    /// <param name="result"></param>
    private void OnLoadFriendsListRequest(Result<Friends> result)
    {
        if (result.IsError)
        {
            Debug.Log("LoadFriends failed:" + result.Error.Message);
            Debug.Log("LoadFriends Response Code: " + result.Error.Code);
            //Show Error Message
        }
        else
        {
            Debug.Log("Loaded friends list successfully.");

            for (int i = 0; i < result.Value.friendsId.Length; i++)
            {
                Debug.Log(result.Value.friendsId[i]);
                if (!friendList.ContainsKey(result.Value.friendsId[i]))
                {
                    friendList.Add(result.Value.friendsId[i], new FriendData(result.Value.friendsId[i], "Loading...", new DateTime(2000, 12, 30), "0"));
                    lastFriendUserId = result.Value.friendsId[i];
                }
            }
            isLoadFriendDisplayName = true;
        }
    }

    /// <summary>
    /// Callback from ListFriendStatus
    /// Update the displayname last seen at and availability and update the friend list UI
    /// </summary>
    /// <param name="result"></param>
    private void OnListFriendsStatusRequest(Result<FriendsStatus> result)
    {
        if (result.IsError)
        {
            Debug.Log("ListFriendsStatusRequest failed:" + result.Error.Message);
            Debug.Log("ListFriendsStatusRequest Response Code: " + result.Error.Code);
            //Show Error Message
        }
        else
        {
            if (friendList.Count > 0)
            {
                Debug.Log("ListFriendsStatusRequest sent successfully.");

                for (int i = 0; i < result.Value.friendsId.Length; i++)
                {
                    string friendUserId = result.Value.friendsId[i];
                    friendList[friendUserId] = new FriendData(friendUserId, friendList[friendUserId].DisplayName, result.Value.lastSeenAt[i], result.Value.availability[i]);
                }
                RefreshFriendsUI();

                // Update chat list UI
                if (lobbyLogic.chatLogic.activePlayerChatUserId == lobbyLogic.partyLogic.GetPartyUserId())
                {
                    lobbyLogic.chatLogic.RefreshDisplayNamePartyChatListUI();
                }
                else
                {
                    lobbyLogic.chatLogic.RefreshDisplayNamePrivateChatListUI();
                }
            }
        }
    }

    internal void RefreshFriendsUI()
    {
        ClearFriendsUIPrefabs();
        foreach (KeyValuePair<string, FriendData> friend in friendList)
        {
            int lastSeen = System.DateTimeOffset.Now.Subtract(friend.Value.LastSeen).Days;
            string timeInfo = " days ago";
            if (lastSeen == 0)
            {
                lastSeen = System.DateTimeOffset.Now.Subtract(friend.Value.LastSeen).Hours;
                timeInfo = " hours ago";
            }
            if (lastSeen == 0)
            {
                lastSeen = System.DateTimeOffset.Now.Subtract(friend.Value.LastSeen).Minutes;
                timeInfo = " minutes ago";
            }

            FriendPrefab friendPrefab = Instantiate(UIHandlerLobbyComponent.friendPrefab, Vector3.zero, Quaternion.identity).GetComponent<FriendPrefab>();
            friendPrefab.transform.SetParent(UIHandlerLobbyComponent.friendScrollContent, false);

            if (friend.Value.IsOnline == "0")
            {
                friendPrefab.GetComponent<FriendPrefab>().SetupFriendUI(friend.Value.DisplayName, lastSeen.ToString() + timeInfo, friend.Value.UserId);
                friendPrefab.GetComponent<FriendPrefab>().SetInviterPartyStatus(lobbyLogic.partyLogic.GetIsLocalPlayerInParty());
            }
            else
            {
                bool isNotInParty = true;
                if (lobbyLogic.partyLogic.GetPartyMemberList().ContainsKey(friend.Value.UserId))
                {
                    isNotInParty = false;
                }
                friendPrefab.GetComponent<FriendPrefab>().SetupFriendUI(friend.Value.DisplayName, "Online", friend.Value.UserId, isNotInParty);
                friendPrefab.GetComponent<FriendPrefab>().SetInviterPartyStatus(lobbyLogic.partyLogic.GetIsLocalPlayerInParty());
            }
            UIHandlerLobbyComponent.friendScrollView.Rebuild(CanvasUpdate.Layout);
        }
    }

    private void ClearFriendsUIPrefabs()
    {
        if (UIHandlerLobbyComponent.friendScrollContent.childCount > 0)
        {
            for (int i = 0; i < UIHandlerLobbyComponent.friendScrollContent.childCount; i++)
            {
                Destroy(UIHandlerLobbyComponent.friendScrollContent.GetChild(i).gameObject);
            }
        }
    }

    /// <summary>
    /// Callback from getFriendInfo request
    /// Update the friend list data with display name
    /// </summary>
    /// <param name="result"> callback result contains the userdata we are using only the display name in this case </param>
    private void OnGetFriendInfoRequest(Result<UserData> result)
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
            string friendUserId = result.Value.userId;
            friendList[friendUserId] = new FriendData(friendUserId, result.Value.displayName, friendList[friendUserId].LastSeen, friendList[friendUserId].IsOnline);

            // if this is the last friend id, then continue to get friend status
            if (lastFriendUserId == friendUserId)
            {
                ListFriendsStatus();
            }
        }
    }

    /// <summary>
    /// Callback from GetIncomingFriendRequest
    /// Instantiate invitation prefab and add to the scroll list
    /// </summary>
    /// <param name="result"> result callback that has friend id on it </param>
    private void OnGetIncomingFriendsRequest(Result<Friends> result)
    {
        if (result.IsError)
        {
            Debug.Log("GetIncomingFriendsRequest failed:" + result.Error.Message);
            Debug.Log("GetIncomingFriendsRequest Response Code: " + result.Error.Code);
            //Show Error Message
        }
        else
        {
            Debug.Log("Loaded incoming friends list successfully.");

            foreach (string friendId in result.Value.friendsId)
            {
                Debug.Log("Incoming Friend Id: " + friendId);

                Transform friend = Instantiate(UIHandlerLobbyComponent.friendInvitePrefab, Vector3.zero, Quaternion.identity);
                friend.transform.SetParent(UIHandlerLobbyComponent.friendScrollContent, false);

                friend.GetComponent<InvitationPrefab>().SetupInvitationPrefab(friendId);
            }
        }
        UIHandlerLobbyComponent.friendScrollView.Rebuild(CanvasUpdate.Layout);
    }

    /// <summary>
    /// Callback from GetIncomingFriendRequest
    /// Instantiate invitation prefab and add to the scroll list
    /// </summary>
    /// <param name="result"></param>
    private void OnGetOutgoingFriendsRequest(Result<Friends> result)
    {
        if (result.IsError)
        {
            Debug.Log("GetGetOutgoingFriendsRequest failed:" + result.Error.Message);
            Debug.Log("GetGetOutgoingFriendsRequest Response Code: " + result.Error.Code);
            //Show Error Message
        }
        else
        {
            Debug.Log("Loaded outgoing friends list successfully.");

            foreach (string friendId in result.Value.friendsId)
            {
                Debug.Log("Outgoing Friend Id: " + friendId);
                //Get person's name, picture, etc

                Transform friend = Instantiate(UIHandlerLobbyComponent.sentInvitePrefab, Vector3.zero, Quaternion.identity);
                friend.transform.SetParent(UIHandlerLobbyComponent.friendScrollContent, false);

                friend.GetComponent<InvitationPrefab>().SetupInvitationPrefab(friendId);
            }
        }

        UIHandlerLobbyComponent.friendScrollView.Rebuild(CanvasUpdate.Layout);
    }

    /// <summary>
    /// Callback from FindAFriend request
    /// </summary>
    /// <param name="result"> callback result that returns paging of user search result </param>
    private void OnFindAFriendRequest(Result<PagedPublicUsersInfo> result)
    {
        for (int i = 0; i < UIHandlerLobbyComponent.friendSearchScrollContent.childCount; i++)
        {
            Destroy(UIHandlerLobbyComponent.friendSearchScrollContent.GetChild(i).gameObject);
        }

        if (result.IsError)
        {
            Debug.Log("GetUserData failed:" + result.Error.Message);
            Debug.Log("GetUserData Response Code: " + result.Error.Code);
        }
        else
        {
            if (result.Value.data.Length > 0)
            {
                Debug.Log("Search Results:");
                Debug.Log("Display Name: " + result.Value.data[0].displayName);
                Debug.Log("UserID: " + result.Value.data[0].userId);

                SearchFriendPrefab friend = Instantiate(UIHandlerLobbyComponent.friendSearchPrefab, Vector3.zero, Quaternion.identity).GetComponent<SearchFriendPrefab>();
                friend.transform.SetParent(UIHandlerLobbyComponent.friendSearchScrollContent, false);
                // Get only the first user
                // TODO: display all the user search result on the list
                friend.GetComponent<SearchFriendPrefab>().SetupFriendPrefab(result.Value.data[0].displayName, result.Value.data[0].userId);
                UIHandlerLobbyComponent.friendSearchScrollView.Rebuild(CanvasUpdate.Layout);
            }
            else
            {
                Debug.Log("Search Results: Not Found!");
            }
        }
    }

    #endregion

    #region AccelByte Friend Notification Callbacks
    //This is for updating your friends list with up to date player information
    /// <summary>
    /// Callback from FriendsStatusChanged event
    /// If the friend status changed this event  will be triggered
    /// The friend list also need to be updated
    /// </summary>
    /// <param name="result"> result callback that contains friend id </param>
    private void OnFriendsStatusChanged(Result<FriendsStatusNotif> result)
    {
        friendsStatusNotif = result.Value;
        isFriendStatusChanged = true;
    }

    /// <summary>
    /// Callback from FriendRequestAccepted event
    /// triggered when the invitation is accepted by your friend
    /// The friend list also need to be updated
    /// </summary>
    /// <param name="result"> result callback that contains friend id </param>
    private void OnFriendRequestAccepted(Result<Friend> result)
    {
        isFriendAcceptRequest = true;
    }

    /// <summary>
    /// Callback from OnIncomingFriendRequest event
    /// Get displayname of the friend right after
    /// </summary>
    /// <param name="result"> result callback that contains friend id </param>
    private void OnIncomingFriendsRequest(Result<Friend> result)
    {
        GetFriendInfo(result.Value.friendId, OnGetFriendInfoIncomingFriendRequest);
    }

    /// <summary>
    /// Callback from GetFriendInfoIncomingFriendRequest
    /// update the invitation with a display name of the inviter
    /// </summary>
    /// <param name="result"> result callback that contains friend id </param>
    private void OnGetFriendInfoIncomingFriendRequest(Result<UserData> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnGetFriendInfoRequest failed:" + result.Error.Message);
            Debug.Log("OnGetFriendInfoRequest Response Code: " + result.Error.Code);
            //Show Error Message
        }
        else
        {
            if (!friendList.ContainsKey(result.Value.userId))
            {
                UIHandlerLobbyComponent.incomingFriendNotificationTitle.text = result.Value.displayName + " sent you a friend request!";
                UIHandlerLobbyComponent.invite.SetupInvitationPrefab(result.Value.userId);
                lobbyLogic.UIElementHandler.ShowNotification(lobbyLogic.UIElementHandler.inviteNotification);
            }
        }
    }
    #endregion
}
