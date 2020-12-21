#if ENABLE_GGP_PREVIEW_APIS
using System.Runtime.InteropServices;

namespace Unity.StadiaWrapper
{
    public partial struct GgpTouchHotplugEvent
    {
        public GgpTouch touch_device;
        [MarshalAs(UnmanagedType.U1)] public bool connected;
    }
}
#endif
