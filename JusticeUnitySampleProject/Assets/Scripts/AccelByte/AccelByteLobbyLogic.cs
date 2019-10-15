using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AccelByte.Api;
using AccelByte.Models;
using AccelByte.Core;
using UITools;
using System;

public class AccelByteLobbyLogic : MonoBehaviour
{
    private Lobby abLobby;

    private IDictionary<string, FriendData> friendList;
    private PartyInvitation abPartyInvitation;
    private PartyInfo abPartyInfo;
    private string gameMode = "raid-mode";
    private bool isInParty;
    private bool isPartyUpdateReady;
    private List<string> PartyDisplayNames;

    #region UI Fields
    [SerializeField]
    private ScrollRect friendScrollView;
    [SerializeField]
    private Transform friendScrollContent;
    [SerializeField]
    private Transform friendPrefab;
    [SerializeField]
    private InputField emailToFind;
    [SerializeField]
    private ScrollRect friendSearchScrollView;
    [SerializeField]
    private Transform friendSearchScrollContent;
    [SerializeField]
    private Transform friendSearchPrefab;
    [SerializeField]
    private Transform friendInvitePrefab;
    [SerializeField]
    private Transform sentInvitePrefab;
    [SerializeField]
    private Transform matchmakingStatus;
    [SerializeField]
    private Transform popupPartyInvitation;
    [SerializeField]
    private Transform popupPartyControl;
    [SerializeField]
    private Transform[] partyMemberButton;

    private UIElementHandler uiHandler;
    #endregion

    private void Awake()
    {
        uiHandler = gameObject.GetComponent<UIElementHandler>();

        //Initialize our Lobby object
        abLobby = AccelBytePlugin.GetLobby();
        friendList = new Dictionary<string, FriendData>();
        PartyDisplayNames = new List<string>();
    }

    public void ConnectToLobby()
    {
        //Establish connection to the lobby service
        abLobby.Connect();
        if (abLobby.IsConnected)
        {
            //If we successfully connected, load our friend list.
            Debug.Log("Successfully Connected to the AccelByte Lobby Service");
            LoadFriendsList();
            abLobby.InvitedToParty += result => OnInvitedToParty(result);
            abLobby.JoinedParty += result => OnMemberJoinedParty(result);
            abLobby.MatchmakingCompleted += result => OnFindMatchCompleted(result);
            abLobby.DSUpdated += result => OnSuccessMatch(result);
            abLobby.ReadyForMatchConfirmed += result => OnGetReadyConfirmationStatus(result);
        }
        else
        {
            //If we don't connect Retry.
            Debug.LogWarning("Not Connected To Lobby. Attempting to Connect...");
            ConnectToLobby();
        }
    }
    public void DisconnectFromLobby()
    {
        if (abLobby.IsConnected)
        {
            Debug.Log("Disconnect from lobby");
            LoadFriendsList();
            abLobby.InvitedToParty -= OnInvitedToParty;
            abLobby.MatchmakingCompleted -= OnFindMatchCompleted;
            abLobby.DSUpdated -= OnSuccessMatch;
            abLobby.ReadyForMatchConfirmed -= OnGetReadyConfirmationStatus;
        }
        else
        {
            Debug.Log("There is no Connection to lobby");
        }
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
        AccelBytePlugin.GetUser().GetUserByLoginId(emailToFind.text, OnFindFriendByEmailRequest);
    }

    public  void SendFriendRequest(string friendId, ResultCallback callback)
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

    public void InviteToParty(string id, ResultCallback callback)
    {
        string invitedPlayerId = id;

        abLobby.InviteToParty(invitedPlayerId, callback);
    }

    public void LeaveParty()
    {
        abLobby.LeaveParty(OnLeaveParty);
    }

    public void GetPartyInfo()
    {
        abLobby.GetPartyInfo(OnGetPartyInfo);
    }

    public void FindMatch()
    {
        abLobby.StartMatchmaking(gameMode, OnFindMatch);
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

        popupPartyInvitation.gameObject.SetActive(false);
    }

    public void OnDeclinePartyClicked()
    {
        popupPartyInvitation.gameObject.SetActive(false);
        Debug.Log("OnDeclinePartyClicked Join party failed");
    }

    public void OnPlayerPartyProfileClicked()
    {
        // If in a party then show the party control menu party leader can invite, kick and leave the party.
        // party member only able to leave the party.
        if (isInParty)
        {
            popupPartyControl.gameObject.SetActive(!popupPartyControl.gameObject.activeSelf);
        }
    }

    public void OnLeavePartyButtonClicked()
    {
        popupPartyControl.gameObject.SetActive(false);
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
            FriendPrefab friendPrefab = Instantiate(this.friendPrefab, Vector3.zero, Quaternion.identity).GetComponent<FriendPrefab>();
            friendPrefab.transform.SetParent(friendScrollContent, false);

            if (friend.Value.IsOnline == "0")
            {
                friendPrefab.GetComponent<FriendPrefab>().SetupFriendUI(friend.Value.DisplayName, daysInactive.ToString() + " days ago", friend.Value.UserId);
                friendPrefab.GetComponent<FriendPrefab>().SetFriendInfo(false, isInParty);
            }
            else
            {
                friendPrefab.GetComponent<FriendPrefab>().SetupFriendUI(friend.Value.DisplayName, "Online",  friend.Value.UserId);
                friendPrefab.GetComponent<FriendPrefab>().SetFriendInfo(true, isInParty);
            }
            friendScrollView.Rebuild(CanvasUpdate.Layout);
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
            if (!friendList.ContainsKey(result.Value.UserId))
            {
                friendList.Add(result.Value.UserId, new FriendData(result.Value.UserId, result.Value.DisplayName, new DateTime(1970, 12, 30), "1"));
            }
        }
    }

    private void OnGetIncomingFriendsRequest(Result<Friends> result)
    {
        ClearFriendsUIPrefabs();
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
                    Transform friend = Instantiate(friendInvitePrefab, Vector3.zero, Quaternion.identity);
                    friend.transform.SetParent(friendScrollContent, false);

                    friend.GetComponent<InvitationPrefab>().SetupInvitationPrefab(friendId);
                }
            }
        }
        friendScrollView.Rebuild(CanvasUpdate.Layout);
    }

    private void OnGetOutgoingFriendsRequest(Result<Friends> result)
    {
        ClearFriendsUIPrefabs();
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
                    Transform friend = Instantiate(sentInvitePrefab, Vector3.zero, Quaternion.identity);
                    friend.transform.SetParent(friendScrollContent, false);

                    friend.GetComponent<InvitationPrefab>().SetupInvitationPrefab(friendId);

                }
            }
        }

        friendScrollView.Rebuild(CanvasUpdate.Layout);
    }

    private void OnFindFriendByEmailRequest(Result<UserData> result)
    {
        for (int i = 0; i < friendSearchScrollContent.childCount; i++)
        {
            Destroy(friendSearchScrollContent.GetChild(i).gameObject);
        }

        if (result.IsError)
        {
            Debug.Log("GetUserData failed:" + result.Error.Message);
            Debug.Log("GetUserData Response Code: " + result.Error.Code);
        }
        else
        {
            Debug.Log("Search Results:");
            Debug.Log("Display Name: " + result.Value.DisplayName);
            Debug.Log("UserID: " + result.Value.UserId);

            SearchFriendPrefab friend = Instantiate(friendSearchPrefab, Vector3.zero, Quaternion.identity).GetComponent<SearchFriendPrefab>();
            friend.transform.SetParent(friendSearchScrollContent, false);
            friend.GetComponent<SearchFriendPrefab>().SetupFriendPrefab(result.Value.DisplayName, result.Value.UserId);
            friendSearchScrollView.Rebuild(CanvasUpdate.Layout);
        }
    }
	
	private void OnPartyCreated(Result<PartyInfo> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnPartyCreated failed:" + result.Error.Message);
            Debug.Log("OnPartyCreated Response Code::" + result.Error.Code);
            matchmakingStatus.GetComponent<Text>().text = "Failed to create a party";

        }
        else
        {
            Debug.Log("OnPartyCreated Party successfully created with party ID: " + result.Value.partyID);
            matchmakingStatus.GetComponent<Text>().text = "Waiting for other players";
            isInParty = true;
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
            for (int i = 0; i < partyMemberButton.Length; i++)
            {
                partyMemberButton[i].GetComponent<Button>().interactable = true;
                if (partyMemberButton[i].childCount > 1)
                {
                    Destroy(partyMemberButton[i].GetChild(1).gameObject);
                }
            }

            Debug.Log("OnLeaveParty Left a party");
            isInParty = false;
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
            isInParty = true;
            abPartyInfo = result.Value;
            UpdatePartySlotDisplay();
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
            Debug.Log("OnInvitedToParty Received Invitation from" + result.Value.from);
            abPartyInvitation = result.Value;
            StartCoroutine(ShowPopupPartyInvitation());
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
            GetPartyInfo();
            StartCoroutine(WaitForUpdatePartySlot());
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
            isPartyUpdateReady = true;
        }
    }

    private void OnGetPartyUserInfoRequest(Result<UserData> result)
    {
        if (result.IsError)
        {
            Debug.Log("OnGetPartyUserInfoRequest failed:" + result.Error.Message);
            Debug.Log("OnGetPartyUserInfoRequest Response Code: " + result.Error.Code);
            //Show Error Message
        }
        else
        {
            Debug.Log("OnGetPartyUserInfoRequest sent successfully.");
            //Setup user profile popup
            PartyDisplayNames.Add(result.Value.DisplayName);
            RefreshPartyPopupUI();
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
            Debug.Log("OnFindMatch Finding matchmaking . . .");
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
        }
    }
    #endregion
	
    private void ClearFriendsUIPrefabs()
    {
        for (int i = 0; i < friendScrollContent.childCount; i++)
        {
            Destroy(friendScrollContent.GetChild(i).gameObject);
        }
    }

    private void UpdatePartySlotDisplay()
    {
        UserData data = AccelByteManager.Instance.AuthLogic.GetUserData();
        string ownId = data.UserId;
        int offset = 0;

        Debug.Log("UpdatePartySlotDisplay member count: " + abPartyInfo.members.Length);

        // Search for user id in friendlist, if N/A then getUserID
        for (int i = 0; i < abPartyInfo.members.Length; i++)

        {
            Debug.Log("UpdatePartySlotDisplay member : " + abPartyInfo.members[i]);
            if (abPartyInfo.members[i] == ownId)
            {
                offset++;
            }

            if ((i + offset) >= abPartyInfo.members.Length)
            {
                return;
            }

            GetFriendInfo(abPartyInfo.members[i + offset], OnGetPartyUserInfoRequest);            

            Debug.Log("UpdatePartySlotDisplay: " + (i + offset) + " - " + abPartyInfo.members[i + offset]);
        }        
    }

    private void RefreshPartyPopupUI()
    {
        UserData data = AccelByteManager.Instance.AuthLogic.GetUserData();
        string ownId = data.UserId;
        int offset = 0;

        Debug.Log("RefreshPartyPopupUI Member count: " + PartyDisplayNames.Count);

        // copying from RefreshFriendsUI
        for (int i = 0; i < PartyDisplayNames.Count; i++)
        {
            Debug.Log("RefreshPartyPopupUI Member names: " + PartyDisplayNames[i]);

            // if user id is the same then abpartyinfo members index +1 to skip add own info to party slot
            if (abPartyInfo.members[i] == ownId)
            {
                Debug.Log("RefreshPartyPopupUI this is local user");
                offset++;
            }

            if ((i + offset) >= abPartyInfo.members.Length)
            {
                Debug.Log("RefreshPartyPopupUI out of index");
                return;
            }

            // setup player profile + leader party info 
            Debug.Log("RefreshPartyPopupUI Member names entered: " + PartyDisplayNames[i]);
            Debug.Log("RefreshPartyPopupUI member user id entered: " + (i + offset) + " - " + abPartyInfo.members[i + offset]);
            partyMemberButton[i].GetComponent<PartyPrefab>().SetupPlayerProfile(abPartyInfo.members[i + offset], PartyDisplayNames[i], "1", abPartyInfo.leaderID);
            //partyMemberButton[i].GetComponent<Button>().interactable = false;
        }
    }

    public void OnLocalPlayerProfileButtonClicked()
    {
        UserData data = AccelByteManager.Instance.AuthLogic.GetUserData();
        ShowPlayerProfile(data.DisplayName, true);
    }

    public void ShowPlayerProfile(string playerName, bool isLocalPlayerButton = false)
    {
        if (isInParty)
        {
            Transform localLeaderCommand = null;
            Transform localmemberCommand = null;
            Transform memberCommand = null;

            UnityEngine.UI.Image img_playerProfile = popupPartyControl.GetComponentInChildren<UnityEngine.UI.Image>();
            Text txt_playerName = null;

            foreach(Transform child in popupPartyControl)
            {
                if (child.name == "LocalLeaderCommand")
                {
                    localLeaderCommand = child;
                }
                else if (child.name == "LocalMemberCommand")
                {
                    localmemberCommand = child;
                }
                else if (child.name == "MemberCommand")
                {
                    memberCommand = child;
                }
                else if (child.name == "PlayerNameText")
                {
                    txt_playerName = child.GetComponent<Text>();
                    txt_playerName.text = playerName;
                }
            }

            localLeaderCommand.gameObject.SetActive(false);
            localmemberCommand.gameObject.SetActive(false);
            memberCommand.gameObject.SetActive(false);

            UserData data = AccelByteManager.Instance.AuthLogic.GetUserData();
            bool isPartyLeader = data.UserId == abPartyInfo.leaderID;

            if (isPartyLeader && isLocalPlayerButton)
            {
                localLeaderCommand.gameObject.SetActive(true); ;
            }
            else if (!isPartyLeader && isLocalPlayerButton)
            {
                localmemberCommand.gameObject.SetActive(true);
            }
            else if(isPartyLeader && !isLocalPlayerButton)
            {
                memberCommand.gameObject.SetActive(true);
            }

            // show the popup
            popupPartyControl.gameObject.SetActive(!popupPartyControl.gameObject.activeSelf);
        }
    }

    IEnumerator ShowPopupPartyInvitation()
    {
        yield return new WaitForSecondsRealtime(2.0f);
        popupPartyInvitation.gameObject.SetActive(true);
        Debug.Log("ShowPopupPartyInvitation Popup is opened");
    }

    IEnumerator WaitForUpdatePartySlot()
    {
        while (!isPartyUpdateReady)
        {
            yield return new WaitForSecondsRealtime(1.0f);
            Debug.Log("PartyUpdateReady is ready");
            //calling update for party slot display
            UpdatePartySlotDisplay();
        }
        isPartyUpdateReady = false;
    }
}
