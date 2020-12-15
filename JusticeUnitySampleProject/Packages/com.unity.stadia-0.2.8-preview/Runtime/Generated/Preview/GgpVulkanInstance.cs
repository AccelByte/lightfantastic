#if ENABLE_GGP_PREVIEW_APIS
using System;

namespace Unity.StadiaWrapper
{
    public partial struct GgpVulkanInstance
    {
        public GgpVulkanInstance(IntPtr pointer)
        {
            Pointer = pointer;
        }

        public IntPtr Pointer;
    }
}
#endif
