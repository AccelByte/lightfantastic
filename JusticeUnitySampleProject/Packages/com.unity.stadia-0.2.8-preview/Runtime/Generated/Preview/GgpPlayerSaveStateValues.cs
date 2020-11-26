#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public enum GgpPlayerSaveStateValues
    {
        kGgpPlayerSaveState_Invalid = 0,
        kGgpPlayerSaveState_Closed = 1,
        kGgpPlayerSaveState_Read = 3,
        kGgpPlayerSaveState_ReadWrite = 4,
        kGgpPlayerSaveState_Opening = 5,
        kGgpPlayerSaveState_Closing = 6,
    }
}
#endif
