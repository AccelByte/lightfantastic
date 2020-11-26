#if !ENABLE_GGP_PREVIEW_APIS
using System.Runtime.InteropServices;

namespace Unity.StadiaWrapper
{
    public partial struct GgpMultiplayerInviteEvent
    {
        public GgpMultiplayerInviteType type;
        public GgpPlayerId player_id;
        public GgpPlayerId invitor_id;
        public GgpMultiplayerInviteId invite_id;
        public GgpMultiplayerInviteContextString context;
        [MarshalAs(UnmanagedType.U1)] public bool queue_to_play;
    }
}
#endif
