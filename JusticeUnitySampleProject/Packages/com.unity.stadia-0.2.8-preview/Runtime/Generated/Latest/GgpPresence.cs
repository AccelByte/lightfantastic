#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpPresence
    {
        public GgpPlayerId player_id;
        public GgpPresenceAvailability availability;
    }
}
#endif
