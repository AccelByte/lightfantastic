#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpLeaderboardEntry
    {
        public GgpPlayerId player_id;
        public long rank;
        public GgpGamerStatDataType data_type;
        public GgpLeaderboardEntryValue value;
    }
}
#endif
