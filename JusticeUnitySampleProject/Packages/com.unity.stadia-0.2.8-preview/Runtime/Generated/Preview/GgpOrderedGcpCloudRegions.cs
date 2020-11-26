#if ENABLE_GGP_PREVIEW_APIS
using System;

namespace Unity.StadiaWrapper
{
    public partial struct GgpOrderedGcpCloudRegions
    {
        public IntPtr ordered_regions;
        public long ordered_regions_count;
        public IntPtr unknown_regions;
        public long unknown_regions_count;
    }
}
#endif
