#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpGamepadInputEvent
    {
        public GgpGamepad gamepad;
        public GgpGamepadComponentMask changed_components;
        public GgpGamepadButtonMask buttons;
        public int axis_left_x;
        public int axis_left_y;
        public int axis_right_x;
        public int axis_right_y;
        public int trigger_left;
        public int trigger_right;
        public GgpInputToken input_token;
    }
}
#endif
