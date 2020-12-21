#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpProductItem
    {
        public GgpPlayerId player_id;
        
        internal unsafe struct product_id_fixed
        {
            private unsafe fixed byte product_id_val[totalsize];
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
                        product_id_val[i] = value[i];
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
                        if (product_id_val[i] == 0)
                            break;

                        list.Add(product_id_val[i]);
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
                        product_id_val[i] = bytes[i];
                    }

                    product_id_val[bytes.Length] = 0;
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
                    data[i] = product_id_val[i];
                }
            }
        }

        
        private product_id_fixed product_id_data;

        public byte[] product_id
        {
            get { return product_id_data.Data; }
            set { product_id_data.Data = value; }
        }

        public string product_idAsString
        {
            get { return product_id_data.DataAsString; }
            set { product_id_data.DataAsString = value; }
        }

        public void product_idToExistingBuffer(ref byte[] data)
        {
            product_id_data.GetDataToExistingBuffer(ref data);
        }

        public GgpProductItemPrice price;
    }
}
#endif
