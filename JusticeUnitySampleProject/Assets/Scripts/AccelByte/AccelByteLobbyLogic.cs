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
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class AccelByteLobbyLogic : MonoBehaviour
{
    private Lobby abLobby;

    private GameObject UIHandler;
    private UILobbyLogicComponent UIHandlerLobbyComponent;
    private UIElementHandler UIElementHandler;

    private static IDictionary<string, FriendData> friendList;
    private static IDictionary<string, PartyData> partyMemberList;
    private static IDictionary<string, ChatData> chatBoxList;
    private PartyInvitation abPartyInvitation;
    private PartyInfo abPartyInfo;
    private MatchmakingNotif abMatchmakingNotif;
    private DsNotif abDSNotif;
    private bool connectToLocal;
    private string ipConnectToLocal = "127.0.0.1";
    private string portConnectToLocal = "15937";
    private string lastFriendUserId;
    private string activePlayerChatUserId;

    private static LightFantasticConfig.GAME_MODES gameModeEnum = LightFantasticConfig.GAME_MODES.unitytest;
    private static string partyUserId = LightFantasticConfig.PARTY_CHAT;
    private string gameMode = gameModeEnum.ToString();
    
    static bool isLocalPlayerInParty;
    static bool isReadyToInviteToParty;
    private List<string> chatList;
    private ChatMesssage receivedPrivateMessage;
    private ChatMesssage receivedPartyMessage;
    private FriendsStatusNotif friendsStatusNotif;
    private MultiplayerMenu multiplayerConnect;
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
        multiplayerConnect = gameObject.GetComponent<MultiplayerMenu>();
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
                RefreshDisplayNamPartyChatListUI();
            else
                RefreshDisplayNamPrivateChatListUI();
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
        Debug.Log("ABLobby OnEnable called!");

        // Register to onsceneloaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        Debug.Log("ABLobby OnDisable called!");

        // Register to onsceneloaded
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (UIHandler != null)
        {
            Debug.Log("ABLobby OnDisable remove all listeners!");
            RemoveListeners();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("ABLobby OnSceneLoaded level loaded!");

        RefreshUIHandler();
    }

    public void RefreshUIHandler()
    {
        UIHandler = GameObject.FindGameObjectWithTag("UIHandler");
        if (UIHandler == null)
        {
            Debug.Log("ABLobby RefreshUIHandler no reference to UI Handler!");
            return;
        }
        UIHandlerLobbyComponent = UIHandler.GetComponent<UILobbyLogicComponent>();
        UIElementHandler = UIHandler.GetComponent<UIElementHandler>();

        AddEventListeners();

        SetupPopupPartyControl();
        
        if (isActionPhaseOver)
        {
            Debug.Log("AbLogic SetIsActionPhaseOver called");
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
        Debug.Log("ABLobby AddEventListeners!");
        // Bind Buttons
        UIHandlerLobbyComponent.logoutButton.onClick.AddListener(OnLogoutButtonClicked);
        
        UIHandlerLobbyComponent.matchmakingButtonCollection.On1VS1ButtonClicked.AddListener(delegate
        {
            GameplaySetGameMode(LightFantasticConfig.GAME_MODES.unitytest);
            FindMatchButtonClicked();
            UIHandlerLobbyComponent.matchmakingButtonCollection.UnselectAll();
        });
        UIHandlerLobbyComponent.matchmakingButtonCollection.On4FFAButtonClicked.AddListener(delegate
        {
            GameplaySetGameMode(LightFantasticConfig.GAME_MODES.upto4player);
            FindMatchButtonClicked();
            UIHandlerLobbyComponent.matchmakingButtonCollection.UnselectAll();
        });
        UIHandlerLobbyComponent.matchmakingButtonCollection.OnLocalButtonClicked.AddListener(delegate { GameplaySetIsLocal(true); });
        UIHandlerLobbyComponent.matchmakingButtonCollection.OnOnlineButtonClicked.AddListener(delegate { GameplaySetIsLocal(false); });
        
        UIHandlerLobbyComponent.matchmakingFailedPromptPanel.primaryButtonAction.AddListener(delegate { FindMatch(); });
        
        UIHandlerLobbyComponent.friendsTabButton.onClick.AddListener(ListFriendsStatus);
        UIHandlerLobbyComponent.friendsTabButton.onClick.AddListener(LoadFriendsList);
        UIHandlerLobbyComponent.invitesTabButton.onClick.AddListener(GetIncomingFriendsRequest);
        UIHandlerLobbyComponent.invitesTabButton.onClick.AddListener(GetOutgoingFriendsRequest);
        UIHandlerLobbyComponent.invitesTabButton.onClick.AddListener(ClearFriendsUIPrefabs);
        UIHandlerLobbyComponent.searchFriendButton.onClick.AddListener(FindFriendByEmail);
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
        UIHandlerLobbyComponent.cancelMatchmakingButton.onClick.AddListener(FindMatchCancelClicked);
        // Bind Game Play / matchmaking request configuration
        UIHandlerLobbyComponent.localMatch_IP_inputFields.onValueChanged.AddListener(GameplaySetLocalIP);
        UIHandlerLobbyComponent.localMatch_Port_inputFields.onValueChanged.AddListener(GameplaySetLocalPort);
    }

    void RemoveListeners()
    {
        Debug.Log("ABLobby RemoveListeners!");
        UIHandlerLobbyComponent.logoutButton.onClick.RemoveAllListeners();
        UIHandlerLobbyComponent.matchmakingButtonCollection.DeregisterAllButton();
        UIHandlerLobbyComponent.friendsTabButton.onClick.RemoveAllListeners();
        UIHandlerLobbyComponent.invitesTabButton.onClick.RemoveAllListeners();
        UIHandlerLobbyComponent.searchFriendButton.onClick.RemoveListener(FindFriendByEmail);
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
        UIHandlerLobbyComponent.cancelMatchmakingButton.onClick.RemoveListener(FindMatchCancelClicked);
        UIHandlerLobbyComponent.localMatch_IP_inputFields.onValueChanged.RemoveListener(GameplaySetLocalIP);
        UIHandlerLobbyComponent.localMatch_Port_inputFields.onValueChanged.RemoveListener(GameplaySetLocalPort);
    }
    #endregion // UI Listeners

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
        ShowMatchmakingBoard(false);
        HidePopUpPartyControl();
        UnsubscribeAllCallbacks();
    }

    private void SetupLobbyUI()
    {
        abLobby.SetUserStatus(UserStatus.Availabe, "OnLobby", OnSetUserStatus);
        LoadFriendsList();
        SetupGeneralCallbacks();
        SetupFriendCallbacks();
        SetupMatchmakingCallbacks();
        SetupChatCallbacks();
        ClearPartySlots();
        GetPartyInfo();
        SetupPlayerInfoBox();
    }

    private void SetupPlayerInfoBox()
    {
        UserData data = AccelByteManager.Instance.AuthLogic.GetUserData();
        UIHandlerLobbyComponent.PlayerDisplayNameText.GetComponent<TMPro.TextMeshProUGUI>().text = data.displayName;
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

    private void SetupMatchmakingCallbacks()
    {
        abLobby.InvitedToParty += result => OnInvitedToParty(result);
        abLobby.JoinedParty += result => OnMemberJoinedParty(result);
        abLobby.MatchmakingCompleted += result => OnFindMatchCompleted(result);
        abLobby.DSUpdated += result => OnSuccessMatch(result);
        abLobby.RematchmakingNotif += result => OnRematchmaking(result);
        abLobby.ReadyForMatchConfirmed += result => OnGetReadyConfirmationStatus(result);
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
        abLobby.MatchmakingCompleted -= OnFindMatchCompleted;
        abLobby.DSUpdated -= OnSuccessMatch;
        abLobby.RematchmakingNotif -= OnRematchmaking;
        abLobby.ReadyForMatchConfirmed -= OnGetReadyConfirmationStatus;
        abLobby.KickedFromParty -= OnKickedFromParty;
        abLobby.LeaveFromParty -= OnMemberLeftParty;
    }

    #region Gameplay Configuration Setter

    public void GameplaySetGameMode(LightFantasticConfig.GAME_MODES gameMode_)
    {
        gameModeEnum = gameMode_;
        gameMode = gameMode_.ToString();
    }
    
    public void GameplaySetIsLocal(bool isLocal)
    {
        connectToLocal = isLocal;
    }

    public void GameplaySetLocalIP(string ip)
    {
        ipConnectToLocal = ip;
    }

    public void GameplaySetLocalPort(string port)
    {
        portConnectToLocal = port;
    }
    
    #endregion
    
    #region AccelByte Notification Callbacks
    private void OnNotificationReceived(Result<Notification> result)
    {
        UIHandlerLobbyComponent.generalNotificationTitle.text = result.Value.topic;
        UIHandlerLobbyComponent.generalNotificationText.text = result.Value.payload;
        UIElementHandler.ShowNotification(UIElementHandler.generalNotification);
    }

    private void OnDisconnectNotificationReceived()
    {
        UIElementHandler.ShowLoadingPanel();
        CleanupLobbyUI();
        AccelByteManager.Instance.AuthLogic.Logout();
    }
    #endregion

    #region AccelByte MatchMaking Functions
    public void FindMatch()
    {
        if (connectToLocal)
        {
            abLobby.StartMatchmaking(gameMode, accelByteManager.LocalDSName, OnFindMatch);
        }
        else
        {
            var latencies = AccelByteQosLogic.Instance.GetLatencies();
            if (latencies != null)
            {
                abLobby.StartMatchmaking(gameMode, "", LightFantasticConfig.DS_TARGET_VERSION, latencies, OnFindMatch);
            }
            else
            {
                abLobby.StartMatchmaking(gameMode, "", LightFantasticConfig.DS_TARGET_VERSION, OnFindMatch);
            }
        }
    }

    public void FindMatchButtonClicked()
    {
        if (!isLocalPlayerInParty)
        {
            abLobby.CreateParty(OnPartyCreatedFindMatch);
        }
        else
        {
            FindMatch();
        }
    }

    public void FindMatchCancelClicked()
    {
        abLobby.CancelMatchmaking(gameMode, OnFindMatchCanceled);
    }

    public void OnAcceptReadyForMatchClicked()
    {
        if (abMatchmakingNotif != null)
        {
            abLobby.ConfirmReadyForMatch(abMatchmakingNotif.matchId, OnReadyForMatchConfirmation);
        }

        UIHandlerLobbyComponent.popupMatchConfirmation.gameObject.SetActive(false);
    }

    public void OnDeclineReadyForMatchClicked()
    {
        abLobby.CancelMatchmaking(gameMode, OnFindMatchCanceled);
        UIHandlerLobbyComponent.popupMatchConfirmation.gameObject.SetActive(false);
    }

    private bool allowToConnectServer = true;
    IEnumerator WaitForGameServerReady(string ip, string port)
    {
        if (allowToConnectServer)
        {
            allowToConnectServer = false;
            bool isActive = true;
            while (isActive)
            {
                yield return new WaitForSecondsRealtime(1.0f);
                if (connectToLocal)
                { 
                    ip = ipConnectToLocal;
                    port = portConnectToLocal;
                }
                multiplayerConnect.SetIPAddressPort(ip, port);
                multiplayerConnect.Connect();
                isActive = false;
            }
        }
    }

    public void ShowMatchmakingBoard(bool show, bool gameFound = false)
    {
        // If there's matchmaking board shown, disable the [ONLINE] and [LOCAL] button
        UIHandlerLobbyComponent.matchmakingButtonCollection.SetInteractable(PlayMatchButtonsScript.ButtonList.OnlineButton, !show);
        UIHandlerLobbyComponent.matchmakingButtonCollection.SetInteractable(PlayMatchButtonsScript.ButtonList.LocalButton, !show);
        
        UIHandlerLobbyComponent.matchmakingBoard.waitingTimerLayout.gameObject.SetActive(false);
        UIHandlerLobbyComponent.matchmakingBoard.gameObject.SetActive(show);
        if (!show)
        {
            UIHandlerLobbyComponent.matchmakingBoard.TerminateCountdown();
        }

        if (gameFound)
        {
            UIHandlerLobbyComponent.matchmakingBoard.gameObject.SetActive(true);
        }
        else
        {
            UIHandlerLobbyComponent.matchmakingBoard.waitingTimerLayout.gameObject.SetActive(true);
        }
    }

    private void WriteInDebugBox(string chat)
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

    public void HostingGameServer()
    {
        multiplayerConnect.Host();
    }

    public void ConnecttoGameServer()
    {
        multiplayerConnect.Connect();
    }

    public void SetIsActionPhaseOver(bool isOver)
    {
        isActionPhaseOver = isOver;
        onMatchOver?.Invoke();
    }

    public bool GetIsActionPhaseOver()
    {
        return isActionPhaseOver;
    }

    /// <summary>
    /// Show prompt panel to choose "RETRY" or "CANCEL" matchmaking
    /// </summary>
    private void OnFailedMatchmaking(string reason)
    {
        abLobby.CancelMatchmaking(gameMode, OnFindMatchCanceled);
        UIHandlerLobbyComponent.matchmakingFailedPromptPanel.SetText("MATCHMAKING FAILED", reason);
        UIHandlerLobbyComponent.matchmakingFailedPromptPanel.Show();
    }
    #endregion

    #region AccelByte MatchMaking Callbacks
    private void OnFindMatch(Result<MatchmakingCode> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnFindMatch failed:" + result.Error.Message);
            Debug.Log("OnFindMatch Response Code::" + result.Error.Code);
            OnFailedMatchmaking("Couldn't do a matchmaking");
        }
        else
        {
            allowToConnectServer = true;
            
            Debug.Log("OnFindMatch Finding matchmaking with gameMode: " + gameMode + " . . .");
            WriteInDebugBox("Searching a match game mode " + gameMode);
            ShowMatchmakingBoard(true);
            UIHandlerLobbyComponent.matchmakingBoard.StartCountdown(MatchmakingWaitingPhase.FindMatch,
                delegate
                {
                    OnFailedMatchmaking("Timeout to finding match");
                });
            UIHandlerLobbyComponent.matchmakingBoard.SetGameMode(gameModeEnum);
        }
    }

    private void OnFindMatchCanceled(Result<MatchmakingCode> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnFindMatchCanceled failed:" + result.Error.Message);
            Debug.Log("OnFindMatchCanceled Response Code::" + result.Error.Code);
        }
        else
        {
            Debug.Log("OnFindMatchCanceled The Match is canceled");
            ShowMatchmakingBoard(false);
            WriteInDebugBox(" Match Canceled");
        }
    }

    private void OnReadyForMatchConfirmation(Result result)
    {
        abMatchmakingNotif = null;
        if (result.IsError)
        {
            Debug.Log("OnReadyForMatchConfirmation failed:" + result.Error.Message);
            Debug.Log("OnReadyForMatchConfirmation Response Code::" + result.Error.Code);
        }
        else
        {
            Debug.Log("OnReadyForMatchConfirmation Waiting for other player . . .");
            WriteInDebugBox("Waiting for other players . . .");
        }
    }
    #endregion

    #region AccelByte MatchMaking Notification Callbacks
    private void OnFindMatchCompleted(Result<MatchmakingNotif> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnFindMatchCompleted failed:" + result.Error.Message);
            Debug.Log("OnFindMatchCompleted Response Code::" + result.Error.Code);
        }
        else
        {
            Debug.Log("OnFindMatchCompleted Finding matchmaking Completed");
            Debug.Log("OnFindMatchCompleted Match Found: " + result.Value.matchId);
            Debug.Log(" Match status: " + result.Value.status);
            Debug.Log(" Expected match status: " + MatchmakingNotifStatus.done.ToString());

            abMatchmakingNotif = result.Value;
            abLobby.ConfirmReadyForMatch(abMatchmakingNotif.matchId, OnReadyForMatchConfirmation);
            UIHandlerLobbyComponent.matchmakingBoard.StartCountdown(MatchmakingWaitingPhase.ConfirmingMatch,
            delegate
            {
                OnFailedMatchmaking("Timeout to confirm matchmaking");
            });
            
            if (result.Value.status == MatchmakingNotifStatus.done.ToString())
            {
                WriteInDebugBox(" Match Found: " + result.Value.matchId);
            }
            // if in a party and party leader start a matchmaking
            else if (result.Value.status == MatchmakingNotifStatus.start.ToString())
            {
                ShowMatchmakingBoard(true);
            }
            else if (result.Value.status == MatchmakingNotifStatus.cancel.ToString())
            {
                ShowMatchmakingBoard(false);
            }
        }
    }

    private void OnSuccessMatch(Result<DsNotif> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnSuccessMatch failed:" + result.Error.Message);
            Debug.Log("OnSuccessMatch Response Code::" + result.Error.Code);
            MainThreadTaskRunner.Instance.Run(delegate
            {
                OnFailedMatchmaking("An error occurs in the dedicated server manager");
            });
        }
        else
        {
            if (result.Value.isOK == "false" && !connectToLocal)
            {
                MainThreadTaskRunner.Instance.Run(delegate
                {
                    OnFailedMatchmaking("Failed to create a dedicated server");
                });
                return;
            }
            
            UIHandlerLobbyComponent.matchmakingBoard.StartCountdown(MatchmakingWaitingPhase.WaitingDSM, 
                delegate { OnFailedMatchmaking("Spawning a dedicated server timed out"); });
            Debug.Log("OnSuccessMatch success match completed");
            // DSM on process creating DS
            if (result.Value.status == DSNotifStatus.CREATING.ToString())
            {
                Debug.Log("Waiting for the game server!");
            }
            // DS is ready
            else if (result.Value.status == DSNotifStatus.READY.ToString())
            {
                // Set IP and port to persistent and connect to the game
                Debug.Log("Entering the game!");

                Debug.Log("Lobby OnSuccessMatch Connect");
                MainThreadTaskRunner.Instance.Run(() => { StartCoroutine(WaitForGameServerReady(result.Value.ip, result.Value.port.ToString())); });
            }
            else if (result.Value.status == DSNotifStatus.BUSY.ToString())
            {
                Debug.Log("Entering the game!");

                Debug.Log("Lobby OnSuccessMatch Connect");
                Debug.Log("ip: " + result.Value.ip + "port: " + result.Value.port);
                //StartStartCoroutine(result.Value.ip, result.Value.port.ToString());
                MainThreadTaskRunner.Instance.Run(() => { StartCoroutine(WaitForGameServerReady(result.Value.ip, result.Value.port.ToString())); });
            }

            Debug.Log("OnSuccessMatch ip: " + result.Value.ip + "port: " + result.Value.port);
            WriteInDebugBox("Match Success status " + result.Value.status + " isOK " + result.Value.isOK + " pod: " + result.Value.podName);
            WriteInDebugBox("Match Success IP: " + result.Value.ip + " Port: " + result.Value.port);
            ShowMatchmakingBoard(true, true);
            abDSNotif = result.Value;
        }
    }

    private void OnRematchmaking(Result<RematchmakingNotification> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnRematchmaking failed:" + result.Error.Message);
            Debug.Log("OnRematchmaking Response Code::" + result.Error.Code);
        }
        else
        {
            Debug.Log(string.Format("OnRematchmaking OnProgress. Banned for {0} seconds", result.Value.banDuration));
            WriteInDebugBox(string.Format("OnRematchmaking... Banned for {0} seconds", result.Value.banDuration));
        }
    }

    private void OnGetReadyConfirmationStatus(Result<ReadyForMatchConfirmation> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnGetReadyConfirmationStatus failed:" + result.Error.Message);
            Debug.Log("OnGetReadyConfirmationStatus Response Code::" + result.Error.Code);
        }
        else
        {
            Debug.Log("OnGetReadyConfirmationStatus Ready confirmation completed");
            Debug.Log("OnGetReadyConfirmationStatus: " + result.Value.userId + " is ready");
            WriteInDebugBox("Player " + result.Value.userId + " is ready");
        }
    }
    #endregion

    #region AccelByte Friend Functions
    public void LoadFriendsList()
    {
        abLobby.LoadFriendsList(OnLoadFriendsListRequest);
        UIHandlerLobbyComponent.friendsTabButton.interactable = false;
        UIHandlerLobbyComponent.invitesTabButton.interactable = true;
    }

    //Doesn't return any display names so needs to be married to an array of usernames or a dictionary of online user data.
    public void ListFriendsStatus()
    {
        abLobby.ListFriendsStatus(OnListFriendsStatusRequest);
    }

    public void GetFriendInfo(string friendId, ResultCallback<UserData> callback)
    {
        AccelBytePlugin.GetUser().GetUserByUserId(friendId, callback);
    }

    public void FindFriendByEmail()
    {
        AccelBytePlugin.GetUser().SearchUsers(UIHandlerLobbyComponent.emailToFind.text, OnFindFriendByEmailRequest);
    }

    public void SendFriendRequest(string friendId, ResultCallback callback)
    {
        abLobby.RequestFriend(friendId, callback);
    }

    public void AcceptFriendRequest(string friendId, ResultCallback callback)
    {
        abLobby.AcceptFriend(friendId, callback);
    }

    public void DeclineFriendRequest(string friendId, ResultCallback callback)
    {
        abLobby.RejectFriend(friendId, callback);
    }

    public void GetIncomingFriendsRequest()
    {
        abLobby.ListIncomingFriends(OnGetIncomingFriendsRequest);
    }

    public void GetOutgoingFriendsRequest()
    {
        abLobby.ListOutgoingFriends(OnGetOutgoingFriendsRequest);
    }

    #endregion

    #region AccelByte Friend Callbacks
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

    //THIS REALLY NEEDS TO RETURN THE DISPLAY NAME
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
                if (activePlayerChatUserId == partyUserId)
                {
                    RefreshDisplayNamPartyChatListUI();
                }
                else
                {
                    RefreshDisplayNamPrivateChatListUI();
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
            if (lastFriendUserId == friendUserId)
            {
                ListFriendsStatus();
            }
        }
    }

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
                //Get person's name, picture, etc

                Transform friend = Instantiate(UIHandlerLobbyComponent.friendInvitePrefab, Vector3.zero, Quaternion.identity);
                friend.transform.SetParent(UIHandlerLobbyComponent.friendScrollContent, false);

                friend.GetComponent<InvitationPrefab>().SetupInvitationPrefab(friendId);
            }
        }
        UIHandlerLobbyComponent.friendScrollView.Rebuild(CanvasUpdate.Layout);
    }

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

    private void OnFindFriendByEmailRequest(Result<PagedPublicUsersInfo> result)
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
                friend.GetComponent<SearchFriendPrefab>().SetupFriendPrefab(result.Value.data[0].displayName, result.Value.data[0].userId);
                UIHandlerLobbyComponent.friendSearchScrollView.Rebuild(CanvasUpdate.Layout);
            }
            else
            {
                Debug.Log("Search Results: Not Found!");
            }
        }
    }

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
    private void OnFriendsStatusChanged(Result<FriendsStatusNotif> result)
    {
        friendsStatusNotif = result.Value;
        isFriendStatusChanged = true;
    }

    //Updating your friends list if someone accepts your friend request
    private void OnFriendRequestAccepted(Result<Friend> result)
    {
        isFriendAcceptRequest = true;
    }

    //You have a new friend invite!
    private void OnIncomingFriendsRequest(Result<Friend> result)
    {
        GetFriendInfo(result.Value.friendId, OnGetFriendInfoIncomingFriendRequest);
    }

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
    public void CreateParty(ResultCallback<PartyInfo> callback)
    {
        abLobby.CreateParty(callback);
    }

    public void CreateAndInvitePlayer(string userId)
    {
        if (!isLocalPlayerInParty)
            abLobby.CreateParty(OnPartyCreated);
        else
            isReadyToInviteToParty = true;
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
                Debug.Log("WaitForInviteToParty InviteToParty is ready");
                InviteToParty(userID, OnInviteParty);
                isReadyToInviteToParty = isActive = false;
            }
        }
    }

    public void InviteToParty(string id, ResultCallback callback)
    {
        string invitedPlayerId = id;

        abLobby.InviteToParty(invitedPlayerId, callback);
    }

    public void KickPartyMember(string id)
    {
        abLobby.KickPartyMember(id, OnKickPartyMember);
        HidePopUpPartyControl();
    }
    public void LeaveParty()
    {
        abLobby.LeaveParty(OnLeaveParty);
        HidePopUpPartyControl();
        ClearActivePlayerChat();
        OpenEmptyChatBox();
    }

    public void GetPartyInfo()
    {
        abLobby.GetPartyInfo(OnGetPartyInfo);
    }

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
                RefreshDisplayNamPartyChatListUI();
            }
        }

        RefreshFriendsUI();
    }

    private void HidePopUpPartyControl()
    {
        UIHandlerLobbyComponent.popupPartyControl.gameObject.SetActive(false);
    }

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

        //PopupManager.Instance.HidePopup();
    }

    public void OnDeclinePartyClicked()
    {
        Debug.Log("OnDeclinePartyClicked Join party failed");

        //PopupManager.Instance.HidePopup();
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

    private void OnPartyCreatedFindMatch(Result<PartyInfo> result)
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
            FindMatch();
        }
    }

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

    private void OnInviteParty(Result result)
    {
        if (result.IsError)
        {
            Debug.Log("OnInviteParty failed:" + result.Error.Message);
            Debug.Log("OnInviteParty Response Code::" + result.Error.Code);

            // if the player already in party then notify the user
            PopupManager.Instance.ShowPopupWarning("Invite to Party Failed", " " + result.Error.Message, "OK");
        }
        else
        {
            Debug.Log("OnInviteParty Succeded on Inviting player to party");
            PopupManager.Instance.ShowPopupWarning("Invite to Party Success", "Waiting for invitee acceptance", "OK");
        }
    }

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

    private void OnKickFromPartyClicked(string userId)
    {
        Debug.Log("OnKickFromPartyClicked Usertokick userId");
        KickPartyMember(userId);
    }

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
            abPartyInvitation = result.Value;
            isRecevedPartyInvitation = true;
        }
    }

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
            MainThreadTaskRunner.Instance.Run(delegate
            {
                PopupManager.Instance.ShowPopupWarning("A New Party Member", "A new member just joined the party!", "OK");
            });
        }
    }

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

    private void SendPartyChat()
    {
        abLobby.SendPartyChat(UIHandlerLobbyComponent.messageInputField.text, OnSendPartyChat);
    }

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

    private void RefreshDisplayNamPrivateChatListUI()
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

    private void RefreshDisplayNamPartyChatListUI()
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

        RefreshDisplayNamPrivateChatListUI();
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
            RefreshDisplayNamPartyChatListUI();
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
