#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpStatusCode
    {
        public GgpStatusCode(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
