#if !ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpAchievementInfo
    {
        public GgpPlayerId player_id;
        public GgpAchievementId achievement_id;
        public GgpAchievementType type;
        
        internal unsafe struct name_fixed
        {
            private unsafe fixed byte name_val[totalsize];
            private const int totalsize = 256;

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

        
        internal unsafe struct description_fixed
        {
            private unsafe fixed byte description_val[totalsize];
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
                        description_val[i] = value[i];
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
                        if (description_val[i] == 0)
                            break;

                        list.Add(description_val[i]);
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
                        description_val[i] = bytes[i];
                    }

                    description_val[bytes.Length] = 0;
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
                    data[i] = description_val[i];
                }
            }
        }

        
        private description_fixed description_data;

        public byte[] description
        {
            get { return description_data.Data; }
            set { description_data.Data = value; }
        }

        public string descriptionAsString
        {
            get { return description_data.DataAsString; }
            set { description_data.DataAsString = value; }
        }

        public void descriptionToExistingBuffer(ref byte[] data)
        {
            description_data.GetDataToExistingBuffer(ref data);
        }

        public int current_progress;
        public GgpTimestampMicroseconds completion_timestamp;
    }
}
#endif
