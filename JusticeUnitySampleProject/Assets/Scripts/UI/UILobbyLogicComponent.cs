using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILobbyLogicComponent : MonoBehaviour
{
    public ScrollRect friendScrollView;
    public Transform friendScrollContent;
    public Transform friendPrefab;
    public InputField emailToFind;
    public ScrollRect friendSearchScrollView;
    public Transform friendSearchScrollContent;
    public Transform friendSearchPrefab;
    public Transform friendInvitePrefab;
    public Transform sentInvitePrefab;

    // Party & matchmaking
    public Transform matchmakingStatus;
    public Transform popupPartyInvitation;
    public Transform popupMatchConfirmation;
    public Transform popupPartyControl;
    public Transform[] partyMemberButtons;
    public Transform ChatTextbox;
    public Transform matchmakingBoard;

    // Notification
    public Text generalNotificationTitle;
    public Text generalNotificationText;
    public Text incomingFriendNotificationTitle;
    public InvitationPrefab invite;

    // Buttons
    public Button logoutButton;
    public Button findMatchButton;

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
}
