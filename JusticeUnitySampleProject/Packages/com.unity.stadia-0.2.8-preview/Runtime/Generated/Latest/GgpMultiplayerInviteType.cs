#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpMultiplayerInviteType
    {
        public GgpMultiplayerInviteType(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
