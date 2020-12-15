#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public enum GgpStatusCodeValues
    {
        kGgpStatusCode_Ok = 0,
        kGgpStatusCode_InvalidArgument = 3,
        kGgpStatusCode_FailedPrecondition = 9,
        kGgpStatusCode_DeadlineExceeded = 4,
        kGgpStatusCode_NotFound = 5,
        kGgpStatusCode_AlreadyExists = 6,
        kGgpStatusCode_PermissionDenied = 7,
        kGgpStatusCode_ResourceExhausted = 8,
        kGgpStatusCode_Cancelled = 1,
        kGgpStatusCode_Aborted = 10,
        kGgpStatusCode_Unimplemented = 12,
        kGgpStatusCode_Internal = 13,
        kGgpStatusCode_Unavailable = 14,
    }
}
#endif
