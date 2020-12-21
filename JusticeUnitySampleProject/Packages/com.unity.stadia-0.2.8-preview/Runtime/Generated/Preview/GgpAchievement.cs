#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpAchievement
    {
        public GgpAchievement(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
