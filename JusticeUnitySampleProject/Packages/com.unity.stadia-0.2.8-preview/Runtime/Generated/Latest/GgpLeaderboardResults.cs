#if !ENABLE_GGP_PREVIEW_APIS
using System;

namespace Unity.StadiaWrapper
{
    public partial struct GgpLeaderboardResults
    {
        public long total_entry_count;
        public long entry_count;
        public IntPtr entries;
    }
}
#endif
