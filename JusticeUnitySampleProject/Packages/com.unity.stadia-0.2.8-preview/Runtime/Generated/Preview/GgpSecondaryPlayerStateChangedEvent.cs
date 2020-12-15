#if ENABLE_GGP_PREVIEW_APIS
using System.Runtime.InteropServices;

namespace Unity.StadiaWrapper
{
    public partial struct GgpSecondaryPlayerStateChangedEvent
    {
        public GgpPlayerId player_id;
        [MarshalAs(UnmanagedType.U1)] public bool connected;
    }
}
#endif
