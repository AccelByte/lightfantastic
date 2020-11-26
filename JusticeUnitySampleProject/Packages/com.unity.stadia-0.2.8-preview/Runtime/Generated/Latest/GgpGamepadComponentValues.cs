#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public enum GgpGamepadComponentValues
    {
        kGgpGamepadComponent_None = 0,
        kGgpGamepadComponent_Buttons = 0x01,
        kGgpGamepadComponent_AxisLeftX = 0x02,
        kGgpGamepadComponent_AxisLeftY = 0x04,
        kGgpGamepadComponent_AxisRightX = 0x08,
        kGgpGamepadComponent_AxisRightY = 0x10,
        kGgpGamepadComponent_TriggerLeft = 0x20,
        kGgpGamepadComponent_TriggerRight = 0x40,
    }
}
#endif
