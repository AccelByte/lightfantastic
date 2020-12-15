#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpPixelDensity
    {
        public GgpPixelDensity(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
