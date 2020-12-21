#if ENABLE_GGP_PREVIEW_APIS
using System.Runtime.InteropServices;

namespace Unity.StadiaWrapper
{
    [StructLayout(LayoutKind.Explicit)]
    public partial struct GgpGameStateAttributeValue
    {
        [FieldOffset(0)]
    public long integer_value;
        [FieldOffset(0)]
    public double double_value;
        [FieldOffset(0)]
    public GgpMicroseconds duration_value;
        [FieldOffset(0)]
    public System.IntPtr localized_string_value;
        [FieldOffset(0)]
    public System.IntPtr non_localized_string_value;
    }
}
#endif
