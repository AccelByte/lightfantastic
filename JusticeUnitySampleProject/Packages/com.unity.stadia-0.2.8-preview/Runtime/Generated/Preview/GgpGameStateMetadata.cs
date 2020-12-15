#if ENABLE_GGP_PREVIEW_APIS
using System;

namespace Unity.StadiaWrapper
{
    public partial struct GgpGameStateMetadata
    {
        public IntPtr type;
        public IntPtr attributes;
        public long attributes_count;
    }
}
#endif
