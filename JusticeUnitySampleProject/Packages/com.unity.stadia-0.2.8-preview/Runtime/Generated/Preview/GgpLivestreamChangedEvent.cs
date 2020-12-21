#if ENABLE_GGP_PREVIEW_APIS
using System.Runtime.InteropServices;

namespace Unity.StadiaWrapper
{
    public partial struct GgpLivestreamChangedEvent
    {
        public GgpPlayerId player_id;
        [MarshalAs(UnmanagedType.U1)] public bool active;
        [MarshalAs(UnmanagedType.U1)] public bool community_poll_active;
        [MarshalAs(UnmanagedType.U1)] public bool queue_to_play_active;
    }
}
#endif
