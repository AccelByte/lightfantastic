#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpDisplayCapabilities
    {
        public GgpPixelDensity pixel_density;
        public GgpDynamicRange dynamic_range;
        public GgpResolution render_resolution;
    }
}
#endif
