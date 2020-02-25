using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UILobbyLogicComponent : MonoBehaviour
{
    [Header("Miscellaneous")]
    public ScrollRect friendScrollView;
    public Transform friendScrollContent;
    public Transform friendPrefab;
    public InputField emailToFind;
    public ScrollRect friendSearchScrollView;
    public Transform friendSearchScrollContent;
    public Transform friendSearchPrefab;
    public Transform friendInvitePrefab;
    public Transform sentInvitePrefab;

    [Header("TRANSFORM Party & matchmaking")] 
    public Transform popupPartyInvitation;
    public Transform popupMatchConfirmation;
    public Transform popupPartyControl;
    public Transform[] partyMemberButtons;
    public Transform ChatTextbox;
    public MatchmakingBoardScript matchmakingBoard;
    public PromptPanel matchmakingFailedPromptPanel;

    [Header("Chat")]
    public InputField playerNameInputField;
    public InputField messageInputField;
    public Text chatMessageText;

    [Header("Notification")]
    public Text generalNotificationTitle;
    public Text generalNotificationText;
    public Text incomingFriendNotificationTitle;
    public InvitationPrefab invite;

    [Header("Buttons")]
    public Button logoutButton;
    
    public Button findMatchButton;
    public Button findLocalMatchButton;

    public Button friendsTabButton;
    public Button invitesTabButton;

    public Button searchFriendButton;

    public Button localPlayerButton;
    public Button partyMember1stButton;
    public Button partyMember2ndButton;
    public Button partyMember3rdButton;

    public Button acceptPartyInvitation;
    public Button declinePartyInvitation;

    public Button closePopupPartyButton;
    public Button leaderLeavePartyButton;
    public Button localLeavePartyButton;
    public Button memberKickPartyButton;

    public Button cancelMatchmakingButton;
    
    [Tooltip("MainMenu's MUTLIPAYER Button")]
    public GameObject mainMenuMultiplayerButton;
    
    [Header("Input Fields")]
    public InputField localMatch_IP_inputFields;
    public InputField localMatch_Port_inputFields;
}
