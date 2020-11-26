#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public enum GgpDeprecatedMultiplayerConstants
    {
        kGgpMaxLobbyListCount = 50,
        kGgpMultiplayerMaxLobbyListCount = 50,
        kGgpLobbyStateInvalid = 0,
        kGgpLobbyStateDefault = 1,
        kGgpLobbyStateReady = 2,
        kGgpLobbyStateFull = 3,
        kGgpLobbyStateCompleted = 4,
        kGgpLobbyStateAbandoned = 5,
        kGgpLobbyVisibilityUnspecified = 0,
        kGgpLobbyVisibilityPrivate = 1,
        kGgpLobbyVisibilityPublicBrowsing = 3,
        kGgpLobbyVisibilityMatchmaking = 4,
        kGgpLobbyVisibilityMatchmakingStable = 5,
        kGgpLobbyPropertyTypeInvalid = 0,
        kGgpLobbyPropertyTypeString = 1,
        kGgpLobbyPropertyTypeInt = 2,
        kGgpLobbyPropertyTypeDouble = 3,
        kGgpLobbyPropertyFilterOperatorInvalid = 0,
        kGgpLobbyPropertyFilterOperatorEquals = 1,
        kGgpLobbyPropertyFilterOperatorNotEquals = 2,
        kGgpLobbyPropertyFilterOperatorGreaterThan = 3,
        kGgpLobbyPropertyFilterOperatorGreaterThanOrEquals = 4,
        kGgpLobbyPropertyFilterOperatorLessThan = 5,
        kGgpLobbyPropertyFilterOperatorLessThanOrEquals = 6,
        kGgpMultiplayerAddressType_Invalid = 0,
        kGgpMultiplayerAddressInvalid = 0,
        kGgpMultiplayerAddressIpV4 = 1,
        kGgpMultiplayerAddressIpV6 = 2,
        kGgpMultiplayerInviteTypeInvalid = 0,
        kGgpMultiplayerInviteSent = 1,
        kGgpMultiplayerInviteAccepted = 2,
        kGgpMultiplayerInviteAcceptedByInvitee = 3,
        kGgpMultiplayerInviteRemovedFromInvite = 4,
    }
}
#endif
