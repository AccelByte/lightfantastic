#if !ENABLE_GGP_PREVIEW_APIS
using System.Runtime.InteropServices;

namespace Unity.StadiaWrapper
{
    public partial struct GgpMicAudioStateChangedEvent
    {
        public GgpPlayerId player_id;
        public GgpMicAudioAccessState access;
        public GgpMicAudioMuteState mute;
        [MarshalAs(UnmanagedType.U1)] public bool listening;
    }
}
#endif
