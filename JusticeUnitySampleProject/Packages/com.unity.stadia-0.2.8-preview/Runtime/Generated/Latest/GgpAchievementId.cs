#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpAchievementId
    {
        public GgpAchievementId(ulong value)
        {
            Value = value;
        }

        public ulong Value;
    }
}
#endif
