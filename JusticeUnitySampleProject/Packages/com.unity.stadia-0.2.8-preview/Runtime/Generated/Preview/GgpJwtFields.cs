#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpJwtFields
    {
        public GgpJwtFields(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
