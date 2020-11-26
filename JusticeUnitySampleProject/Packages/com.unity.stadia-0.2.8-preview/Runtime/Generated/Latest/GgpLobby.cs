#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpLobby
    {
        public GgpLobby(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
