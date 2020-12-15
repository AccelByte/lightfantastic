#if !ENABLE_GGP_PREVIEW_APIS
using System.Runtime.InteropServices;

namespace Unity.StadiaWrapper
{
    public partial struct GgpGamerStatInfo
    {
        public GgpPlayerId player_id;
        
        internal unsafe struct gamer_stat_id_fixed
        {
            private unsafe fixed byte gamer_stat_id_val[totalsize];
            private const int totalsize = 40;

            public unsafe byte[] Data
            {
                get
                {
                    byte[] retval = new byte[totalsize];
                    GetDataToExistingBuffer(ref retval);
                    return retval;
                }
                set
                {
                    int passedSize = value.Length;

                    if (passedSize != totalsize)
                    {
                        throw new System.ArgumentException($"The passed data is not of the correct size. Expected byte[{totalsize}].");
                    }

                    for (int i = 0; i < passedSize; ++i)
                    {
                        gamer_stat_id_val[i] = value[i];
                    }
                }
            }

            public unsafe string DataAsString
            {
                get
                {
                    System.Collections.Generic.List<byte> list = new System.Collections.Generic.List<byte>(totalsize);

                    for (int i = 0; i < totalsize; ++i)
                    {
                        if (gamer_stat_id_val[i] == 0)
                            break;

                        list.Add(gamer_stat_id_val[i]);
                    }

                    return System.Text.Encoding.UTF8.GetString(list.ToArray());
                }
                set
                {
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value);
                    int passedSize = bytes.Length;

                    if (passedSize >= totalsize)
                    {
                        throw new System.ArgumentException($"The passed string is too long. The size in UTF8 must be a maximum of {totalsize - 1} bytes.");
                    }

                    for (int i = 0; i < passedSize; ++i)
                    {
                        gamer_stat_id_val[i] = bytes[i];
                    }

                    gamer_stat_id_val[bytes.Length] = 0;
                }
            }

            public unsafe void GetDataToExistingBuffer(ref byte[] data)
            {
                if (data.Length != totalsize)
                {
                    throw new System.ArgumentException($"The target buffer is not of the correct size. Expected byte[{totalsize}].");
                }

                for (int i = 0; i < totalsize; ++i)
                {
                    data[i] = gamer_stat_id_val[i];
                }
            }
        }

        
        private gamer_stat_id_fixed gamer_stat_id_data;

        public byte[] gamer_stat_id
        {
            get { return gamer_stat_id_data.Data; }
            set { gamer_stat_id_data.Data = value; }
        }

        public string gamer_stat_idAsString
        {
            get { return gamer_stat_id_data.DataAsString; }
            set { gamer_stat_id_data.DataAsString = value; }
        }

        public void gamer_stat_idToExistingBuffer(ref byte[] data)
        {
            gamer_stat_id_data.GetDataToExistingBuffer(ref data);
        }

        [MarshalAs(UnmanagedType.U1)] public bool visible;
        
        internal unsafe struct name_fixed
        {
            private unsafe fixed byte name_val[totalsize];
            private const int totalsize = 128;

            public unsafe byte[] Data
            {
                get
                {
                    byte[] retval = new byte[totalsize];
                    GetDataToExistingBuffer(ref retval);
                    return retval;
                }
                set
                {
                    int passedSize = value.Length;

                    if (passedSize != totalsize)
                    {
                        throw new System.ArgumentException($"The passed data is not of the correct size. Expected byte[{totalsize}].");
                    }

                    for (int i = 0; i < passedSize; ++i)
                    {
                        name_val[i] = value[i];
                    }
                }
            }

            public unsafe string DataAsString
            {
                get
                {
                    System.Collections.Generic.List<byte> list = new System.Collections.Generic.List<byte>(totalsize);

                    for (int i = 0; i < totalsize; ++i)
                    {
                        if (name_val[i] == 0)
                            break;

                        list.Add(name_val[i]);
                    }

                    return System.Text.Encoding.UTF8.GetString(list.ToArray());
                }
                set
                {
                    byte[] bytes = System.Text.Encoding.UTF8.GetBytes(value);
                    int passedSize = bytes.Length;

                    if (passedSize >= totalsize)
                    {
                        throw new System.ArgumentException($"The passed string is too long. The size in UTF8 must be a maximum of {totalsize - 1} bytes.");
                    }

                    for (int i = 0; i < passedSize; ++i)
                    {
                        name_val[i] = bytes[i];
                    }

                    name_val[bytes.Length] = 0;
                }
            }

            public unsafe void GetDataToExistingBuffer(ref byte[] data)
            {
                if (data.Length != totalsize)
                {
                    throw new System.ArgumentException($"The target buffer is not of the correct size. Expected byte[{totalsize}].");
                }

                for (int i = 0; i < totalsize; ++i)
                {
                    data[i] = name_val[i];
                }
            }
        }

        
        private name_fixed name_data;

        public byte[] name
        {
            get { return name_data.Data; }
            set { name_data.Data = value; }
        }

        public string nameAsString
        {
            get { return name_data.DataAsString; }
            set { name_data.DataAsString = value; }
        }

        public void nameToExistingBuffer(ref byte[] data)
        {
            name_data.GetDataToExistingBuffer(ref data);
        }

        public GgpGamerStatDataType data_type;
        public GgpGamerStatValue value;
    }
}
#endif
