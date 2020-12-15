#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpLeaderboard
    {
        public GgpLeaderboard(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
