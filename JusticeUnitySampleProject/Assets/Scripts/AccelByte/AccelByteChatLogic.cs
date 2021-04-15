// Copyright (c) 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections.Generic;
using AccelByte.Api;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;
using UnityEngine.UI;

public class AccelByteChatLogic : MonoBehaviour
{
    private enum CHAT_TYPE{PARTY, PERSONAL, CHANNEL};
    private CHAT_TYPE currentChatType;
    private UILobbyLogicComponent UIHandlerLobbyComponent;
    private AccelByteLobbyLogic lobbyLogic;
    private AccelByteManager accelByteManager;

    private static IDictionary<string, ChatData> chatBoxList;
    internal string activePlayerChatUserId;

    private ChatMessage receivedPrivateMessage;
    private ChatMessage receivedPartyMessage;
    private ChannelChatMessage receivedChannelMessage;
    private ChatChannelSlug channel = null;

    static bool isReceivedPrivateMessage = false;
    static bool isReceivedPartyMessage = false;
    static bool isReceivedChannelMessage = false;

    private void Awake()
    {
        chatBoxList = new Dictionary<string, ChatData>();
        Debug.Log("chat box list initiated");
    }

    public void Init(UILobbyLogicComponent uiLobbyLogicComponent, AccelByteLobbyLogic lobbyLogic)
    {
        UIHandlerLobbyComponent = uiLobbyLogicComponent;
        this.lobbyLogic = lobbyLogic;
        accelByteManager = lobbyLogic.GetComponent<AccelByteManager>();
    }

    public void AddEventListener()
    {
        UIHandlerLobbyComponent.enterToChatButton.onClick.AddListener(OpenEmptyChatBox);
        UIHandlerLobbyComponent.sendMessageButton.onClick.AddListener(SendChatMessage);
        UIHandlerLobbyComponent.backChatButton.onClick.AddListener(ClearActivePlayerChat);
        UIHandlerLobbyComponent.partyChatButton.onClick.AddListener(OpenPartyChatBox);
        UIHandlerLobbyComponent.privateChatButton.onClick.AddListener(ClearActivePlayerChat);
        UIHandlerLobbyComponent.privateChatButton.onClick.AddListener(OpenEmptyChatBox);
        UIHandlerLobbyComponent.privateChatButton.onClick.AddListener(LoadFriendList);
        //global chat listener
        UIHandlerLobbyComponent.channelChatButton.onClick.AddListener(JoinChannel);
        UIHandlerLobbyComponent.channelChatButton.onClick.AddListener(ClearActivePlayerChat);
        UIHandlerLobbyComponent.channelChatButton.onClick.AddListener(OpenEmptyChatBox);
    }

    public void RemoveListener()
    {
        UIHandlerLobbyComponent.enterToChatButton.onClick.RemoveListener(OpenEmptyChatBox);
        UIHandlerLobbyComponent.sendMessageButton.onClick.RemoveListener(SendChatMessage);
        UIHandlerLobbyComponent.backChatButton.onClick.RemoveListener(ClearActivePlayerChat);
        UIHandlerLobbyComponent.partyChatButton.onClick.RemoveListener(OpenPartyChatBox);
        UIHandlerLobbyComponent.privateChatButton.onClick.RemoveAllListeners();
        UIHandlerLobbyComponent.channelChatButton.onClick.RemoveAllListeners();
    }

    public void SetupChatCallbacks()
    {
        lobbyLogic.abLobby.PersonalChatReceived += result => OnPersonalChatReceived(result);
        lobbyLogic.abLobby.PartyChatReceived += result => OnPartyChatReceived(result);
        lobbyLogic.abLobby.ChannelChatReceived += result => OnChannelChatReceived(result);
    }

    public void UnsubscribeAllCallbacks()
    {
        lobbyLogic.abLobby.PersonalChatReceived -= OnPersonalChatReceived;
        lobbyLogic.abLobby.PartyChatReceived -= OnPartyChatReceived;
        lobbyLogic.abLobby.ChannelChatReceived -= OnChannelChatReceived;
    }
    // Update is called once per frame
    void Update()
    {
        if (isReceivedPrivateMessage)
        {
            isReceivedPrivateMessage = false;
            currentChatType = CHAT_TYPE.PERSONAL;
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
            currentChatType = CHAT_TYPE.PARTY;
            if (!chatBoxList.ContainsKey(lobbyLogic.partyLogic.GetPartyUserId()))
            {
                chatBoxList.Add(lobbyLogic.partyLogic.GetPartyUserId(), new ChatData(lobbyLogic.partyLogic.GetPartyUserId(), new List<string>(), new List<string>()));
            }
            chatBoxList[lobbyLogic.partyLogic.GetPartyUserId()].sender.Add(receivedPartyMessage.from);
            chatBoxList[lobbyLogic.partyLogic.GetPartyUserId()].message.Add(receivedPartyMessage.payload);

            if (lobbyLogic.partyLogic.GetPartyUserId() == activePlayerChatUserId)
            {
                RefreshChatBoxUI();
            }
        }
        if (isReceivedChannelMessage)
        {
            Debug.Log("channel chat received");
            isReceivedChannelMessage = false;
            currentChatType = CHAT_TYPE.CHANNEL;
            //global message logic
            if(!chatBoxList.ContainsKey(activePlayerChatUserId))
            {
                Debug.Log("add new list channel");
                chatBoxList.Add(activePlayerChatUserId, new ChatData(receivedChannelMessage.from, new List<string>(), new List<string>()));
            }
            AccelBytePlugin.GetUser().GetUserByUserId(receivedChannelMessage.from, OnGetDisplayNameChannelChat);
        }
    }

    #region AccelByte Chat Functions
    public void SendChatMessage()
    {
        if (string.IsNullOrEmpty(UIHandlerLobbyComponent.messageInputField.text))
        {
            WriteWarningInChatBox("Please enter write your message");
        }
        else if (!UIHandlerLobbyComponent.partyChatButton.interactable)
        {
            SendPartyChat();
        }
        else if (!UIHandlerLobbyComponent.channelChatButton.interactable)
        {
            SendChannelChat();
        }
        else if (!UIHandlerLobbyComponent.privateChatButton.interactable)
        {
            if (string.IsNullOrEmpty(activePlayerChatUserId))
            {
                WriteWarningInChatBox("Please select player or party to chat");
            }
            else
            {
                SendPersonalChat(activePlayerChatUserId);
            }
        }
            
    }

    /// <summary>
    /// Send a party chat if the player is in a party
    /// </summary>
    private void SendPartyChat()
    {
        lobbyLogic.abLobby.SendPartyChat(UIHandlerLobbyComponent.messageInputField.text, OnSendPartyChat);
    }

    /// <summary>
    /// Send a personal chat to a friend
    /// </summary>
    /// <param name="userId"> required user id to chat with </param>
    private void SendPersonalChat(string userId)
    {
        lobbyLogic.abLobby.SendPersonalChat(userId, UIHandlerLobbyComponent.messageInputField.text, OnSendPersonalChat);
    }

    /// <summary>
    /// Send a chat to channel
    /// </summary>
    private void SendChannelChat()
    {
        Debug.Log("Channel Send Clicked");
        Debug.Log("Channel Message to send : " + UIHandlerLobbyComponent.messageInputField.text);
        lobbyLogic.abLobby.SendChannelChat(UIHandlerLobbyComponent.messageInputField.text, OnSendChannelChat);
    }

    /// <summary>
    /// join channel 
    /// </summary>
    private void JoinChannel()
    {
        if (lobbyLogic.abLobby.channelSlug == null)
        {
            lobbyLogic.abLobby.JoinDefaultChatChannel(OnJoinChannel);
        }
        Debug.Log("Channel Slug : " + lobbyLogic.abLobby.channelSlug);
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
        foreach (var friend in lobbyLogic.friendsLogic.GetFriendList())
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
        foreach (var partyMember in lobbyLogic.partyLogic.GetAbPartyInfo().members)
        {
            var myUserData = accelByteManager.AuthLogic.GetUserData();
            if (lobbyLogic.partyLogic.GetPartyMemberList().ContainsKey(partyMember) || partyMember == myUserData.userId)
            {
                PlayerChatPrefab playerChatPrefab = Instantiate(UIHandlerLobbyComponent.playerChatPrefab, Vector3.zero, Quaternion.identity).GetComponent<PlayerChatPrefab>();
                playerChatPrefab.transform.SetParent(UIHandlerLobbyComponent.playerChatScrollContent, false);


                if (partyMember != myUserData.userId && !string.IsNullOrEmpty(lobbyLogic.partyLogic.GetPartyMemberList()[partyMember].PlayerName))
                {
                    playerChatPrefab.GetComponent<PlayerChatPrefab>().SetupPlayerChatUI(lobbyLogic.partyLogic.GetPartyMemberList()[partyMember].PlayerName, partyMember, true);
                    playerChatPrefab.GetComponent<PlayerChatPrefab>().activePlayerButton.interactable = false;
                    if (partyMember == lobbyLogic.partyLogic.GetAbPartyInfo().leaderID)
                    {
                        playerChatPrefab.GetComponent<PlayerChatPrefab>().SetupPartyLeader(lobbyLogic.partyLogic.GetPartyMemberList()[partyMember].PlayerName);
                    }
                }
                else if (partyMember == myUserData.userId)
                {
                    playerChatPrefab.GetComponent<PlayerChatPrefab>().SetupPlayerChatUI(myUserData.displayName, myUserData.userId, true);
                    playerChatPrefab.GetComponent<PlayerChatPrefab>().activePlayerButton.interactable = false;
                    if (partyMember == lobbyLogic.partyLogic.GetAbPartyInfo().leaderID)
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
                    string playerChatName = "test";
                    if (currentChatType == CHAT_TYPE.PERSONAL)
                    {
                        playerChatName = lobbyLogic.friendsLogic.GetFriendList()[chatBoxList[activePlayerChatUserId].sender[i]].DisplayName;
                    }
                    else if (currentChatType == CHAT_TYPE.CHANNEL)
                    {
                        playerChatName = chatBoxList[activePlayerChatUserId].sender[i];
                    }
                    
                    chatPrefab.GetComponent<ChatMessagePrefab>().WriteMessage(playerChatName, chatBoxList[activePlayerChatUserId].message[i], isMe);
                }
                UIHandlerLobbyComponent.friendScrollView.Rebuild(CanvasUpdate.Layout);
            }
        }
    }

    private void OnGetDisplayNameChannelChat(Result<UserData> result)
    {
        if (result.IsError)
        {
            Debug.Log("User not found");
        }
        else
        {
            chatBoxList[activePlayerChatUserId].sender.Add(result.Value.displayName);
            chatBoxList[activePlayerChatUserId].message.Add(receivedChannelMessage.payload);
            RefreshChatBoxUI();
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
        if (!UIHandlerLobbyComponent.partyChatButton.interactable)
        {
            LoadFriendList();
        }
        if (!UIHandlerLobbyComponent.channelChatButton.interactable)
        {
            if (channel != null)
            {
                if (chatBoxList.ContainsKey(channel.channelSlug))
                {
                    chatBoxList[channel.channelSlug].sender.Clear();
                    chatBoxList[channel.channelSlug].message.Clear();
                }
            }
        }
    }

    private void LoadFriendList()
    {
        lobbyLogic.friendsLogic.LoadFriendsList();
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
        if (lobbyLogic.partyLogic.GetPartyMemberList().Count > 0)
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
            if (lobbyLogic.partyLogic.GetIsLocalPlayerInParty())
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
            if (result.Error.Code == ErrorCode.PersonalChatReceiverNotFound)
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
            if (result.Error.Code == ErrorCode.PartyChatPartyNotFound)
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

    /// <summary>
    /// Callback on SendPartyChat
    /// Refresh UI chat on success
    /// </summary>
    /// <param name="result"> result callback </param>
    private void OnSendChannelChat(Result result)
    {
        if (result.IsError)
        {
            Debug.Log("Send channel chat failed:" + result.Error.Message);
            Debug.Log("Send channel chat response:" + result.Error.Code);
            if (result.Error.Code == ErrorCode.ChannelSendChatChannelError)
            {
                WriteWarningInChatBox("Send Chat Channel Error");
                UIHandlerLobbyComponent.messageInputField.text = string.Empty;
            }
        }
        else
        {
            Debug.Log("Send channel chat succesful");
            chatBoxList[activePlayerChatUserId].sender.Add(accelByteManager.AuthLogic.GetUserData().userId);
            chatBoxList[activePlayerChatUserId].message.Add(UIHandlerLobbyComponent.messageInputField.text);
            UIHandlerLobbyComponent.messageInputField.text = string.Empty;
            
            //RefreshChatBoxUI();
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
    private void OnPersonalChatReceived(Result<ChatMessage> result)
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
    private void OnPartyChatReceived(Result<ChatMessage> result)
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
    /// <summary>
    /// Callback from ChannelChat event
    /// Triggered if the player recieved a Channel chat message
    /// Update the chat UI on success
    /// </summary>
    /// <param name="result"> result callback </param>
    private void OnChannelChatReceived(Result<ChannelChatMessage> result)
    {
        if (result.IsError)
        {
            Debug.Log("Get party chat failed:" + result.Error.Message);
            Debug.Log("Get party chat Response Code: " + result.Error.Code);
        }
        else
        {
            receivedChannelMessage = result.Value;
            isReceivedChannelMessage = true;
        }
    }
    /// <summary>
    /// Callback from Join Channel event
    /// Triggered if the player joined channel
    /// Update the chat UI on success
    /// </summary>
    /// <param name="result"> result callback </param>
    private void OnJoinChannel(Result<ChatChannelSlug> result)
    {
        if (result.IsError)
        {
            Debug.Log("join chat channel failed:" + result.Error.Message);
            Debug.Log("join chat channel Response Code: " + result.Error.Code);
        }
        else
        {
            if (channel == null)
            {
                channel = new ChatChannelSlug();
            } 
            channel = result.Value;
            activePlayerChatUserId = channel.channelSlug;
            if (!chatBoxList.ContainsKey(activePlayerChatUserId))
            {
                chatBoxList.Add(activePlayerChatUserId, new ChatData(activePlayerChatUserId, new List<string>(), new List<string>()));
            }
        }
    }
    #endregion
}
