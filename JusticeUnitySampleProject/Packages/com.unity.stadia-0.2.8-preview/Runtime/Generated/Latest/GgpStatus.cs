#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpStatus
    {
        public GgpStatusCode status_code;
        public GgpStatusMessageToken message_token;
    }
}
#endif
