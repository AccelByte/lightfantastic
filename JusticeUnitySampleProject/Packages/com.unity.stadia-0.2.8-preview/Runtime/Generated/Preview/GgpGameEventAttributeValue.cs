#if ENABLE_GGP_PREVIEW_APIS
using System.Runtime.InteropServices;

namespace Unity.StadiaWrapper
{
    [StructLayout(LayoutKind.Explicit)]
    public partial struct GgpGameEventAttributeValue
    {
        [FieldOffset(0)]
    public long int64_value;
        [FieldOffset(0)]
    public double double_value;
        [FieldOffset(0)]
    public System.IntPtr string_value;
    }
}
#endif
