#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpProductItemPriceMicros
    {
        public GgpProductItemPriceMicros(long value)
        {
            Value = value;
        }

        public long Value;
    }
}
#endif
