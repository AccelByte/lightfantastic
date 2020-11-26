#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpGameEventAttribute
    {
        public string attribute_type_id;
        public GgpGameEventAttributeValueType value_type;
        public GgpGameEventAttributeValue value;
    }
}
#endif
