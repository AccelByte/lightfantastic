#if !ENABLE_GGP_PREVIEW_APIS
using System.Runtime.InteropServices;

namespace Unity.StadiaWrapper
{
    public partial struct GgpGamepadHotplugEvent
    {
        public GgpGamepad gamepad;
        public uint index;
        [MarshalAs(UnmanagedType.U1)] public bool connected;
    }
}
#endif
