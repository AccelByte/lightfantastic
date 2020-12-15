#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpConsentStatus
    {
        public GgpConsentStatus(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
