#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpMultiplayerInviteId
    {
        public GgpMultiplayerInviteId(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
