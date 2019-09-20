using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using AccelByte.Api;
using AccelByte.Models;
using AccelByte.Core;
using UITools;

public class AccelByteLobbyLogic : MonoBehaviour
{
    Lobby abLobby;
    FriendsStatus abFriendsStatus;

    List<string> friendNames;

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

    private void Awake()
    {
        uiHandler = gameObject.GetComponent<UIElementHandler>();

        abLobby = AccelBytePlugin.GetLobby();
        friendNames = new List<string>();
    }

    public void ConnectToLobby()
    {

        abLobby.Connect();
        if (abLobby.IsConnected)
        {
            Debug.Log("Connected To Lobby");
            LoadFriendsList();
        }
        else
        {
            Debug.Log("Not Connected To Lobby. Attempting to Connect...");

            abLobby.Connect();
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

    private void GetFriendInfo(string friendId)
    {
        AccelBytePlugin.GetUser().GetUserByUserId(friendId, OnGetFriendInfoRequest);
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
                GetFriendInfo(result.Value.friendsId[i]);
            }
            ListFriendsStatus();
        }
    }


    //THIS REALLY NEEDS TO RETURN THE DISPLAY NAME
    private void OnListFriendsStatusRequest(Result<FriendsStatus> result)
    {
        for (int i = 0; i < friendScrollContent.childCount; i++)
        {
            Destroy(friendScrollContent.GetChild(i).gameObject);
        }
        if (result.IsError)
        {
            Debug.Log("ListFriendsStatusRequest failed:" + result.Error.Message);
            Debug.Log("ListFriendsStatusRequest Response Code: " + result.Error.Code);
            //Show Error Message
        }
        else
        {
            if (friendNames.Count > 0)
            {
                Debug.Log("ListFriendsStatusRequest sent successfully.");
                abFriendsStatus = result.Value;
                for (int i = 0; i < abFriendsStatus.friendsId.Length; i++)
                {
                    Debug.Log("Friends Status: ID: " + abFriendsStatus.friendsId[i] + " Last Seen: " + abFriendsStatus.lastSeenAt[i] + " Availability: " + abFriendsStatus.availability[i] + " Activity: " + abFriendsStatus.activity[i]);
                    //Get person's name, picture, etc
                    int daysInactive = System.DateTime.Now.Subtract(abFriendsStatus.lastSeenAt[i]).Days;
                    FriendPrefab friend = Instantiate(friendPrefab, Vector3.zero, Quaternion.identity).GetComponent<FriendPrefab>();
                    friend.transform.SetParent(friendScrollContent, false);

                    if (abFriendsStatus.availability[i] == "0")
                    {
                        friend.GetComponent<FriendPrefab>().SetupFriendUI(friendNames[i], daysInactive.ToString() + " days ago");
                    }
                    else
                    {
                        friend.GetComponent<FriendPrefab>().SetupFriendUI(friendNames[i], "Online");
                    }
                    friendScrollView.Rebuild(CanvasUpdate.Layout);
                }
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
            friendNames.Add(result.Value.DisplayName);
        }
    }

    private void OnGetIncomingFriendsRequest(Result<Friends> result)
    {
        for (int i = 0; i < friendScrollContent.childCount; i++)
        {
            Destroy(friendScrollContent.GetChild(i).gameObject);
        }
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

                    friend.GetComponent<InvitationPrefab>().SetupInvitationPrefab(friendId, friendId);
                    
                    friendScrollView.Rebuild(CanvasUpdate.Layout);
                }
            }
        }
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
            Debug.Log("Loaded incoming friends list successfully.");

            foreach (string friendId in result.Value.friendsId)
            {
                Debug.Log("Friend Id: " + friendId);
                //Get person's name, picture, etc

                Friends abInvites = result.Value;
                for (int i = 0; i < abInvites.friendsId.Length; i++)
                {
                    Transform friend = Instantiate(sentInvitePrefab, Vector3.zero, Quaternion.identity);
                    friend.transform.SetParent(friendScrollContent, false);

                    friend.GetComponent<InvitationPrefab>().SetupInvitationPrefab(friendId, friendId);

                    friendScrollView.Rebuild(CanvasUpdate.Layout);
                }
            }
        }
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
}
