#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpAllocationType
    {
        public GgpAllocationType(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
