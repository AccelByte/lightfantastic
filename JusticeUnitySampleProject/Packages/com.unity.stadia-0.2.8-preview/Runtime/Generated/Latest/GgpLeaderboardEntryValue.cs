#if !ENABLE_GGP_PREVIEW_APIS
using System.Runtime.InteropServices;

namespace Unity.StadiaWrapper
{
    [StructLayout(LayoutKind.Explicit)]
    public partial struct GgpLeaderboardEntryValue
    {
        [FieldOffset(0)]
    public long int64_value;
        [FieldOffset(0)]
    public double double_value;
    }
}
#endif
