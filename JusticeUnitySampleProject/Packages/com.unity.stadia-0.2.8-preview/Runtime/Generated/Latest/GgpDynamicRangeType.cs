#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpDynamicRangeType
    {
        public GgpDynamicRangeType(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
