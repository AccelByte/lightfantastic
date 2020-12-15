#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpTm
    {
        public sbyte tm_sec;
        public sbyte tm_min;
        public sbyte tm_hour;
        public sbyte tm_mday;
        public sbyte tm_mon;
        public sbyte tm_wday;
        public short tm_year;
        public short tm_yday;
        public sbyte tm_isdst;
    }
}
#endif
