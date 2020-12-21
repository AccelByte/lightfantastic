#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpNetworkTopologyId
    {
        public GgpNetworkTopologyId(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
