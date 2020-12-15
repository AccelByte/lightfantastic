#if !ENABLE_GGP_PREVIEW_APIS
using System;

namespace Unity.StadiaWrapper
{
    public partial struct GgpInitializeSettings
    {
        public IntPtr user_data;
        public GgpAllocateCallback allocate_callback;
        public GgpDeallocateCallback deallocate_callback;
    }
}
#endif
