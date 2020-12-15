#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public enum GgpClipboardStatusValues
    {
        kGgpClipboardStatus_Invalid = 0,
        kGgpClipboardStatus_NotPresent = 1,
        kGgpClipboardStatus_Allowed = 2,
        kGgpClipboardStatus_Denied = 3,
        kGgpClipboardStatus_Undetermined = 4,
    }
}
#endif
