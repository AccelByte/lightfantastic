#if !ENABLE_GGP_PREVIEW_APIS
using System.Runtime.InteropServices;

namespace Unity.StadiaWrapper
{
    [StructLayout(LayoutKind.Explicit)]
    public partial struct GgpLobbyPropertyValue
    {
        
        internal unsafe struct string_value_fixed
        {
            private unsafe fixed byte string_value_val[totalsize];
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
                        string_value_val[i] = value[i];
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
                        if (string_value_val[i] == 0)
                            break;

                        list.Add(string_value_val[i]);
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
                        string_value_val[i] = bytes[i];
                    }

                    string_value_val[bytes.Length] = 0;
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
                    data[i] = string_value_val[i];
                }
            }
        }

        [FieldOffset(0)]
        private string_value_fixed string_value_data;

        public byte[] string_value
        {
            get { return string_value_data.Data; }
            set { string_value_data.Data = value; }
        }

        public string string_valueAsString
        {
            get { return string_value_data.DataAsString; }
            set { string_value_data.DataAsString = value; }
        }

        public void string_valueToExistingBuffer(ref byte[] data)
        {
            string_value_data.GetDataToExistingBuffer(ref data);
        }

        [FieldOffset(0)]
    public long int64_value;
        [FieldOffset(0)]
    public double double_value;
    }
}
#endif
