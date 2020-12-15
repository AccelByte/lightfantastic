#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpMultiplayerAddress
    {
        public GgpMultiplayerAddressTypes types;
        public ushort min_port;
        public GgpMultiplayerAddressAddress address;
    }
}
#endif
