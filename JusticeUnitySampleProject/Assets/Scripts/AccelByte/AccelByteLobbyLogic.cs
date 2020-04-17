// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AccelByte.Api;
using AccelByte.Models;
using AccelByte.Core;
using UITools;
using System;
using HybridWebSocket;
using UnityEngine.SceneManagement;

public class AccelByteLobbyLogic : MonoBehaviour
{
    public Lobby abLobby;
    
    private AccelByteMatchmakingLogic matchmakingLogic;
    //TODO another child/sub lobby logic

    private GameObject UIHandler;
    private UILobbyLogicComponent UIHandlerLobbyComponent;
    private UIElementHandler UIElementHandler;

    private static IDictionary<string, FriendData> friendList;
    private static IDictionary<string, PartyData> partyMemberList;
    private static IDictionary<string, ChatData> chatBoxList;
    private PartyInvitation abPartyInvitation;
    public PartyInfo abPartyInfo;
    private string lastFriendUserId;
    private string activePlayerChatUserId;
    private static string partyUserId = LightFantasticConfig.PARTY_CHAT;

    private static LightFantasticConfig.GAME_MODES gameModeEnum     = LightFantasticConfig.GAME_MODES.unitytest;
    private string gameMode = gameModeEnum.ToString();

    private static bool isLocalPlayerInParty;
    public void SetIsLocalPlayerInParty(bool value){isLocalPlayerInParty = value;}
    public bool GetIsLocalPlayerInParty(){return isLocalPlayerInParty;}
    
    static bool isReadyToInviteToParty;
    private List<string> chatList;
    private ChatMesssage receivedPrivateMessage;
    private ChatMesssage receivedPartyMessage;
    private FriendsStatusNotif friendsStatusNotif;
    private AccelByteManager accelByteManager;
    private bool isActionPhaseOver = false;

    static bool isReceivedPrivateMessage = false;
    static bool isReceivedPartyMessage = false;
    static bool isRecevedPartyInvitation = false;
    static bool isMemberJoinedParty = false;
    static bool isMemberLeftParty = false;
    static bool isMemberKickedParty = false;
    static bool isLoadFriendDisplayName = false;
    static bool isFriendStatusChanged = false;
    static bool isFriendAcceptRequest = false;
    #region UI Fields
    private Transform localLeaderCommand;
    private Transform localmemberCommand;
    private Transform memberCommand;
    private Transform PlayerNameText;
    private Transform playerEmailText;
    #endregion

    public delegate void LobbyMatchOverEvent();
    public event LobbyMatchOverEvent onMatchOver;

    private void Awake()
    {
        accelByteManager = gameObject.GetComponent<AccelByteManager>();
        //Initialize our Lobby object
        abLobby = AccelBytePlugin.GetLobby();
        friendList = new Dictionary<string, FriendData>();
        partyMemberList = new Dictionary<string, PartyData>();
        chatBoxList = new Dictionary<string, ChatData>();
        chatList = new List<string>();

        matchmakingLogic = gameObject.GetComponent<AccelByteMatchmakingLogic>();
    }

    private void Update()
    {
        if (isReceivedPrivateMessage)
        {
            isReceivedPrivateMessage = false;
            if (!chatBoxList.ContainsKey(receivedPrivateMessage.from))
            {
                chatBoxList.Add(receivedPrivateMessage.from, new ChatData(receivedPrivateMessage.from, new List<string>(), new List<string>()));
            }
            chatBoxList[receivedPrivateMessage.from].sender.Add(receivedPrivateMessage.from);
            chatBoxList[receivedPrivateMessage.from].message.Add(receivedPrivateMessage.payload);

            if (receivedPrivateMessage.from == activePlayerChatUserId)
            {
                RefreshChatBoxUI();
            }
        }
        if (isReceivedPartyMessage)
        {
            isReceivedPartyMessage = false;
            
            if (!chatBoxList.ContainsKey(partyUserId))
            {
                chatBoxList.Add(partyUserId, new ChatData(partyUserId, new List<string>(), new List<string>()));
            }
            chatBoxList[partyUserId].sender.Add(receivedPartyMessage.from);
            chatBoxList[partyUserId].message.Add(receivedPartyMessage.payload);

            if (partyUserId == activePlayerChatUserId)
            {
                RefreshChatBoxUI();
            }
        }
        if (isRecevedPartyInvitation)
        {
            isRecevedPartyInvitation = false;
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
            if (activePlayerChatUserId == partyUserId)
                RefreshDisplayNamePartyChatListUI();
            else
                RefreshDisplayNamePrivateChatListUI();
        }
        if (isFriendAcceptRequest)
        {
            isFriendAcceptRequest = false;
            LoadFriendsList();
        }
    }

    private void OnApplicationQuit()
    {
        OnExitFromLobby(null);
    }
    
    /// <summary>
    /// On quit:
    /// - set user status to offline
    /// - leave party
    /// - cancel matchmaking
    /// - disconnect 
    /// </summary>
    public void OnExitFromLobby(Action onComplete)
    {
        Debug.Log("Application ending after " + Time.time + " seconds");
        if (abLobby.IsConnected)
        {
            abLobby.CancelMatchmaking(gameMode, cancelResult =>
            {
                abLobby.LeaveParty(leaveResult =>
                {
                    abLobby.SetUserStatus(UserStatus.Offline, "Offline", setStatusResult =>
                    {
                        onComplete.Invoke();
                    });
                });
            });
        }
    }

    #region UI Listeners
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (UIHandler != null)
        {
            RemoveListeners();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshUIHandler();
    }

    public void RefreshUIHandler()
    {
        UIHandler = GameObject.FindGameObjectWithTag("UIHandler");
        if (UIHandler == null)
        {
            return;
        }
        UIHandlerLobbyComponent = UIHandler.GetComponent<UILobbyLogicComponent>();
        UIElementHandler = UIHandler.GetComponent<UIElementHandler>();

        matchmakingLogic.Init(UIHandlerLobbyComponent, this);
        
        AddEventListeners();

        SetupPopupPartyControl();
        
        if (isActionPhaseOver)
        {
            // move to main menu screen
            // TODO: after from action pahse all the main menu stuff has tobe refreshed (player profile and statistic)
            
            UIElementHandler.ShowExclusivePanel(ExclusivePanelType.MAIN_MENU);
            UIElementHandler.ShowNonExclusivePanel(NonExclusivePanelType.PARENT_OF_OVERLAY_PANELS);
            
            SetupLobbyUI();
            isActionPhaseOver = false;
        }
    }

    void AddEventListeners()
    {
        // Bind Buttons
        UIHandlerLobbyComponent.logoutButton.onClick.AddListener(OnLogoutButtonClicked);
        
        UIHandlerLobbyComponent.friendsTabButton.onClick.AddListener(ListFriendsStatus);
        UIHandlerLobbyComponent.friendsTabButton.onClick.AddListener(LoadFriendsList);
        UIHandlerLobbyComponent.invitesTabButton.onClick.AddListener(GetIncomingFriendsRequest);
        UIHandlerLobbyComponent.invitesTabButton.onClick.AddListener(GetOutgoingFriendsRequest);
        UIHandlerLobbyComponent.invitesTabButton.onClick.AddListener(ClearFriendsUIPrefabs);
        UIHandlerLobbyComponent.searchFriendButton.onClick.AddListener(FindAFriendRequest);
        UIHandlerLobbyComponent.localPlayerButton.onClick.AddListener(OnLocalPlayerProfileButtonClicked);
        UIHandlerLobbyComponent.partyMember1stButton.onClick.AddListener(LoadFriendsList);
        UIHandlerLobbyComponent.partyMember1stButton.onClick.AddListener(() => UIElementHandler.ShowExclusivePanel(ExclusivePanelType.FRIENDS));
        UIHandlerLobbyComponent.partyMember2ndButton.onClick.AddListener(LoadFriendsList);
        UIHandlerLobbyComponent.partyMember2ndButton.onClick.AddListener(() => UIElementHandler.ShowExclusivePanel(ExclusivePanelType.FRIENDS));
        UIHandlerLobbyComponent.partyMember3rdButton.onClick.AddListener(LoadFriendsList);
        UIHandlerLobbyComponent.partyMember3rdButton.onClick.AddListener(() => UIElementHandler.ShowExclusivePanel(ExclusivePanelType.FRIENDS));
        UIHandlerLobbyComponent.acceptPartyInvitation.onClick.AddListener(OnAcceptPartyClicked);
        UIHandlerLobbyComponent.declinePartyInvitation.onClick.AddListener(OnDeclinePartyClicked);
        UIHandlerLobbyComponent.closePopupPartyButton.onClick.AddListener(OnPlayerPartyProfileClicked);
        UIHandlerLobbyComponent.closePopupPartyButton.onClick.AddListener(OnClosePartyInfoButtonClicked);
        UIHandlerLobbyComponent.enterToChatButton.onClick.AddListener(OpenEmptyChatBox);
        UIHandlerLobbyComponent.sendMessageButton.onClick.AddListener(SendChatMessage);
        UIHandlerLobbyComponent.backChatButton.onClick.AddListener(ClearActivePlayerChat);
        UIHandlerLobbyComponent.partyChatButton.onClick.AddListener(OpenPartyChatBox);
        UIHandlerLobbyComponent.privateChatButton.onClick.AddListener(ClearActivePlayerChat);
        UIHandlerLobbyComponent.privateChatButton.onClick.AddListener(OpenEmptyChatBox);
        UIHandlerLobbyComponent.leaderLeavePartyButton.onClick.AddListener(OnLeavePartyButtonClicked);
        UIHandlerLobbyComponent.localLeavePartyButton.onClick.AddListener(OnLeavePartyButtonClicked);

        matchmakingLogic.AddEventListener();
    }

    void RemoveListeners()
    {
        UIHandlerLobbyComponent.logoutButton.onClick.RemoveAllListeners();
        UIHandlerLobbyComponent.matchmakingButtonCollection.DeregisterAllButton();
        UIHandlerLobbyComponent.friendsTabButton.onClick.RemoveAllListeners();
        UIHandlerLobbyComponent.invitesTabButton.onClick.RemoveAllListeners();
        UIHandlerLobbyComponent.searchFriendButton.onClick.RemoveListener(FindAFriendRequest);
        UIHandlerLobbyComponent.partyMember1stButton.onClick.RemoveAllListeners();
        UIHandlerLobbyComponent.partyMember2ndButton.onClick.RemoveAllListeners();
        UIHandlerLobbyComponent.partyMember3rdButton.onClick.RemoveAllListeners();
        UIHandlerLobbyComponent.localPlayerButton.onClick.RemoveListener(OnLocalPlayerProfileButtonClicked);
        UIHandlerLobbyComponent.acceptPartyInvitation.onClick.RemoveListener(OnAcceptPartyClicked);
        UIHandlerLobbyComponent.declinePartyInvitation.onClick.RemoveListener(OnDeclinePartyClicked);
        UIHandlerLobbyComponent.closePopupPartyButton.onClick.RemoveListener(OnPlayerPartyProfileClicked);
        UIHandlerLobbyComponent.closePopupPartyButton.onClick.RemoveListener(OnClosePartyInfoButtonClicked);
        UIHandlerLobbyComponent.enterToChatButton.onClick.RemoveListener(OpenEmptyChatBox);
        UIHandlerLobbyComponent.sendMessageButton.onClick.RemoveListener(SendChatMessage);
        UIHandlerLobbyComponent.backChatButton.onClick.RemoveListener(ClearActivePlayerChat);
        UIHandlerLobbyComponent.partyChatButton.onClick.RemoveListener(OpenPartyChatBox);
        UIHandlerLobbyComponent.privateChatButton.onClick.RemoveAllListeners();
        UIHandlerLobbyComponent.leaderLeavePartyButton.onClick.RemoveListener(OnLeavePartyButtonClicked);
        UIHandlerLobbyComponent.localLeavePartyButton.onClick.RemoveListener(OnLeavePartyButtonClicked);

        matchmakingLogic.RemoveListener();
    }
    #endregion // UI Listeners

    #region AccelByte Lobby Functions
    private void SetupPopupPartyControl()
    {
        localLeaderCommand = UIHandlerLobbyComponent.popupPartyControl.Find("LocalLeaderCommand");
        localmemberCommand = UIHandlerLobbyComponent.popupPartyControl.Find("LocalMemberCommand");
        memberCommand = UIHandlerLobbyComponent.popupPartyControl.Find("MemberCommand");
        PlayerNameText = UIHandlerLobbyComponent.popupPartyControl.Find("PlayerNameText");
        playerEmailText = UIHandlerLobbyComponent.popupPartyControl.Find("PlayerEmailText");

        // TODO: Add player Image & player stats
    }

    public void ConnectToLobby()
    {
        // Reset lobby to prevent dual session callback
        // Each time user connect to lobby after login, it needs to renew the lobby.
        abLobby = new Lobby(AccelBytePlugin.Config.LobbyServerUrl, new WebSocket(), AccelBytePlugin.GetUser().Session, new CoroutineRunner());
        
        //Establish connection to the lobby service
        abLobby.Connect();
        if (abLobby.IsConnected)
        {
            //If we successfully connected, load our friend list.
            Debug.Log("Successfully Connected to the AccelByte Lobby Service");
            abLobby.SetUserStatus(UserStatus.Availabe, "OnLobby", OnSetUserStatus);
            friendList.Clear();
            SetupLobbyUI();
        }
        else
        {
            //If we don't connect Retry.
            // TODO: use coroutine to day the call to avoid spam
            Debug.LogWarning("Not Connected To Lobby. Attempting to Connect...");
            ConnectToLobby();
        }
    }

    public void OnLogoutButtonClicked()
    {
        UIElementHandler.ShowLoadingPanel();
        
        // Clean lobby state
        if (abLobby.IsConnected)
        {
            OnExitFromLobby(AccelByteManager.Instance.AuthLogic.Logout);
        }
        else
        {
            AccelByteManager.Instance.AuthLogic.Logout();
        }
        CleanupLobbyUI();
    }

    private void CleanupLobbyUI()
    {
        // Clean lobby UI
        matchmakingLogic.CleanupMatchmakingUI();
        HidePopUpPartyControl();
        UnsubscribeAllCallbacks();
    }

    private void SetupLobbyUI()
    {
        abLobby.SetUserStatus(UserStatus.Availabe, "OnLobby", OnSetUserStatus);
        LoadFriendsList();
        SetupGeneralCallbacks();
        SetupFriendCallbacks();
        SetupPartyCallbacks();
        SetupChatCallbacks();
        matchmakingLogic.SetupMatchmakingCallbacks();
        ClearPartySlots();
        GetPartyInfo();
        SetupPlayerInfoBox();
    }

    private void SetupPlayerInfoBox()
    {
        UserData data = AccelByteManager.Instance.AuthLogic.GetUserData();
        UIHandlerLobbyComponent.PlayerDisplayNameText.GetComponent<Text>().text = data.displayName;
    }

    private void SetupGeneralCallbacks()
    {
        abLobby.OnNotification += result => OnNotificationReceived(result);
        abLobby.Disconnected += OnDisconnectNotificationReceived;
    }

    private void SetupFriendCallbacks()
    {
        abLobby.OnIncomingFriendRequest += result => OnIncomingFriendsRequest(result);
        abLobby.FriendRequestAccepted += result => OnFriendRequestAccepted(result);
        abLobby.FriendsStatusChanged += result => OnFriendsStatusChanged(result);
    }

    private void SetupPartyCallbacks()
    {
        abLobby.InvitedToParty += result => OnInvitedToParty(result);
        abLobby.JoinedParty += result => OnMemberJoinedParty(result);
        abLobby.KickedFromParty += result => OnKickedFromParty(result);
        abLobby.LeaveFromParty += result => OnMemberLeftParty(result);
    }

    private void SetupChatCallbacks()
    {
        abLobby.PersonalChatReceived += result => OnPersonalChatReceived(result);
        abLobby.PartyChatReceived += result => OnPartyChatReceived(result);
    }

    private void UnsubscribeAllCallbacks()
    {
        abLobby.Disconnected -= OnDisconnectNotificationReceived;
        abLobby.InvitedToParty -= OnInvitedToParty;
        abLobby.JoinedParty -= OnMemberJoinedParty;
        matchmakingLogic.UnsubscribeAllCallbacks();
        abLobby.KickedFromParty -= OnKickedFromParty;
        abLobby.LeaveFromParty -= OnMemberLeftParty;
    }
    #endregion // AccelByte Lobby Functions

    #region AccelByte Notification Callbacks
    /// <summary>
    /// Callback from OnNotificationReceived lobby event
    /// </summary>
    /// <param name="result"> Result callback to show on game lobby </param>
    private void OnNotificationReceived(Result<Notification> result)
    {
        UIHandlerLobbyComponent.generalNotificationTitle.text = result.Value.topic;
        UIHandlerLobbyComponent.generalNotificationText.text = result.Value.payload;
        UIElementHandler.ShowNotification(UIElementHandler.generalNotification);
    }

    /// <summary>
    /// Callback when disconnected from lobby service
    /// Clean up the lobby menu and logout from IAM
    /// </summary>
    private void OnDisconnectNotificationReceived()
    {
        UIElementHandler.ShowLoadingPanel();
        CleanupLobbyUI();
        AccelByteManager.Instance.AuthLogic.Logout();
    }
    #endregion

    public void WriteInDebugBox(string chat)
    {
        string textChat = "";

        if (chatList.Count >= 5)
        {
            chatList.RemoveAt(0);
        }
        chatList.Add(chat);

        foreach (var s in chatList)
        {
            textChat += s + "\n";
        }
        UIHandlerLobbyComponent.ChatTextbox.GetComponentInChildren<Text>().text = textChat;
    }

    /// <summary>
    /// Triggered when the match is over
    /// </summary>
    /// <param name="isOver"> flag after the game is over </param>
    public void SetIsActionPhaseOver(bool isOver)
    {
        isActionPhaseOver = isOver;
        onMatchOver?.Invoke();
    }

    public bool GetIsActionPhaseOver()
    {
        return isActionPhaseOver;
    }

    #region AccelByte Friend Functions
    /// <summary>
    /// Load friendlist from lobby social service
    /// </summary>
    public void LoadFriendsList()
    {
        abLobby.LoadFriendsList(OnLoadFriendsListRequest);
        UIHandlerLobbyComponent.friendsTabButton.interactable = false;
        UIHandlerLobbyComponent.invitesTabButton.interactable = true;
    }

    /// <summary>
    /// Getting the list of friend status
    /// Returns availability, last seen at and their activity
    /// </summary>
    public void ListFriendsStatus()
    {
        abLobby.ListFriendsStatus(OnListFriendsStatusRequest);
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
    public void FindAFriendRequest()
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
        abLobby.RequestFriend(friendId, callback);
    }

    /// <summary>
    /// Accept a friend request 
    /// </summary>
    /// <param name="friendId"> required friend id </param>
    /// <param name="callback"> result callback to be bound to a button onclick event </param>
    public void AcceptFriendRequest(string friendId, ResultCallback callback)
    {
        abLobby.AcceptFriend(friendId, callback);
    }

    /// <summary>
    /// Decline a friend request
    /// </summary>
    /// <param name="friendId"> required friend id </param>
    /// <param name="callback"> result callback to be bound to a button onclick event </param>
    public void DeclineFriendRequest(string friendId, ResultCallback callback)
    {
        abLobby.RejectFriend(friendId, callback);
    }

    /// <summary>
    /// Getting the incoming friend request(s)
    /// </summary>
    public void GetIncomingFriendsRequest()
    {
        abLobby.ListIncomingFriends(OnGetIncomingFriendsRequest);
    }

    /// <summary>
    /// Getting sent friend request(s)
    /// </summary>
    public void GetOutgoingFriendsRequest()
    {
        abLobby.ListOutgoingFriends(OnGetOutgoingFriendsRequest);
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
                if (activePlayerChatUserId == partyUserId)
                {
                    RefreshDisplayNamePartyChatListUI();
                }
                else
                {
                    RefreshDisplayNamePrivateChatListUI();
                }
            }
        }
    }

    private void RefreshFriendsUI()
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
                friendPrefab.GetComponent<FriendPrefab>().SetInviterPartyStatus(isLocalPlayerInParty);
            }
            else
            {
                bool isNotInParty = true;
                if (partyMemberList.ContainsKey(friend.Value.UserId))
                {
                    isNotInParty = false;
                }
                friendPrefab.GetComponent<FriendPrefab>().SetupFriendUI(friend.Value.DisplayName, "Online", friend.Value.UserId, isNotInParty);
                friendPrefab.GetComponent<FriendPrefab>().SetInviterPartyStatus(isLocalPlayerInParty);
            }
            UIHandlerLobbyComponent.friendScrollView.Rebuild(CanvasUpdate.Layout);
        }
    }

    public void ClearFriendsUIPrefabs()
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

    /// <summary>
    /// Callback on SetUserStatus
    /// Could be used for various status other than Online or Offline
    /// </summary>
    /// <param name="result"></param>
    private void OnSetUserStatus(Result result)
    {
        if (result.IsError)
        {
            Debug.Log("OnSetUserStatus failed:" + result.Error.Message);
            Debug.Log("OnSetUserStatus Response Code::" + result.Error.Code);

        }
        else
        {
            Debug.Log("OnSetUserStatus Success ");
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
                UIElementHandler.ShowNotification(UIElementHandler.inviteNotification);
            }
        }
    }
    #endregion

    #region AccelByte Party Functions
    /// <summary>
    /// Create party lobby service
    /// </summary>
    /// <param name="callback"> callback result that includes party info </param>
    public void CreateParty(ResultCallback<PartyInfo> callback)
    {
        abLobby.CreateParty(callback);
    }

    /// <summary>
    /// Create party if not in a party yet
    /// then invite a friend to the party
    /// </summary>
    /// <param name="userId"> required userid from the friend list </param>
    public void CreateAndInvitePlayer(string userId)
    {
        if (!isLocalPlayerInParty)
        {
            abLobby.CreateParty(OnPartyCreated);
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

        abLobby.InviteToParty(invitedPlayerId, callback);
    }

    /// <summary>
    /// Kick party member from a party
    /// hide popup party UI
    /// </summary>
    /// <param name="id"> required userid to kick </param>
    public void KickPartyMember(string id)
    {
        abLobby.KickPartyMember(id, OnKickPartyMember);
        HidePopUpPartyControl();
    }

    /// <summary>
    /// Leave a party
    /// hide popup party UI clear UI chat
    /// </summary>
    public void LeaveParty()
    {
        abLobby.LeaveParty(OnLeaveParty);
        HidePopUpPartyControl();
        ClearActivePlayerChat();
        OpenEmptyChatBox();
    }

    /// <summary>
    /// Get party info that has party id
    /// Party info holds the party leader user id and the members user id
    /// </summary>
    public void GetPartyInfo()
    {
        abLobby.GetPartyInfo(OnGetPartyInfo);
    }

    /// <summary>
    /// Get party member info to get the display name
    /// </summary>
    /// <param name="friendId"></param>
    public void GetPartyMemberInfo(string friendId)
    {
        AccelBytePlugin.GetUser().GetUserByUserId(friendId, OnGetPartyMemberInfo);
    }

    private void ClearPartySlots()
    {
        // Clear the party slot buttons
        for (int i = 0; i < UIHandlerLobbyComponent.partyMemberButtons.Length; i++)
        {
            UIHandlerLobbyComponent.partyMemberButtons[i].GetComponent<PartyPrefab>().OnClearProfileButton();
            UIHandlerLobbyComponent.partyMemberButtons[i].GetComponent<Button>().onClick.AddListener(() => UIElementHandler.ShowExclusivePanel(ExclusivePanelType.FRIENDS));
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

            if (activePlayerChatUserId == partyUserId)
            {
                RefreshDisplayNamePartyChatListUI();
            }
        }

        RefreshFriendsUI();
    }

    private void HidePopUpPartyControl()
    {
        UIHandlerLobbyComponent.popupPartyControl.gameObject.SetActive(false);
    }

    /// <summary>
    /// Get user local player/ self userdata
    /// and setup the party UI
    /// </summary>
    public void OnLocalPlayerProfileButtonClicked()
    {
        // If in a party then show the party control menu party leader can invite, kick and leave the party.
        // party member only able to leave the party.
        if (isLocalPlayerInParty)
        {
            UserData data = AccelByteManager.Instance.AuthLogic.GetUserData();
            ShowPlayerProfile(new PartyData(data.userId, data.displayName, data.emailAddress), true);
        }
    }

    /// <summary>
    /// Do setup show player profile in party UI based on selected party slot button
    /// </summary>
    /// <param name="memberData"> required selected party member's data </param>
    /// <param name="isLocalPlayerButton"> flag of is the selected button is local player's slot (the most left) </param>
    // TODO: Add more player info here player name, email, image, stats ingame
    public void ShowPlayerProfile(PartyData memberData, bool isLocalPlayerButton = false)
    {
        // If visible then toogle it off to refresh the data
        if (UIHandlerLobbyComponent.popupPartyControl.gameObject.activeSelf)
        {
            UIHandlerLobbyComponent.popupPartyControl.gameObject.SetActive(!UIHandlerLobbyComponent.popupPartyControl.gameObject.activeSelf);
            memberCommand.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
        }

        if (isLocalPlayerInParty)
        {
            localLeaderCommand.gameObject.SetActive(false);
            localmemberCommand.gameObject.SetActive(false);
            memberCommand.gameObject.SetActive(false);
            PlayerNameText.GetComponent<Text>().text = memberData.PlayerName;
            playerEmailText.GetComponent<Text>().text = memberData.PlayerEmail;

            UserData data = AccelByteManager.Instance.AuthLogic.GetUserData();
            bool isPartyLeader = data.userId == abPartyInfo.leaderID;

            if (isPartyLeader && isLocalPlayerButton)
            {
                localLeaderCommand.gameObject.SetActive(true); ;
            }
            else if (!isPartyLeader && isLocalPlayerButton)
            {
                localmemberCommand.gameObject.SetActive(true);
            }
            else if (isPartyLeader && !isLocalPlayerButton)
            {
                memberCommand.gameObject.SetActive(true);
            }

            memberCommand.GetComponentInChildren<Button>().onClick.AddListener(() => OnKickFromPartyClicked(memberData.UserID));

            // Show the popup
            UIHandlerLobbyComponent.popupPartyControl.gameObject.SetActive(!UIHandlerLobbyComponent.popupPartyControl.gameObject.activeSelf);
        }
    }

    public void OnClosePartyInfoButtonClicked()
    {
        HidePopUpPartyControl();
    }

    /// <summary>
    /// Accept party invitation by calling join party
    /// require abPartyInvitation from callback party invitation event
    /// </summary>
    public void OnAcceptPartyClicked()
    {
        if (abPartyInvitation != null)
        {
            abLobby.JoinParty(abPartyInvitation.partyID, abPartyInvitation.invitationToken, OnJoinedParty);
        }
        else
        {
            Debug.Log("OnJoinPartyClicked Join party failed abPartyInvitation is null");
        }
    }

    public void OnDeclinePartyClicked()
    {
        Debug.Log("OnDeclinePartyClicked Join party failed");
    }

    public void OnPlayerPartyProfileClicked()
    {
        // If in a party then show the party control menu party leader can invite, kick and leave the party.
        // party member only able to leave the party.
        if (isLocalPlayerInParty)
        {
            // Remove listerner before closing
            if (UIHandlerLobbyComponent.popupPartyControl.gameObject.activeSelf)
            {
                memberCommand.GetComponentInChildren<Button>().onClick.RemoveAllListeners();
            }
            UIHandlerLobbyComponent.popupPartyControl.gameObject.SetActive(!UIHandlerLobbyComponent.popupPartyControl.gameObject.activeSelf);
        }
    }

    public List<PartyData> GetMemberPartyData()
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

    public void OnLeavePartyButtonClicked()
    {
        if (accelByteManager.AuthLogic.GetUserData().userId == abPartyInfo.leaderID)
        {
            localLeaderCommand.gameObject.SetActive(false);
        }
        else
        {
            localmemberCommand.gameObject.SetActive(false);
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
            isLocalPlayerInParty = true;
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
            isLocalPlayerInParty = false;
            RefreshFriendsUI();

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
            isLocalPlayerInParty = true;
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
                isLocalPlayerInParty = false;
                RefreshFriendsUI();
                ClearActivePlayerChat();
                OpenEmptyChatBox();
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
                ClearActivePlayerChat();
                OpenEmptyChatBox();
            }

            isLocalPlayerInParty = true;
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
    private void OnKickFromPartyClicked(string userId)
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
            isRecevedPartyInvitation = true;
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

    #region AccelByte Chat Functions
    public void SendChatMessage()
    {
        if (string.IsNullOrEmpty(UIHandlerLobbyComponent.messageInputField.text))
            WriteWarningInChatBox("Please enter write your message");
        else if (string.IsNullOrEmpty(activePlayerChatUserId))
            WriteWarningInChatBox("Please select player or party to chat");
        else if (!UIHandlerLobbyComponent.partyChatButton.interactable)
            SendPartyChat();
        else
        {
            SendPersonalChat(activePlayerChatUserId);
        }
    }

    /// <summary>
    /// Send a party chat if the player is in a party
    /// </summary>
    private void SendPartyChat()
    {
        abLobby.SendPartyChat(UIHandlerLobbyComponent.messageInputField.text, OnSendPartyChat);
    }

    /// <summary>
    /// Send a personal chat to a friend
    /// </summary>
    /// <param name="userId"> required user id to chat with </param>
    private void SendPersonalChat(string userId)
    {
        abLobby.SendPersonalChat(userId, UIHandlerLobbyComponent.messageInputField.text, OnSendPersonalChat);
    }

    public void ClearChatBoxUIPrefabs()
    {
        if (UIHandlerLobbyComponent.chatBoxScrollContent.childCount > 0)
        {
            for (int i = 0; i < UIHandlerLobbyComponent.chatBoxScrollContent.childCount; i++)
            {
                Destroy(UIHandlerLobbyComponent.chatBoxScrollContent.GetChild(i).gameObject);
            }
        }
    }

    public void ClearPlayerChatListUIPrefabs()
    {
        if (UIHandlerLobbyComponent.playerChatScrollContent.childCount > 0)
        {
            for (int i = 0; i < UIHandlerLobbyComponent.playerChatScrollContent.childCount; i++)
            {
                Destroy(UIHandlerLobbyComponent.playerChatScrollContent.GetChild(i).gameObject);
            }
        }
    }

    private void RefreshDisplayNamePrivateChatListUI()
    {
        ClearPlayerChatListUIPrefabs();
        foreach (var friend in friendList)
        {
            PlayerChatPrefab playerChatPrefab = Instantiate(UIHandlerLobbyComponent.playerChatPrefab, Vector3.zero, Quaternion.identity).GetComponent<PlayerChatPrefab>();
            playerChatPrefab.transform.SetParent(UIHandlerLobbyComponent.playerChatScrollContent, false);

            playerChatPrefab.GetComponent<PlayerChatPrefab>().SetupPlayerChatUI(friend.Value.DisplayName, friend.Value.UserId, friend.Value.IsOnline == "1");
            playerChatPrefab.GetComponent<PlayerChatPrefab>().activePlayerButton.onClick.AddListener(() => OpenPrivateChatBox(friend.Value.UserId));

            if (!string.IsNullOrEmpty(activePlayerChatUserId) && friend.Value.UserId == activePlayerChatUserId)
            {
                playerChatPrefab.GetComponent<PlayerChatPrefab>().backgroundImage.SetActive(true);
            }

            UIHandlerLobbyComponent.friendScrollView.Rebuild(CanvasUpdate.Layout);
        }
    }

    private void RefreshDisplayNamePartyChatListUI()
    {
        ClearPlayerChatListUIPrefabs();
        foreach (var partyMember in abPartyInfo.members)
        {
            var myUserData = accelByteManager.AuthLogic.GetUserData();
            if (partyMemberList.ContainsKey(partyMember) || partyMember == myUserData.userId)
            {
                PlayerChatPrefab playerChatPrefab = Instantiate(UIHandlerLobbyComponent.playerChatPrefab, Vector3.zero, Quaternion.identity).GetComponent<PlayerChatPrefab>();
                playerChatPrefab.transform.SetParent(UIHandlerLobbyComponent.playerChatScrollContent, false);


                if (partyMember != myUserData.userId && !string.IsNullOrEmpty(partyMemberList[partyMember].PlayerName))
                {
                    playerChatPrefab.GetComponent<PlayerChatPrefab>().SetupPlayerChatUI(partyMemberList[partyMember].PlayerName, partyMember, true);
                    playerChatPrefab.GetComponent<PlayerChatPrefab>().activePlayerButton.interactable = false;
                    if (partyMember == abPartyInfo.leaderID)
                    {
                        playerChatPrefab.GetComponent<PlayerChatPrefab>().SetupPartyLeader(partyMemberList[partyMember].PlayerName);
                    }
                }
                else if (partyMember == myUserData.userId)
                {
                    playerChatPrefab.GetComponent<PlayerChatPrefab>().SetupPlayerChatUI(myUserData.displayName, myUserData.userId, true);
                    playerChatPrefab.GetComponent<PlayerChatPrefab>().activePlayerButton.interactable = false;
                    if (partyMember == abPartyInfo.leaderID)
                    {
                        playerChatPrefab.GetComponent<PlayerChatPrefab>().SetupPartyLeader(myUserData.displayName);
                    }
                }

                UIHandlerLobbyComponent.friendScrollView.Rebuild(CanvasUpdate.Layout);
            }
        }
    }

    private void RefreshChatBoxUI()
    {
        ClearChatBoxUIPrefabs();

        if (chatBoxList[activePlayerChatUserId].message.Count > 0)
        {
            for (int i = 0; i < chatBoxList[activePlayerChatUserId].message.Count; i++)
            {
                ChatMessagePrefab chatPrefab = Instantiate(UIHandlerLobbyComponent.chatBoxPrefab, Vector3.zero, Quaternion.identity).GetComponent<ChatMessagePrefab>();
                chatPrefab.transform.SetParent(UIHandlerLobbyComponent.chatBoxScrollContent, false);

                bool isMe = chatBoxList[activePlayerChatUserId].sender[i] == accelByteManager.AuthLogic.GetUserData().userId;

                if (isMe)
                {
                    chatPrefab.GetComponent<ChatMessagePrefab>().WriteMessage("You", chatBoxList[activePlayerChatUserId].message[i], isMe);
                }
                else
                {
                    string playerChatName = friendList[chatBoxList[activePlayerChatUserId].sender[i]].DisplayName;
                    chatPrefab.GetComponent<ChatMessagePrefab>().WriteMessage(playerChatName, chatBoxList[activePlayerChatUserId].message[i], isMe);
                }
                UIHandlerLobbyComponent.friendScrollView.Rebuild(CanvasUpdate.Layout);
            }
        }
    }

    private void WriteWarningInChatBox(string message)
    {
        ChatMessagePrefab chatPrefab = Instantiate(UIHandlerLobbyComponent.chatBoxPrefab, Vector3.zero, Quaternion.identity).GetComponent<ChatMessagePrefab>();
        chatPrefab.transform.SetParent(UIHandlerLobbyComponent.chatBoxScrollContent, false);

        chatPrefab.GetComponent<ChatMessagePrefab>().WriteWarning(message);

        UIHandlerLobbyComponent.friendScrollView.Rebuild(CanvasUpdate.Layout);
    }

    public void OpenEmptyChatBox()
    {
        ClearChatBoxUIPrefabs();
        ClearPlayerChatListUIPrefabs();
        UIHandlerLobbyComponent.privateChatButton.interactable = false;
        UIHandlerLobbyComponent.partyChatButton.interactable = true;
        LoadFriendsList();
    }

    public void OpenPrivateChatBox(string userId)
    {
        activePlayerChatUserId = userId;
        if (!chatBoxList.ContainsKey(activePlayerChatUserId))
        {
            chatBoxList.Add(activePlayerChatUserId, new ChatData(activePlayerChatUserId, new List<string>(), new List<string>()));
        }

        RefreshDisplayNamePrivateChatListUI();
        RefreshChatBoxUI();
    }

    public void OpenPartyChatBox()
    {
        if (partyMemberList.Count > 0)
        {
            activePlayerChatUserId = "party";
            if (!chatBoxList.ContainsKey(activePlayerChatUserId))
            {
                chatBoxList.Add(activePlayerChatUserId, new ChatData(activePlayerChatUserId, new List<string>(), new List<string>()));
            }

            RefreshChatBoxUI();
            RefreshDisplayNamePartyChatListUI();
        }
        else
        {
            UIHandlerLobbyComponent.privateChatButton.interactable = false;
            UIHandlerLobbyComponent.partyChatButton.interactable = true;
            if (isLocalPlayerInParty)
            {
                WriteWarningInChatBox("You don't have any party member");
            }
            else
            {
                WriteWarningInChatBox("You don't have any party");
            }
        }
    }

    public void ClearActivePlayerChat()
    {
        activePlayerChatUserId = null;
    }
    #endregion

    #region AccelByte Chat Callbacks
    /// <summary>
    /// Callback on SendPersonalChat
    /// Refresh chat UI on success, notify if the recipient player is offline
    /// </summary>
    /// <param name="result"> result callback </param>
    private void OnSendPersonalChat(Result result)
    {
        if (result.IsError)
        {
            Debug.Log("Send personal message failed:" + result.Error.Message);
            Debug.Log("Send personal message Response Code: " + result.Error.Code);
            //Show Error Message
            if (result.Error.Code == ErrorCode.ReceiverNotFound)
            {
                WriteWarningInChatBox("Player is offline");
                UIHandlerLobbyComponent.messageInputField.text = string.Empty;
            }
        }
        else
        {
            Debug.Log("Send personal chat successful");

            chatBoxList[activePlayerChatUserId].sender.Add(accelByteManager.AuthLogic.GetUserData().userId);
            chatBoxList[activePlayerChatUserId].message.Add(UIHandlerLobbyComponent.messageInputField.text);
            UIHandlerLobbyComponent.messageInputField.text = string.Empty;

            RefreshChatBoxUI();
        }
    }

    /// <summary>
    /// Callback on SendPartyChat
    /// Refresh UI chat on success
    /// </summary>
    /// <param name="result"> result callback </param>
    private void OnSendPartyChat(Result result)
    {
        if (result.IsError)
        {
            Debug.Log("Send party chat failed:" + result.Error.Message);
            Debug.Log("Send party chat Response Code: " + result.Error.Code);
            if (result.Error.Code == ErrorCode.PartyNotFound)
            {
                WriteWarningInChatBox("Party is not found");
                UIHandlerLobbyComponent.messageInputField.text = string.Empty;
            }
        }
        else
        {
            Debug.Log("Send party chat successful");
            chatBoxList[activePlayerChatUserId].sender.Add(accelByteManager.AuthLogic.GetUserData().userId);
            chatBoxList[activePlayerChatUserId].message.Add(UIHandlerLobbyComponent.messageInputField.text);
            UIHandlerLobbyComponent.messageInputField.text = string.Empty;

            RefreshChatBoxUI();
        }
    }
    #endregion

    #region AccelByte Chat Notification Callbacks
    /// <summary>
    /// Callback from PersonalChatReceived event
    /// Triggered if the player recieved a personal chat message
    /// Update the chat UI on success
    /// </summary>
    /// <param name="result"> result callback </param>
    private void OnPersonalChatReceived(Result<ChatMesssage> result)
    {
        if (result.IsError)
        {
            Debug.Log("Get personal chat failed:" + result.Error.Message);
            Debug.Log("Get personal chat Response Code: " + result.Error.Code);
        }
        else
        {
            receivedPrivateMessage = result.Value;
            isReceivedPrivateMessage = true;
        }
    }

    /// <summary>
    /// Callback from PartyChatReceived event
    /// Triggered if the player is in a party and received a party cchat message
    /// Update UI chat on success
    /// </summary>
    /// <param name="result"> result callback that contains message id, sender, timestamp, and the message </param>
    private void OnPartyChatReceived(Result<ChatMesssage> result)
    {
        if (result.IsError)
        {
            Debug.Log("Get party chat failed:" + result.Error.Message);
            Debug.Log("Get party chat Response Code: " + result.Error.Code);
        }
        else
        {
            receivedPartyMessage = result.Value;
            isReceivedPartyMessage = true;
        }
    }
    #endregion
}
