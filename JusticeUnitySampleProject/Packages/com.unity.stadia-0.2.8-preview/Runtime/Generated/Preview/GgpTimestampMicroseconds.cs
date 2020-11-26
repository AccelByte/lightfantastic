#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpTimestampMicroseconds
    {
        public GgpTimestampMicroseconds(long value)
        {
            Value = value;
        }

        public long Value;
    }
}
#endif
