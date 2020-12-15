#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpEntitlementChangedEvent
    {
        public GgpEntitlementChangedEventType event_type;
        public GgpEntitlement entitlement;
    }
}
#endif
