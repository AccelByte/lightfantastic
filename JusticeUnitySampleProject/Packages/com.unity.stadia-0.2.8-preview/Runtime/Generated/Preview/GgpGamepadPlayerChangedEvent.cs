#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpGamepadPlayerChangedEvent
    {
        public GgpGamepad gamepad;
        public GgpPlayerId player_id;
    }
}
#endif
