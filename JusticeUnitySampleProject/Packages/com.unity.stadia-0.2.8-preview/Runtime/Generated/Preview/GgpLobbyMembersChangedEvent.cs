#if ENABLE_GGP_PREVIEW_APIS
using System;

namespace Unity.StadiaWrapper
{
    public partial struct GgpLobbyMembersChangedEvent
    {
        public GgpPlayerId player_id;
        public GgpLobby lobby;
        public IntPtr members_changed;
        public long members_changed_count;
        public IntPtr members_added;
        public long members_added_count;
        public IntPtr members_removed;
        public long members_removed_count;
    }
}
#endif
