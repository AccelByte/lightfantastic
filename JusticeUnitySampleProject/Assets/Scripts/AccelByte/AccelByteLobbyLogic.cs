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
    public AccelByteChatLogic chatLogic;

    private GameObject UIHandler;
    private UILobbyLogicComponent UIHandlerLobbyComponent;
    internal UIElementHandler UIElementHandler;

    private AccelByteManager accelByteManager;
    private bool isActionPhaseOver = false;
    private List<string> chatList;

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
        chatList = new List<string>();

        accelByteManager = gameObject.GetComponent<AccelByteManager>();
        //Initialize our Lobby object
        abLobby = AccelBytePlugin.GetLobby();

        matchmakingLogic = gameObject.GetComponent<AccelByteMatchmakingLogic>();
        partyLogic = gameObject.GetComponent<AccelBytePartyLogic>();
        friendsLogic = gameObject.GetComponent<AccelByteFriendsLogic>();
        chatLogic = gameObject.GetComponent<AccelByteChatLogic>();
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
        chatLogic.Init(UIHandlerLobbyComponent, this);
        
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

        matchmakingLogic.AddEventListener();
        partyLogic.AddEventListener();
        friendsLogic.AddEventListener();
        chatLogic.AddEventListener();
    }

    void RemoveListeners()
    {
        UIHandlerLobbyComponent.logoutButton.onClick.RemoveAllListeners();
        UIHandlerLobbyComponent.localPlayerButton.onClick.RemoveListener(OnLocalPlayerProfileButtonClicked);
        
        matchmakingLogic.RemoveListener();
        partyLogic.RemoveListener();
        friendsLogic.RemoveListener();
        chatLogic.RemoveListener();
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
        abLobby = new Lobby(AccelBytePlugin.Config.LobbyServerUrl, new WebSocket(), new LobbyApi(AccelBytePlugin.Config.BaseUrl, new UnityHttpWorker()), AccelBytePlugin.GetUser().Session, AccelBytePlugin.Config.Namespace, new CoroutineRunner());
        
        //Establish connection to the lobby service
        abLobby.Connect();
        if (abLobby.IsConnected)
        {
            //If we successfully connected, load our friend list.
            Debug.Log("Successfully Connected to the AccelByte Lobby Service");
            SetFriendList();
        }
        else
        {
            //If we don't connect Retry.
            // TODO: use coroutine to day the call to avoid spam
            Debug.LogWarning("Not Connected To Lobby. Attempting to Connect...");
            //ConnectToLobby();
        }
    }

    private void SetFriendList()
    {

        abLobby.SetUserStatus(UserStatus.Availabe, "OnLobby", OnSetUserStatus);
        friendsLogic.ClearFriendList();
        SetupLobbyUI();
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
        chatLogic.SetupChatCallbacks();
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

    private void UnsubscribeAllCallbacks()
    {
        abLobby.Disconnected -= OnDisconnectNotificationReceived;
        matchmakingLogic.UnsubscribeAllCallbacks();
        partyLogic.UnsubscribeAllCallbacks();
        friendsLogic.UnsubscribeAllCallbacks();
        chatLogic.UnsubscribeAllCallbacks();
    }


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
    #endregion // AccelByte Lobby Functions

    #region AccelByte Notification Callbacks
    /// <summary>
    /// Callback from OnNotificationReceived lobby event
    /// </summary>
    /// <param name="result"> Result callback to show on game lobby </param>
    private void OnNotificationReceived(Result<Notification> result)
    {
        MainThreadTaskRunner.Instance.Run(() =>
        {
            UIHandlerLobbyComponent.generalNotificationTitle.text = string.IsNullOrEmpty(result.Value.topic)? "UNTITLED" : result.Value.topic;
            UIHandlerLobbyComponent.generalNotificationText.text = result.Value.payload;
            UIElementHandler.ShowNotification(UIElementHandler.generalNotification);
        });
    }

    /// <summary>
    /// Callback when disconnected from lobby service
    /// Clean up the lobby menu and logout from IAM
    /// </summary>
    private void OnDisconnectNotificationReceived(WsCloseCode closeCode)
    {
        UIElementHandler.ShowLoadingPanel();
        CleanupLobbyUI();
        Debug.Log("Lobby Disconnected, code: " + closeCode + " | " + closeCode.ToString());
        AccelByteManager.Instance.AuthLogic.Logout();
    }
    #endregion

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

    ///check if player is a leader/master
    public bool IsLeader()
    {
        bool isLeaderHasMember = partyLogic.GetPartyMemberList().Count > 0;
        UserData data = AccelByteManager.Instance.AuthLogic.GetUserData();
        return partyLogic.GetAbPartyInfo() != null && (data.userId == partyLogic.GetAbPartyInfo().leaderID) && isLeaderHasMember;
    }
}
