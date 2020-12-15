#if ENABLE_GGP_PREVIEW_APIS
namespace Unity.StadiaWrapper
{
    public partial struct GgpStreamInfo
    {
        public GgpVideoResolution video_resolution;
        public GgpSurfaceFormat surface_format;
        
        internal unsafe struct pulse_audio_device_fixed
        {
            private unsafe fixed byte pulse_audio_device_val[totalsize];
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
                        pulse_audio_device_val[i] = value[i];
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
                        if (pulse_audio_device_val[i] == 0)
                            break;

                        list.Add(pulse_audio_device_val[i]);
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
                        pulse_audio_device_val[i] = bytes[i];
                    }

                    pulse_audio_device_val[bytes.Length] = 0;
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
                    data[i] = pulse_audio_device_val[i];
                }
            }
        }

        
        private pulse_audio_device_fixed pulse_audio_device_data;

        public byte[] pulse_audio_device
        {
            get { return pulse_audio_device_data.Data; }
            set { pulse_audio_device_data.Data = value; }
        }

        public string pulse_audio_deviceAsString
        {
            get { return pulse_audio_device_data.DataAsString; }
            set { pulse_audio_device_data.DataAsString = value; }
        }

        public void pulse_audio_deviceToExistingBuffer(ref byte[] data)
        {
            pulse_audio_device_data.GetDataToExistingBuffer(ref data);
        }

    }
}
#endif
