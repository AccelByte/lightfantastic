#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpVulkanColorSpace
    {
        public GgpVulkanColorSpace(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
