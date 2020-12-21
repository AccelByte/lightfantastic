#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpGameBusMessageScope
    {
        public GgpGameBusMessageScope(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
