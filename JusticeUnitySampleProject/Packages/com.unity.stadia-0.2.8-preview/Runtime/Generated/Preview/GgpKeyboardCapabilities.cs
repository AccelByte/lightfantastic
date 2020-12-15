#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpKeyboardCapabilities
    {
        
        internal unsafe struct keymap_fixed
        {
            private unsafe fixed byte keymap_val[totalsize];
            private const int totalsize = 256 * 16;
            private const int chunksize = 16;
            private const int chunks = 256;

            public unsafe byte[,] Data
            {
                get
                {
                    byte[,] retval = new byte[totalsize / chunksize, chunksize];
                    GetDataToExistingBuffer(ref retval);
                    return retval;
                }
                set
                {
                    int passedChunks = value.GetLength(0);
                    int passedChunkSize = value.GetLength(1);

                    if(passedChunks != chunks || passedChunkSize != chunksize)
                    {
                        throw new System.ArgumentException($"The passed data is not of the correct size.Expected byte[{ chunks}, { chunksize}].");
                    }

                    int index = 0;

                    for (int i = 0; i < chunks; ++i)
                    {
                        for(int j = 0; j < chunksize; ++j)
                        {
                            keymap_val[index ++] = value[i, j];
                        }
                    }
                }
            }

            public unsafe void GetDataToExistingBuffer(ref byte[,] data)
            {
                int passedChunks = data.GetLength(0);
                int passedChunkSize = data.GetLength(1);

                if (passedChunks != chunks || passedChunkSize != chunksize)
                {
                    throw new System.ArgumentException($"The target buffer is not of the correct size. Expected byte[{chunks}, {chunksize}].");
                }

                int index = 0;

                for (int i = 0; i < chunks; ++i)
                {
                    for(int j = 0; j < chunksize; ++j)
                    {
                        data[i, j] = keymap_val[index ++];
                    }
                }
            }
        }

        
        private keymap_fixed keymap_data;

        public byte[,] keymap
        {
            get { return keymap_data.Data; }
            set { keymap_data.Data = value; }
        }

        public void keymapToExistingBuffer(ref byte[,] data)
        {
            keymap_data.GetDataToExistingBuffer(ref data);
        }

    }
}
#endif
