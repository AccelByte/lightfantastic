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
    public AccelByteFriendsLogic friendsLogic;

    private GameObject UIHandler;
    private UILobbyLogicComponent UIHandlerLobbyComponent;
    internal UIElementHandler UIElementHandler;

    private static IDictionary<string, ChatData> chatBoxList;
    internal string activePlayerChatUserId;

    private List<string> chatList;
    private ChatMesssage receivedPrivateMessage;
    private ChatMesssage receivedPartyMessage;
    private AccelByteManager accelByteManager;
    private bool isActionPhaseOver = false;

    static bool isReceivedPrivateMessage = false;
    static bool isReceivedPartyMessage = false;

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
        chatBoxList = new Dictionary<string, ChatData>();
        chatList = new List<string>();

        matchmakingLogic = gameObject.GetComponent<AccelByteMatchmakingLogic>();
        partyLogic = gameObject.GetComponent<AccelBytePartyLogic>();
        friendsLogic = gameObject.GetComponent<AccelByteFriendsLogic>();
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
        friendsLogic.Init(UIHandlerLobbyComponent, this);
        
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
        
        UIHandlerLobbyComponent.localPlayerButton.onClick.AddListener(OnLocalPlayerProfileButtonClicked);
        UIHandlerLobbyComponent.enterToChatButton.onClick.AddListener(OpenEmptyChatBox);
        UIHandlerLobbyComponent.sendMessageButton.onClick.AddListener(SendChatMessage);
        UIHandlerLobbyComponent.backChatButton.onClick.AddListener(ClearActivePlayerChat);
        UIHandlerLobbyComponent.partyChatButton.onClick.AddListener(OpenPartyChatBox);
        UIHandlerLobbyComponent.privateChatButton.onClick.AddListener(ClearActivePlayerChat);
        UIHandlerLobbyComponent.privateChatButton.onClick.AddListener(OpenEmptyChatBox);

        matchmakingLogic.AddEventListener();
        partyLogic.AddEventListener();
        friendsLogic.AddEventListener();
    }

    void RemoveListeners()
    {
        UIHandlerLobbyComponent.logoutButton.onClick.RemoveAllListeners();
        UIHandlerLobbyComponent.localPlayerButton.onClick.RemoveListener(OnLocalPlayerProfileButtonClicked);
        UIHandlerLobbyComponent.enterToChatButton.onClick.RemoveListener(OpenEmptyChatBox);
        UIHandlerLobbyComponent.sendMessageButton.onClick.RemoveListener(SendChatMessage);
        UIHandlerLobbyComponent.backChatButton.onClick.RemoveListener(ClearActivePlayerChat);
        UIHandlerLobbyComponent.partyChatButton.onClick.RemoveListener(OpenPartyChatBox);
        UIHandlerLobbyComponent.privateChatButton.onClick.RemoveAllListeners();

        matchmakingLogic.RemoveListener();
        partyLogic.RemoveListener();
        friendsLogic.RemoveListener();
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
            friendsLogic.ClearFriendList();
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
        friendsLogic.LoadFriendsList();
        SetupGeneralCallbacks();
        friendsLogic.SetupFriendCallbacks();
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
        friendsLogic.UnsubscribeAllCallbacks();
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

    internal void RefreshDisplayNamePrivateChatListUI()
    {
        ClearPlayerChatListUIPrefabs();
        foreach (var friend in friendsLogic.GetFriendList())
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
                    string playerChatName = friendsLogic.GetFriendList()[chatBoxList[activePlayerChatUserId].sender[i]].DisplayName;
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
        friendsLogic.LoadFriendsList();
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
