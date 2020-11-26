#if ENABLE_GGP_PREVIEW_APIS
using System.Runtime.InteropServices;

namespace Unity.StadiaWrapper
{
    public partial struct GgpMouseInputEvent
    {
        public GgpMouse mouse;
        public GgpMouseInputEventType type;
        public GgpMouseCoordinateMode coordinate_mode;
        public int x;
        public int y;
        public GgpMouseButton changed_button;
        public int button_click_count;
        [MarshalAs(UnmanagedType.U1)] public bool button_state_left;
        [MarshalAs(UnmanagedType.U1)] public bool button_state_middle;
        [MarshalAs(UnmanagedType.U1)] public bool button_state_right;
        [MarshalAs(UnmanagedType.U1)] public bool button_state_button4;
        [MarshalAs(UnmanagedType.U1)] public bool button_state_button5;
        public double scroll_x;
        public double scroll_y;
        public int wheel_y_clicks;
        [MarshalAs(UnmanagedType.U1)] public bool modifier_state_control;
        [MarshalAs(UnmanagedType.U1)] public bool modifier_state_shift;
        [MarshalAs(UnmanagedType.U1)] public bool modifier_state_alt;
        public GgpInputToken input_token;
    }
}
#endif
