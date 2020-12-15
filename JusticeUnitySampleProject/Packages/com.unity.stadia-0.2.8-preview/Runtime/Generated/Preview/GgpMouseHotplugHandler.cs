#if ENABLE_GGP_PREVIEW_APIS
using System;
using System.Runtime.InteropServices;

namespace Unity.StadiaWrapper
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void GgpMouseHotplugHandler(ref GgpMouseHotplugEvent @event, IntPtr user_data);
}
#endif
