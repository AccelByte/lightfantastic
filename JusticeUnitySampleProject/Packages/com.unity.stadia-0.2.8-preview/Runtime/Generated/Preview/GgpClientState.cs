#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public enum GgpClientState
    {
        kGgpClientState_Unknown = 0,
        kGgpClientState_Invalid = 0,
        kGgpClientState_Streaming = 1,
        kGgpClientState_Exited = 2,
        kGgpClientState_Waiting = 4,
        kGgpClientState_Disconnected = 5,
    }
}
#endif
