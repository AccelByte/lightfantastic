#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpSocialGraphChangedEvent
    {
        public GgpPlayerId player_id;
        public GgpSocialGraphChangeType type;
    }
}
#endif
