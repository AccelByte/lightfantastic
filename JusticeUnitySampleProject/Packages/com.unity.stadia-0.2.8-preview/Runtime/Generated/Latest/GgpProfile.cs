#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpProfile
    {
        public GgpPlayerId player_id;
        
        internal unsafe struct stadia_name_fixed
        {
            private unsafe fixed byte stadia_name_val[totalsize];
            private const int totalsize = 21;

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
                        stadia_name_val[i] = value[i];
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
                        if (stadia_name_val[i] == 0)
                            break;

                        list.Add(stadia_name_val[i]);
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
                        stadia_name_val[i] = bytes[i];
                    }

                    stadia_name_val[bytes.Length] = 0;
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
                    data[i] = stadia_name_val[i];
                }
            }
        }

        
        private stadia_name_fixed stadia_name_data;

        public byte[] stadia_name
        {
            get { return stadia_name_data.Data; }
            set { stadia_name_data.Data = value; }
        }

        public string stadia_nameAsString
        {
            get { return stadia_name_data.DataAsString; }
            set { stadia_name_data.DataAsString = value; }
        }

        public void stadia_nameToExistingBuffer(ref byte[] data)
        {
            stadia_name_data.GetDataToExistingBuffer(ref data);
        }

    }
}
#endif
