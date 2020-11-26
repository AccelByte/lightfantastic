#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpPlayerSavePaths
    {
        
        internal unsafe struct data_path_fixed
        {
            private unsafe fixed byte data_path_val[totalsize];
            private const int totalsize = 1024;

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
                        data_path_val[i] = value[i];
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
                        if (data_path_val[i] == 0)
                            break;

                        list.Add(data_path_val[i]);
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
                        data_path_val[i] = bytes[i];
                    }

                    data_path_val[bytes.Length] = 0;
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
                    data[i] = data_path_val[i];
                }
            }
        }

        
        private data_path_fixed data_path_data;

        public byte[] data_path
        {
            get { return data_path_data.Data; }
            set { data_path_data.Data = value; }
        }

        public string data_pathAsString
        {
            get { return data_path_data.DataAsString; }
            set { data_path_data.DataAsString = value; }
        }

        public void data_pathToExistingBuffer(ref byte[] data)
        {
            data_path_data.GetDataToExistingBuffer(ref data);
        }

    }
}
#endif
