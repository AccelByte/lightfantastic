#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpStartupDataType
    {
        public GgpStartupDataType(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
