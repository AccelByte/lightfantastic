#if !ENABLE_GGP_PREVIEW_APIS
using System;

namespace Unity.StadiaWrapper
{
    public partial struct GgpStreamSubscriptionCreateRequest
    {
        public string connection_id;
        public string stream_name;
        public GgpVulkanInstance vulkan_instance;
        public GgpVulkanPhysicalDevice physical_device;
        public GgpVulkanDevice device;
        public IntPtr allocator;
        public GgpVideoResolution resolution;
        public GgpSurfaceFormat image_format;
        public int video_decode_cpu_threads;
    }
}
#endif
