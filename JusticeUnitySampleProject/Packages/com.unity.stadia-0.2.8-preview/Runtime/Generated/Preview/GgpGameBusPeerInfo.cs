#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpGameBusPeerInfo
    {
        public GgpGameBusMessageAddress address;
        
        internal unsafe struct purpose_fixed
        {
            private unsafe fixed byte purpose_val[totalsize];
            private const int totalsize = 64;

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
                        purpose_val[i] = value[i];
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
                        if (purpose_val[i] == 0)
                            break;

                        list.Add(purpose_val[i]);
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
                        purpose_val[i] = bytes[i];
                    }

                    purpose_val[bytes.Length] = 0;
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
                    data[i] = purpose_val[i];
                }
            }
        }

        
        private purpose_fixed purpose_data;

        public byte[] purpose
        {
            get { return purpose_data.Data; }
            set { purpose_data.Data = value; }
        }

        public string purposeAsString
        {
            get { return purpose_data.DataAsString; }
            set { purpose_data.DataAsString = value; }
        }

        public void purposeToExistingBuffer(ref byte[] data)
        {
            purpose_data.GetDataToExistingBuffer(ref data);
        }

        public GgpPlayerId node_player_id;
    }
}
#endif
