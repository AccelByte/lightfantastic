#if !ENABLE_GGP_PREVIEW_APIS
using System;

namespace Unity.StadiaWrapper
{
    public partial struct GgpVulkanCommandBuffer
    {
        public GgpVulkanCommandBuffer(IntPtr pointer)
        {
            Pointer = pointer;
        }

        public IntPtr Pointer;
    }
}
#endif
