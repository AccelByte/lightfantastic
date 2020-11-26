#if ENABLE_GGP_PREVIEW_APIS
using System;

namespace Unity.StadiaWrapper
{
    public partial struct GgpGameBusMessage
    {
        public GgpGameBusMessageNamespace message_namespace;
        public GgpGameBusMessageTypeId message_type_id;
        public IntPtr message_buffer;
        public long message_buffer_size;
    }
}
#endif
