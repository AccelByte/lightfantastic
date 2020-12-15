#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpGamerStat
    {
        public GgpGamerStat(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
