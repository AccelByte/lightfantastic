#if ENABLE_GGP_PREVIEW_APIS
using System.Runtime.InteropServices;

namespace Unity.StadiaWrapper
{
    public partial struct GgpTextCapabilities
    {
        [MarshalAs(UnmanagedType.U1)] public bool has_text_input_device;
    }
}
#endif
