#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public enum GgpJwtFieldValues
    {
        kGgpJwtField_None = 0,
        kGgpJwtField_PurchaseCountry = 0x0002,
        kGgpJwtField_CurrentCountry = 0x0004,
        kGgpJwtField_StableSessionId = 0x0008,
        kGgpJwtField_InstanceIp = 0x0010,
        kGgpJwtField_RestrictTextChat = 0x0020,
        kGgpJwtField_RestrictVoiceChat = 0x0040,
        kGgpJwtField_RestrictMultiplayer = 0x0080,
        kGgpJwtField_RestrictStreamConnect = 0x0100,
        kGgpJwtField_StadiaName = 0x0200,
    }
}
#endif
