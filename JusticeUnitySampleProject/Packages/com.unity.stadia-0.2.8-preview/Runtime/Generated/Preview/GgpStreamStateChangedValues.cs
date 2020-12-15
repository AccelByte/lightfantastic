#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public enum GgpStreamStateChangedValues
    {
        kGgpStreamStateChanged_Unknown = 0,
        kGgpStreamStateChanged_Invalid = 0,
        kGgpStreamStateChanged_Starting = 1,
        kGgpStreamStateChanged_Initial = 1,
        kGgpStreamStateChanged_Started = 2,
        kGgpStreamStateChanged_Exited = 3,
        kGgpStreamStateChanged_Suspended = 4,
        kGgpStreamStateChanged_Unexpected = 4,
    }
}
#endif
