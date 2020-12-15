#if !ENABLE_GGP_PREVIEW_APIS
using System;

namespace Unity.StadiaWrapper
{
    public partial struct GgpClipboardItem
    {
        public GgpClipboardDataTypeMask type;
        public IntPtr data_buffer;
        public long data_buffer_size;
    }
}
#endif
