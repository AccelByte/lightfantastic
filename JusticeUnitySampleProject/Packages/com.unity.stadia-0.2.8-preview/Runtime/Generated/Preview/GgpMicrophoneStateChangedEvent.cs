#if ENABLE_GGP_PREVIEW_APIS
using System.Runtime.InteropServices;

namespace Unity.StadiaWrapper
{
    public partial struct GgpMicrophoneStateChangedEvent
    {
        public GgpMicrophone microphone;
        [MarshalAs(UnmanagedType.U1)] public bool is_live;
    }
}
#endif
