#if ENABLE_GGP_PREVIEW_APIS
using System;
using System.Runtime.InteropServices;

namespace Unity.StadiaWrapper
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void GgpEntitlementChangedHandler(ref GgpEntitlementChangedEvent @event, IntPtr user_data);
}
#endif
