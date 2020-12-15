#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpLobbyState
    {
        public GgpLobbyState(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
