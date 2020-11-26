#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpPlayerSaveOpenMode
    {
        public GgpPlayerSaveOpenMode(int value)
        {
            Value = value;
        }

        public int Value;
    }
}
#endif
