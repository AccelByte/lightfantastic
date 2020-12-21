#if ENABLE_GGP_PREVIEW_APIS
using System;

namespace Unity.StadiaWrapper
{
    public partial struct GgpOpenInviteState
    {
        public int total_player_slots;
        public int player_slots_filled;
        public IntPtr stadia_players;
        public long stadia_players_count;
    }
}
#endif
