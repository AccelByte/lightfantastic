#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpTextEvent
    {
        public GgpPlayerId player;
        public string text;
        public GgpTextAction action;
        public GgpTextCursorActionModifier cursor_action_modifier;
    }
}
#endif
