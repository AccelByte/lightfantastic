#if ENABLE_GGP_PREVIEW_APIS
using System;

namespace Unity.StadiaWrapper
{
    public partial struct GgpVulkanDevice
    {
        public GgpVulkanDevice(IntPtr pointer)
        {
            Pointer = pointer;
        }

        public IntPtr Pointer;
    }
}
#endif
