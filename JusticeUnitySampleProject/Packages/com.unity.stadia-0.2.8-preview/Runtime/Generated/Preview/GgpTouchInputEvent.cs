#if ENABLE_GGP_PREVIEW_APIS
using System;

namespace Unity.StadiaWrapper
{
    public partial struct GgpTouchInputEvent
    {
        public GgpTouch touch_device;
        public GgpTouchInputEventType type;
        public IntPtr active_pointers;
        public long active_pointers_count;
        public GgpInputToken input_token;
    }
}
#endif
