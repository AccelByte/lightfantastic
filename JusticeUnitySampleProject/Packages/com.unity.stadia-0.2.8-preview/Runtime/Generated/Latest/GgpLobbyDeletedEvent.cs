#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpLobbyDeletedEvent
    {
        public GgpPlayerId player_id;
        public GgpLobby lobby;
    }
}
#endif
