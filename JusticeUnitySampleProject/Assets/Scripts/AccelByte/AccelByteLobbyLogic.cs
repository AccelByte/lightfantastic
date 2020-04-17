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
    public AccelBytePartyLogic partyLogic;
    //TODO another child/sub lobby logic

    private GameObject UIHandler;
    private UILobbyLogicComponent UIHandlerLobbyComponent;
    internal UIElementHandler UIElementHandler;

    private static IDictionary<string, FriendData> friendList;
    private static IDictionary<string, ChatData> chatBoxList;
    private string lastFriendUserId;
    internal string activePlayerChatUserId;

    private List<string> chatList;
    private ChatMesssage receivedPrivateMessage;
    private ChatMesssage receivedPartyMessage;
    private FriendsStatusNotif friendsStatusNotif;
    private AccelByteManager accelByteManager;
    private bool isActionPhaseOver = false;

    static bool isReceivedPrivateMessage = false;
    static bool isReceivedPartyMessage = false;

    static bool isLoadFriendDisplayName = false;
    static bool isFriendStatusChanged = false;
    static bool isFriendAcceptRequest = false;
    #region UI Fields

    internal Transform localLeaderCommand;
    internal Transform localmemberCommand;
    internal Transform memberCommand;
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
        chatBoxList = new Dictionary<string, ChatData>();
        chatList = new List<string>();

        matchmakingLogic = gameObject.GetComponent<AccelByteMatchmakingLogic>();
        partyLogic = gameObject.GetComponent<AccelBytePartyLogic>();
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
            
            if (!chatBoxList.ContainsKey(partyLogic.GetPartyUserId()))
            {
                chatBoxList.Add(partyLogic.GetPartyUserId(), new ChatData(partyLogic.GetPartyUserId(), new List<string>(), new List<string>()));
            }
            chatBoxList[partyLogic.GetPartyUserId()].sender.Add(receivedPartyMessage.from);
            chatBoxList[partyLogic.GetPartyUserId()].message.Add(receivedPartyMessage.payload);

            if (partyLogic.GetPartyUserId() == activePlayerChatUserId)
            {
                RefreshChatBoxUI();
            }
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
            if (activePlayerChatUserId == partyLogic.GetPartyUserId())
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
            abLobby.CancelMatchmaking(matchmakingLogic.gameMode, cancelResult =>
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
        partyLogic.Init(UIHandlerLobbyComponent, this);
        
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
        UIHandlerLobbyComponent.enterToChatButton.onClick.AddListener(OpenEmptyChatBox);
        UIHandlerLobbyComponent.sendMessageButton.onClick.AddListener(SendChatMessage);
        UIHandlerLobbyComponent.backChatButton.onClick.AddListener(ClearActivePlayerChat);
        UIHandlerLobbyComponent.partyChatButton.onClick.AddListener(OpenPartyChatBox);
        UIHandlerLobbyComponent.privateChatButton.onClick.AddListener(ClearActivePlayerChat);
        UIHandlerLobbyComponent.privateChatButton.onClick.AddListener(OpenEmptyChatBox);

        matchmakingLogic.AddEventListener();
        partyLogic.AddEventListener();
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
        UIHandlerLobbyComponent.enterToChatButton.onClick.RemoveListener(OpenEmptyChatBox);
        UIHandlerLobbyComponent.sendMessageButton.onClick.RemoveListener(SendChatMessage);
        UIHandlerLobbyComponent.backChatButton.onClick.RemoveListener(ClearActivePlayerChat);
        UIHandlerLobbyComponent.partyChatButton.onClick.RemoveListener(OpenPartyChatBox);
        UIHandlerLobbyComponent.privateChatButton.onClick.RemoveAllListeners();

        matchmakingLogic.RemoveListener();
        partyLogic.RemoveListener();
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
        partyLogic.CleanupPartyUI();
        UnsubscribeAllCallbacks();
    }

    private void SetupLobbyUI()
    {
        abLobby.SetUserStatus(UserStatus.Availabe, "OnLobby", OnSetUserStatus);
        LoadFriendsList();
        SetupGeneralCallbacks();
        SetupFriendCallbacks();
        SetupChatCallbacks();
        matchmakingLogic.SetupMatchmakingCallbacks();
        partyLogic.SetupPartyCallbacks();
        partyLogic.SetupPartyUI();
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

    private void SetupChatCallbacks()
    {
        abLobby.PersonalChatReceived += result => OnPersonalChatReceived(result);
        abLobby.PartyChatReceived += result => OnPartyChatReceived(result);
    }

    private void UnsubscribeAllCallbacks()
    {
        abLobby.Disconnected -= OnDisconnectNotificationReceived;
        matchmakingLogic.UnsubscribeAllCallbacks();
        partyLogic.UnsubscribeAllCallbacks();
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
                if (activePlayerChatUserId == partyLogic.GetPartyUserId())
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
                friendPrefab.GetComponent<FriendPrefab>().SetInviterPartyStatus(partyLogic.GetIsLocalPlayerInParty());
            }
            else
            {
                bool isNotInParty = true;
                if (partyLogic.GetPartyMemberList().ContainsKey(friend.Value.UserId))
                {
                    isNotInParty = false;
                }
                friendPrefab.GetComponent<FriendPrefab>().SetupFriendUI(friend.Value.DisplayName, "Online", friend.Value.UserId, isNotInParty);
                friendPrefab.GetComponent<FriendPrefab>().SetInviterPartyStatus(partyLogic.GetIsLocalPlayerInParty());
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

    /// <summary>
    /// Get user local player/ self userdata
    /// and setup the party UI
    /// </summary>
    public void OnLocalPlayerProfileButtonClicked()
    {
        // If in a party then show the party control menu party leader can invite, kick and leave the party.
        // party member only able to leave the party.
        if (partyLogic.GetIsLocalPlayerInParty())
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

        if (partyLogic.GetIsLocalPlayerInParty())
        {
            localLeaderCommand.gameObject.SetActive(false);
            localmemberCommand.gameObject.SetActive(false);
            memberCommand.gameObject.SetActive(false);
            PlayerNameText.GetComponent<Text>().text = memberData.PlayerName;
            playerEmailText.GetComponent<Text>().text = memberData.PlayerEmail;

            UserData data = AccelByteManager.Instance.AuthLogic.GetUserData();
            bool isPartyLeader = data.userId == partyLogic.GetAbPartyInfo().leaderID;

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

            memberCommand.GetComponentInChildren<Button>().onClick.AddListener(() => partyLogic.OnKickFromPartyClicked(memberData.UserID));

            // Show the popup
            UIHandlerLobbyComponent.popupPartyControl.gameObject.SetActive(!UIHandlerLobbyComponent.popupPartyControl.gameObject.activeSelf);
        }
    }

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

    internal void RefreshDisplayNamePartyChatListUI()
    {
        ClearPlayerChatListUIPrefabs();
        foreach (var partyMember in partyLogic.GetAbPartyInfo().members)
        {
            var myUserData = accelByteManager.AuthLogic.GetUserData();
            if (partyLogic.GetPartyMemberList().ContainsKey(partyMember) || partyMember == myUserData.userId)
            {
                PlayerChatPrefab playerChatPrefab = Instantiate(UIHandlerLobbyComponent.playerChatPrefab, Vector3.zero, Quaternion.identity).GetComponent<PlayerChatPrefab>();
                playerChatPrefab.transform.SetParent(UIHandlerLobbyComponent.playerChatScrollContent, false);


                if (partyMember != myUserData.userId && !string.IsNullOrEmpty(partyLogic.GetPartyMemberList()[partyMember].PlayerName))
                {
                    playerChatPrefab.GetComponent<PlayerChatPrefab>().SetupPlayerChatUI(partyLogic.GetPartyMemberList()[partyMember].PlayerName, partyMember, true);
                    playerChatPrefab.GetComponent<PlayerChatPrefab>().activePlayerButton.interactable = false;
                    if (partyMember == partyLogic.GetAbPartyInfo().leaderID)
                    {
                        playerChatPrefab.GetComponent<PlayerChatPrefab>().SetupPartyLeader(partyLogic.GetPartyMemberList()[partyMember].PlayerName);
                    }
                }
                else if (partyMember == myUserData.userId)
                {
                    playerChatPrefab.GetComponent<PlayerChatPrefab>().SetupPlayerChatUI(myUserData.displayName, myUserData.userId, true);
                    playerChatPrefab.GetComponent<PlayerChatPrefab>().activePlayerButton.interactable = false;
                    if (partyMember == partyLogic.GetAbPartyInfo().leaderID)
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
        if (partyLogic.GetPartyMemberList().Count > 0)
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
            if (partyLogic.GetIsLocalPlayerInParty())
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
