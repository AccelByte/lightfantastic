#if !ENABLE_GGP_PREVIEW_APIS
using System;

namespace Unity.StadiaWrapper
{
    public partial struct GgpLobbyFilters
    {
        public string content_type;
        public IntPtr property_filters;
        public long property_filters_count;
        public IntPtr player_filters;
        public long player_filters_count;
    }
}
#endif
