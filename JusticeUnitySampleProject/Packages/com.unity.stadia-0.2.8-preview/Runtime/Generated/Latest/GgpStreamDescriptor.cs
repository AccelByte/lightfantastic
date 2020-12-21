#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpStreamDescriptor
    {
        public GgpStreamDescriptor(uint value)
        {
            Value = value;
        }

        public uint Value;
    }
}
#endif
