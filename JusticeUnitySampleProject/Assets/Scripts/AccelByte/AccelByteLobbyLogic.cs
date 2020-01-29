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
using UnityEngine.SceneManagement;

public class AccelByteLobbyLogic : MonoBehaviour
{
    private Lobby abLobby;

    private GameObject UIHandler;
    private UILobbyLogicComponent UIHandlerLobbyComponent;
    private UIElementHandler UIElementHandler;

    private IDictionary<string, FriendData> friendList;
    private IDictionary<string, PartyData> partyMemberList;
    private PartyInvitation abPartyInvitation;
    private PartyInfo abPartyInfo;
    private MatchmakingNotif abMatchmakingNotif;
    private DsNotif abDSNotif;
    [SerializeField]
    private bool connectToLocal;
    [SerializeField]
    private string gameMode = "testUnity";
    private bool isLocalPlayerInParty;
    private bool isReadyToUpdatePartySlot;
    private bool isReadyToInviteToParty;
    private List<string> chatList;
    private MultiplayerMenu multiplayerConnect;
    private AccelByteManager accelByteManager;
    private bool isActionPhaseOver = false;
    #region UI Fields
    private Transform localLeaderCommand;
    private Transform localmemberCommand;
    private Transform memberCommand;
    private Transform PlayerNameText;
    private Transform playerEmailText;
    private Transform matchmakingBoardSearchLayout;
    private Transform matchmakingBoardMatchFoundLayout;
    private Transform loadingDots;
    private Transform timeLeftText;
    #endregion

    private void Awake()
    {
        accelByteManager = gameObject.GetComponent<AccelByteManager>();
        //Initialize our Lobby object
        abLobby = AccelBytePlugin.GetLobby();
        friendList = new Dictionary<string, FriendData>();
        partyMemberList = new Dictionary<string, PartyData>();
        chatList = new List<string>();
        multiplayerConnect = gameObject.GetComponent<MultiplayerMenu>();
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
        SetupMatchmakingBoard();
		
        if (isActionPhaseOver)
        {
            Debug.Log("AbLogic SetIsActionPhaseOver called");
            // move to main menu screen
            // TODO: after from action pahse all the main menu stuff has tobe refreshed
            UIElementHandler.FadeLogin();
            UIElementHandler.FadePersistentFriends();
            UIElementHandler.FadeMenu();
            //AccelByteManager.Instance.UserProfileLogic.Init();
            SetupLobbyUI();

            isActionPhaseOver = false;
        }
    }

    void AddEventListeners()
    {
        Debug.Log("ABLobby AddEventListeners!");
        // Bind Buttons
        UIHandlerLobbyComponent.logoutButton.onClick.AddListener(DisconnectFromLobby);
        UIHandlerLobbyComponent.logoutButton.onClick.AddListener(AccelByteManager.Instance.AuthLogic.Logout);
        UIHandlerLobbyComponent.findMatchButton.onClick.AddListener(FindMatchButtonClicked);
        UIHandlerLobbyComponent.friendsTabButton.onClick.AddListener(ListFriendsStatus);
        UIHandlerLobbyComponent.friendsTabButton.onClick.AddListener(LoadFriendsList);
        UIHandlerLobbyComponent.invitesTabButton.onClick.AddListener(GetIncomingFriendsRequest);
        UIHandlerLobbyComponent.invitesTabButton.onClick.AddListener(GetOutgoingFriendsRequest);
        UIHandlerLobbyComponent.invitesTabButton.onClick.AddListener(ClearFriendsUIPrefabs);
        UIHandlerLobbyComponent.searchFriendButton.onClick.AddListener(FindFriendByEmail);
        UIHandlerLobbyComponent.localPlayerButton.onClick.AddListener(OnLocalPlayerProfileButtonClicked);
        UIHandlerLobbyComponent.partyMember1stButton.onClick.AddListener(ListFriendsStatus);
        UIHandlerLobbyComponent.partyMember2ndButton.onClick.AddListener(ListFriendsStatus);
        UIHandlerLobbyComponent.partyMember3rdButton.onClick.AddListener(ListFriendsStatus);
        UIHandlerLobbyComponent.acceptPartyInvitation.onClick.AddListener(OnAcceptPartyClicked);
        UIHandlerLobbyComponent.declinePartyInvitation.onClick.AddListener(OnDeclinePartyClicked);
        UIHandlerLobbyComponent.closePopupPartyButton.onClick.AddListener(OnPlayerPartyProfileClicked);
        UIHandlerLobbyComponent.leaderLeavePartyButton.onClick.AddListener(OnLeavePartyButtonClicked);
        UIHandlerLobbyComponent.localLeavePartyButton.onClick.AddListener(OnLeavePartyButtonClicked);
        UIHandlerLobbyComponent.cancelMatchmakingButton.onClick.AddListener(FindMatchCancelClicked);
    }

    void RemoveListeners()
    {
        Debug.Log("ABLobby RemoveListeners!");
        UIHandlerLobbyComponent.logoutButton.onClick.RemoveAllListeners();
        UIHandlerLobbyComponent.findMatchButton.onClick.RemoveListener(FindMatchButtonClicked);
        UIHandlerLobbyComponent.friendsTabButton.onClick.RemoveAllListeners();
        UIHandlerLobbyComponent.invitesTabButton.onClick.RemoveAllListeners();
        UIHandlerLobbyComponent.searchFriendButton.onClick.RemoveListener(FindFriendByEmail);
        UIHandlerLobbyComponent.partyMember1stButton.onClick.RemoveListener(ListFriendsStatus);
        UIHandlerLobbyComponent.partyMember2ndButton.onClick.RemoveListener(ListFriendsStatus);
        UIHandlerLobbyComponent.partyMember3rdButton.onClick.RemoveListener(ListFriendsStatus);
        UIHandlerLobbyComponent.localPlayerButton.onClick.RemoveListener(OnLocalPlayerProfileButtonClicked);
        UIHandlerLobbyComponent.acceptPartyInvitation.onClick.RemoveListener(OnAcceptPartyClicked);
        UIHandlerLobbyComponent.declinePartyInvitation.onClick.RemoveListener(OnDeclinePartyClicked);
        UIHandlerLobbyComponent.closePopupPartyButton.onClick.RemoveListener(OnPlayerPartyProfileClicked);
        UIHandlerLobbyComponent.leaderLeavePartyButton.onClick.RemoveListener(OnLeavePartyButtonClicked);
        UIHandlerLobbyComponent.localLeavePartyButton.onClick.RemoveListener(OnLeavePartyButtonClicked);
        UIHandlerLobbyComponent.cancelMatchmakingButton.onClick.RemoveListener(FindMatchCancelClicked);
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

    private void SetupMatchmakingBoard()
    {
        matchmakingBoardSearchLayout = UIHandlerLobbyComponent.matchmakingBoard.Find("SearchModeLayout");
        matchmakingBoardMatchFoundLayout = UIHandlerLobbyComponent.matchmakingBoard.Find("MatchFoundLayout");
        loadingDots = matchmakingBoardSearchLayout.Find("LoadingDots");
        timeLeftText = matchmakingBoardSearchLayout.Find("TimeText");
    }

    public void ConnectToLobby()
    {
        //Establish connection to the lobby service
        abLobby.Connect();
        if (abLobby.IsConnected)
        {
            //If we successfully connected, load our friend list.
            Debug.Log("Successfully Connected to the AccelByte Lobby Service");
            abLobby.SetUserStatus(UserStatus.Availabe, "OnLobby", OnSetUserStatus);
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

    public void DisconnectFromLobby()
    {
        if (abLobby.IsConnected)
        {
            Debug.Log("Disconnect from lobby");
            abLobby.SetUserStatus(UserStatus.Offline, "Offline", OnSetUserStatus);
            LoadFriendsList();
            UnsubscribeAllCallbacks();
        }
        else
        {
            Debug.Log("There is no Connection to lobby");
        }
    }

    private void SetupLobbyUI()
    {
        abLobby.SetUserStatus(UserStatus.Availabe, "OnLobby", OnSetUserStatus);
        LoadFriendsList();
        SetupGeneralCallbacks();
        SetupFriendCallbacks();
        SetupMatchmakingCallbacks();
        SetupChatCallbacks();
    }

    private void SetupGeneralCallbacks()
    {
        abLobby.OnNotification += result => OnNotificationReceived(result);
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
        abLobby.InvitedToParty -= OnInvitedToParty;
        abLobby.JoinedParty -= OnMemberJoinedParty;
        abLobby.MatchmakingCompleted -= OnFindMatchCompleted;
        abLobby.DSUpdated -= OnSuccessMatch;
        abLobby.RematchmakingNotif -= OnRematchmaking;
        abLobby.ReadyForMatchConfirmed -= OnGetReadyConfirmationStatus;
        abLobby.KickedFromParty -= OnKickedFromParty;
        abLobby.LeaveFromParty -= OnMemberLeftParty;
    }

    public void LoadFriendsList()
    {
        abLobby.LoadFriendsList(OnLoadFriendsListRequest);
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

    public void CreateParty()
    {
        abLobby.CreateParty(OnPartyCreated);
    }

    public void CreateParty(ResultCallback<PartyInfo> callback)
    {
        abLobby.CreateParty(callback);
    }

    public void CreateAndInvitePlayer(string userId)
    {
        abLobby.CreateParty(OnPartyCreated);
        StartCoroutine(WaitForInviteToParty(userId));
    }

    public void InviteToParty(string id, ResultCallback callback)
    {
        string invitedPlayerId = id;

        abLobby.InviteToParty(invitedPlayerId, callback);
    }

    public void KickPartyMember(string id)
    {
        abLobby.KickPartyMember(id, OnKickPartyMember);
    }
    public void LeaveParty()
    {
        abLobby.LeaveParty(OnLeaveParty);
    }

    public void GetPartyInfo()
    {
        abLobby.GetPartyInfo(OnGetPartyInfo);
    }

    public void GetPartyMemberInfo(string friendId)
    {
        AccelBytePlugin.GetUser().GetUserByUserId(friendId, OnGetPartyMemberInfo);
    }

    public void FindMatch()
    {
        if (connectToLocal)
        {
            abLobby.StartMatchmaking(gameMode, accelByteManager.LocalDSName, OnFindMatch);
        }
        else
        {
            abLobby.StartMatchmaking(gameMode, OnFindMatch);
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

        UIHandlerLobbyComponent.popupPartyInvitation.gameObject.SetActive(false);
    }

    public void OnDeclinePartyClicked()
    {
        UIHandlerLobbyComponent.popupPartyInvitation.gameObject.SetActive(false);
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
        UIHandlerLobbyComponent.popupPartyControl.gameObject.SetActive(false);
        LeaveParty();
    }

    #region AccelByte Lobby Callbacks
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
                GetFriendInfo(result.Value.friendsId[i], OnGetFriendInfoRequest);
            }
            ListFriendsStatus();
        }
    }

    //THIS REALLY NEEDS TO RETURN THE DISPLAY NAME
    private void OnListFriendsStatusRequest(Result<FriendsStatus> result)
    {
        ClearFriendsUIPrefabs();
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
                    friendList[result.Value.friendsId[i]] = new FriendData(result.Value.friendsId[i], friendList[result.Value.friendsId[i]].DisplayName, result.Value.lastSeenAt[i], result.Value.availability[i]);
                }
                RefreshFriendsUI();
            }
        }
    }

    private void RefreshFriendsUI()
    {
        foreach (KeyValuePair<string, FriendData> friend in friendList)
        {
            int daysInactive = System.DateTime.Now.Subtract(friend.Value.LastSeen).Days;
            FriendPrefab friendPrefab = Instantiate(UIHandlerLobbyComponent.friendPrefab, Vector3.zero, Quaternion.identity).GetComponent<FriendPrefab>();
            friendPrefab.transform.SetParent(UIHandlerLobbyComponent.friendScrollContent, false);

            if (friend.Value.IsOnline == "0")
            {
                friendPrefab.GetComponent<FriendPrefab>().SetupFriendUI(friend.Value.DisplayName, daysInactive.ToString() + " days ago", friend.Value.UserId);
                friendPrefab.GetComponent<FriendPrefab>().SetInviterPartyStatus(isLocalPlayerInParty);
            }
            else
            {
                friendPrefab.GetComponent<FriendPrefab>().SetupFriendUI(friend.Value.DisplayName, "Online", friend.Value.UserId, friend.Value.IsOnline == "1");
                friendPrefab.GetComponent<FriendPrefab>().SetInviterPartyStatus(isLocalPlayerInParty);
            }
            UIHandlerLobbyComponent.friendScrollView.Rebuild(CanvasUpdate.Layout);
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
            if (!friendList.ContainsKey(result.Value.userId))
            {
                friendList.Add(result.Value.userId, new FriendData(result.Value.userId, result.Value.displayName, new DateTime(1970, 12, 30), "1"));
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
                Debug.Log("Friend Id: " + friendId);
                //Get person's name, picture, etc
                Friends abInvites = result.Value;
                for (int i = 0; i < abInvites.friendsId.Length; i++)
                {
                    Transform friend = Instantiate(UIHandlerLobbyComponent.friendInvitePrefab, Vector3.zero, Quaternion.identity);
                    friend.transform.SetParent(UIHandlerLobbyComponent.friendScrollContent, false);

                    friend.GetComponent<InvitationPrefab>().SetupInvitationPrefab(friendId);
                }
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
                Debug.Log("Friend Id: " + friendId);
                //Get person's name, picture, etc

                Friends abInvites = result.Value;
                for (int i = 0; i < abInvites.friendsId.Length; i++)
                {
                    Transform friend = Instantiate(UIHandlerLobbyComponent.sentInvitePrefab, Vector3.zero, Quaternion.identity);
                    friend.transform.SetParent(UIHandlerLobbyComponent.friendScrollContent, false);

                    friend.GetComponent<InvitationPrefab>().SetupInvitationPrefab(friendId);

                }
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
            Debug.Log("OnJoinedParty Joined party with ID: " + result.Value.partyID);
            isLocalPlayerInParty = true;
            abPartyInfo = result.Value;

            ClearPartySlots();
            for (int i = 0; i < result.Value.members.Length; i++)
            {
                // get member info
                Debug.Log("OnGetPartyInfo adding new party member: " + result.Value.members[i]);
                GetPartyMemberInfo(result.Value.members[i]);
            }
            StartCoroutine(WaitForUpdatedPartyInfo());
            isReadyToUpdatePartySlot = true;
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
            isLocalPlayerInParty = false;
            ClearPartySlots();
        }
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
            Debug.Log("OnInviteParty Succeded on Inviting player to party");
        }
    }

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
            AccelBytePlugin.GetUser().GetUserByUserId(result.Value.from, OnGetUserOnInvite);
            abPartyInvitation = result.Value;
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
            StartCoroutine(ShowPopupPartyInvitation(result.Value.displayName));
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
            ClearPartySlots();
            GetPartyInfo();
            StartCoroutine(WaitForUpdatedPartyInfo());
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
            ClearPartySlots();
            GetPartyInfo();
            StartCoroutine(WaitForUpdatedPartyInfo());
        }
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
            StartCoroutine(WaitForUpdatedPartyInfo());
        }
    }

    private void OnGetPartyInfo(Result<PartyInfo> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnGetPartyInfo failed:" + result.Error.Message);
            Debug.Log("OnGetPartyInfo Response Code::" + result.Error.Code);
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
            isReadyToUpdatePartySlot = true;
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
        }
    }

    private void OnFindMatch(Result<MatchmakingCode> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnFindMatch failed:" + result.Error.Message);
            Debug.Log("OnFindMatch Response Code::" + result.Error.Code);
        }
        else
        {
            Debug.Log("OnFindMatch Finding matchmaking with gameMode: " + gameMode + " . . .");
            WriteInDebugBox("Searching a match game mode " + gameMode);
            ShowMatchmakingBoard(true);
        }
    }

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
            if (result.Value.status == MatchmakingNotifStatus.done.ToString())
            {
                // Auto comfirm ready consent
                abLobby.ConfirmReadyForMatch(abMatchmakingNotif.matchId, OnReadyForMatchConfirmation);
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

    private void OnReadyForMatchConfirmation(Result result)
    {
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

    private void OnSuccessMatch(Result<DsNotif> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnSuccessMatch failed:" + result.Error.Message);
            Debug.Log("OnSuccessMatch Response Code::" + result.Error.Code);
        }
        else
        {
            Debug.Log("OnSuccessMatch success match completed");
            // DSM on process creating DS
            if (result.Value.status == DSNotifStatus.CREATING.ToString())
            {
                // Show countdown waiting for the DS creation
                Debug.Log("Waiting for the game server!");
            }
            // DS is ready
            else if (result.Value.status == DSNotifStatus.READY.ToString())
            {
                // Set IP and port to persistent and connect to the game
                Debug.Log("Entering the game!");

                Debug.Log("Lobby OnSuccessMatch Connect");
                StartCoroutine(WaitForGameServerReady(result.Value.ip, result.Value.port.ToString()));
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
    #endregion

    public void ClearFriendsUIPrefabs()
    {
        for (int i = 0; i < UIHandlerLobbyComponent.friendScrollContent.childCount; i++)
        {
            Destroy(UIHandlerLobbyComponent.friendScrollContent.GetChild(i).gameObject);
        }
    }

    private void ClearPartySlots()
    {
        // Clear the party slot buttons
        for (int i = 0; i < UIHandlerLobbyComponent.partyMemberButtons.Length; i++)
        {
            UIHandlerLobbyComponent.partyMemberButtons[i].GetComponent<PartyPrefab>().OnClearProfileButton();
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
                UIHandlerLobbyComponent.partyMemberButtons[j].GetComponent<PartyPrefab>().SetupPlayerProfile(member.Value, abPartyInfo.leaderID);
                j++;
            }
        }
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

    private void OnKickFromPartyClicked(string userId)
    {
        Debug.Log("OnKickFromPartyClicked Usertokick userId");
        KickPartyMember(userId);
    }

    IEnumerator ShowPopupPartyInvitation(string displayName)
    {
        yield return new WaitForSecondsRealtime(1.0f);
        UIHandlerLobbyComponent.popupPartyInvitation.Find("PopupTittle").GetComponent<Text>().text = "Received Invitation From " + displayName;
        UIHandlerLobbyComponent.popupPartyInvitation.gameObject.SetActive(true);
        Debug.Log("ShowPopupPartyInvitation Popup is opened");
    }

    IEnumerator WaitForUpdatedPartyInfo()
    {
        bool isActive = true;
        while (isActive)
        {
            yield return new WaitForSecondsRealtime(0.5f);

            // Check if partymemberlist filled, excluding local player
            bool isPartyUpdated = (partyMemberList.Count == (abPartyInfo.members.Length - 1));

            // Calling update for party slot display
            if (isReadyToUpdatePartySlot && isPartyUpdated)
            {
                Debug.Log("WaitForUpdatedPartyInfo PartyUpdateReady is ready");
                RefreshPartySlots();
                isReadyToUpdatePartySlot = isActive = false;
            }
        }
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

    IEnumerator WaitForGameServerReady(string ip, string port)
    {
        bool isActive = true;
        while (isActive)
        {
            yield return new WaitForSecondsRealtime(1.0f);
            multiplayerConnect.SetIPAddressPort(ip, port);
            multiplayerConnect.Connect();
            isActive = false;
        }
    }

    public void ShowMatchmakingBoard(bool show, bool gameFound = false)
    {
        matchmakingBoardSearchLayout.gameObject.SetActive(false);
        matchmakingBoardMatchFoundLayout.gameObject.SetActive(false);
        UIHandlerLobbyComponent.matchmakingBoard.gameObject.SetActive(show);

        if (gameFound)
        {
            matchmakingBoardMatchFoundLayout.gameObject.SetActive(true);
            UIHandlerLobbyComponent.matchmakingBoard.gameObject.SetActive(true);
        }
        else
        {
            matchmakingBoardSearchLayout.gameObject.SetActive(true);
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

    // On quit set user status to offline
    private void OnApplicationQuit()
    {
        Debug.Log("Application ending after " + Time.time + " seconds");
        if (abLobby.IsConnected)
        {
            abLobby.LeaveParty(OnLeaveParty);
            abLobby.SetUserStatus(UserStatus.Offline, "Offline", OnSetUserStatus);
        }
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
    }

    public bool GetIsActionPhaseOver()
    {
        return isActionPhaseOver;
    }
	
    #region AccelByte Notification Callbacks
    private void OnNotificationReceived(Result<Notification> result)
    {
        UIHandlerLobbyComponent.generalNotificationTitle.text = result.Value.topic;
        UIHandlerLobbyComponent.generalNotificationText.text = result.Value.payload;
        UIElementHandler.ShowNotification(UIElementHandler.generalNotification);
    }

    //This is for updating your friends list with up to date player information
    private void OnFriendsStatusChanged(Result<FriendsStatusNotif> result)
    {
        string friendName = friendList[result.Value.userID].DisplayName;
        friendList[result.Value.userID] = new FriendData(result.Value.userID, friendName, result.Value.lastSeenAt, result.Value.availability);
        RefreshFriendsUI();
    }

    //Updating your friends list if someone accepts your friend request
    private void OnFriendRequestAccepted(Result<Friend> result)
    {
        throw new NotImplementedException();

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


    private void OnPersonalChatReceived(Result<ChatMesssage> result)
    {
        throw new NotImplementedException();
    }

    private void OnPartyChatReceived(Result<ChatMesssage> result)
    {
        throw new NotImplementedException();
    }
    #endregion


}
