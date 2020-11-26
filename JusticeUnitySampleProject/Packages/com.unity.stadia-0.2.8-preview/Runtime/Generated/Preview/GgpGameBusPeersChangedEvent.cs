#if ENABLE_GGP_PREVIEW_APIS
using System;

namespace Unity.StadiaWrapper
{
    public partial struct GgpGameBusPeersChangedEvent
    {
        public IntPtr peers_added;
        public long peers_added_count;
        public IntPtr peers_removed;
        public long peers_removed_count;
    }
}
#endif
