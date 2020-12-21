#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpProfileAvatarId
    {
        public GgpProfileAvatarId(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
