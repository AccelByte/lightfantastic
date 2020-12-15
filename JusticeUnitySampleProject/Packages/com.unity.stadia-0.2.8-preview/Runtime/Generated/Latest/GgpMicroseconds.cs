#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpMicroseconds
    {
        public GgpMicroseconds(long value)
        {
            Value = value;
        }

        public long Value;
    }
}
#endif
