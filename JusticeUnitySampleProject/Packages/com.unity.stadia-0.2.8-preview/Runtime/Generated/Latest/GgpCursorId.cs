#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpCursorId
    {
        public GgpCursorId(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
