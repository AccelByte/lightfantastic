#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpItemType
    {
        public GgpItemType(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
