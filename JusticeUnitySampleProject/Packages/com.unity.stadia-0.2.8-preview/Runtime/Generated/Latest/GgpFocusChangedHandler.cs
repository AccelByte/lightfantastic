#if !ENABLE_GGP_PREVIEW_APIS
using System;
using System.Runtime.InteropServices;

namespace Unity.StadiaWrapper
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void GgpFocusChangedHandler(ref GgpFocusChangedEvent @event, IntPtr user_data);
}
#endif
