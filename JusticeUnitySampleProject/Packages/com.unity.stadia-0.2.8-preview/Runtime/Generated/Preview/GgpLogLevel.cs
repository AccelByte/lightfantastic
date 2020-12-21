#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpLogLevel
    {
        public GgpLogLevel(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
