#if ENABLE_GGP_PREVIEW_APIS
using System;

namespace Unity.StadiaWrapper
{
    public partial struct GgpLeaderboardSettings
    {
        public string gamer_stat_namespace;
        public string gamer_stat_id;
        public GgpLeaderboardType leaderboard_type;
        public GgpLeaderboardSortOrder sort_order;
        public IntPtr player_ids;
        public long player_ids_count;
    }
}
#endif
