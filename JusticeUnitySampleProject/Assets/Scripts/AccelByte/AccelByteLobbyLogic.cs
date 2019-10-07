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

    private UIElementHandler uiHandler;
    #endregion

    private void Awake()
    {
        uiHandler = gameObject.GetComponent<UIElementHandler>();

        //Initialize our Lobby object
        abLobby = AccelBytePlugin.GetLobby();
        friendList = new Dictionary<string, FriendData>();
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
        }
        else
        {
            //If we don't connect Retry.
            Debug.LogWarning("Not Connected To Lobby. Attempting to Connect...");
            ConnectToLobby();
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
                friendPrefab.GetComponent<FriendPrefab>().SetupFriendUI(friend.Value.DisplayName, daysInactive.ToString() + " days ago");
            }
            else
            {
                friendPrefab.GetComponent<FriendPrefab>().SetupFriendUI(friend.Value.DisplayName, "Online");
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
    #endregion

    private void ClearFriendsUIPrefabs()
    {
        for (int i = 0; i < friendScrollContent.childCount; i++)
        {
            Destroy(friendScrollContent.GetChild(i).gameObject);
        }
    }
}
