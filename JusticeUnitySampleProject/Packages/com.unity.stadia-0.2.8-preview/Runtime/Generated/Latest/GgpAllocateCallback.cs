#if !ENABLE_GGP_PREVIEW_APIS
using System;
using System.Runtime.InteropServices;

namespace Unity.StadiaWrapper
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr GgpAllocateCallback(ref GgpAllocationInfo info, long alignment, long size, IntPtr user_data);
}
#endif
