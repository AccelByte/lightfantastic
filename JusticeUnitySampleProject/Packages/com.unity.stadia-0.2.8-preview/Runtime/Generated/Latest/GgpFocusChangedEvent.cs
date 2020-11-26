#if !ENABLE_GGP_PREVIEW_APIS
using System.Runtime.InteropServices;

namespace Unity.StadiaWrapper
{
    public partial struct GgpFocusChangedEvent
    {
        [MarshalAs(UnmanagedType.U1)] public bool has_focus;
    }
}
#endif
