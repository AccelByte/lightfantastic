#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpPresenceAvailability
    {
        public GgpPresenceAvailability(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
