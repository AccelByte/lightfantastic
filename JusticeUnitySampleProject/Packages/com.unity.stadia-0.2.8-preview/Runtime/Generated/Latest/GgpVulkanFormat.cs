#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpVulkanFormat
    {
        public GgpVulkanFormat(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
