#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpSocialGraphChangeType
    {
        public GgpSocialGraphChangeType(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
