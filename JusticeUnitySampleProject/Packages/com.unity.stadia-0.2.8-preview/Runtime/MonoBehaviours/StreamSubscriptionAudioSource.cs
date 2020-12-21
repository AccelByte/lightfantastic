using AOT;
using System;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.StadiaWrapper
{
    /// <summary>
    /// An AudioSource for StreamSubscription audio playback.
    /// </summary>
    [AddComponentMenu("")]            // Hide from the Editor Inspector
    [DisallowMultipleComponent]       // OnAudioFilterRead is called once per GameObject
    public class StreamSubscriptionAudioSource : MonoBehaviour
    {
        /// <summary>
        /// Creates a GameObject that has everything needed to play Stadia StreamSubscription audio.
        /// </summary>
        /// <param name="name">Optional name of the GameObject.</param>
        /// <returns>
        /// The StreamSubscriptionAudioSource attached to the GameObject.
        /// </returns>
        public static StreamSubscriptionAudioSource CreateGameObject(string name = "StreamSubscriptionAudio")
        {
            return new GameObject(name).AddComponent<StreamSubscriptionAudioSource>();
        }

        /// <summary>
        /// The volume of the audio source (0.0 to 1.0).
        /// </summary>
        public float Volume
        {
            get => audioSource.volume;
            set => audioSource.volume = value;
        }

        /// <summary>
        /// Mutes the AudioSource. Mute sets the volume=0, Un-Mute restore the original volume.
        /// </summary>
        /// 
        public bool Mute
        {
            get => audioSource.mute;
            set => audioSource.mute = value;
        }
        
        /// <summary>
        /// Sets how much this AudioSource is affected by 3D spatialisation calculations (attenuation, doppler etc). 0.0 makes the sound full 2D, 1.0 makes it full 3D.
        /// </summary>
        public float SpatialBlend
        {
            get => audioSource.spatialBlend;
            set => audioSource.spatialBlend = value;
        }

        /// <summary>
        /// Play StreamSubscription audio.
        /// </summary>
        /// <param name="pulseAudioDeviceName">Name of the StreamSubscription PulseAudio device.</param>
        /// <param name="pulseAudioMainLoopPtr">StreamSourceAudioListener's PulseAudio main loop pointer.</param>
        /// <param name="pulseAudioContextPtr">StreamSourceAudioListener's PulseAudio context pointer</param>
        public void PlayStreamingAudio(string pulseAudioDeviceName, IntPtr pulseAudioMainLoopPtr, IntPtr pulseAudioContextPtr)
        {
            // GameObject must be active in order to run Coroutines and to have Awake called
            if (!gameObject.activeInHierarchy)
            {
                Debug.LogError("GameObject must be active!");
                return;
            }
            
            // Make sure it's not already running
            StopStreamingAudioImmediate();

            // Save the device name as a IntPtr
            pulseAudioDeviceNamePtr = Marshal.StringToHGlobalAnsi(pulseAudioDeviceName);

            // Save the main loop
            mainLoopPtr = pulseAudioMainLoopPtr;

            // Save the context
            contextPtr = pulseAudioContextPtr;

            // Lock the main loop
            pa_threaded_mainloop_lock(mainLoopPtr);
            {
                // Get the source info by way of device name
                pa_operation_unref(pa_context_get_source_info_by_name(contextPtr, pulseAudioDeviceNamePtr, pa_context_source_info_callback, (IntPtr) thisGCHandle));
            }
            // Unlock the main loop
            pa_threaded_mainloop_unlock(mainLoopPtr);
        }

        /// <summary>
        /// Stop audio playback immediately.
        /// </summary>
        public void StopStreamingAudioImmediate()
        {
            // Validate
            if (mainLoopPtr != IntPtr.Zero)
            {
                // Lock the main loop
                pa_threaded_mainloop_lock(mainLoopPtr);
                {
                    // Validate
                    if (streamPtr != IntPtr.Zero)
                    {
                        // Disconnect the stream
                        pa_stream_disconnect(streamPtr);

                        // Clear the callbacks
                        pa_stream_set_state_callback(streamPtr, null, IntPtr.Zero);
                        pa_stream_set_read_callback(streamPtr, null, IntPtr.Zero);

                        // Unref the stream
                        pa_stream_unref(streamPtr);
                        streamPtr = IntPtr.Zero;
                    }
                }
                // Unlock the main loop
                pa_threaded_mainloop_unlock(mainLoopPtr);
                
                // Zero out the context and main loop as we don't own them
                contextPtr = IntPtr.Zero;
                mainLoopPtr = IntPtr.Zero;
            }
            
            // Free the device name
            if (pulseAudioDeviceNamePtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(pulseAudioDeviceNamePtr);
                pulseAudioDeviceNamePtr = IntPtr.Zero;
            }
            
            // Point the memory stream at the start of its buffer but don't release the memory in case of reuse
            lock(memoryStreamLockObject)
            {
                memoryStream.SetLength(0);
            }
            
            // Cleanup the audio source
            if (audioSource != null)
            {
                // Stop the streaming audio source
                audioSource.Stop();
                
                // Free the clip
                audioSource.clip = null;
            }
        }
        
        /// <summary>
        /// Stop audio playback next frame on the main thread.
        /// </summary>
        public void StopStreamingAudioNextFrameOnMainThread()
        {
            // GameObject must be active in order to run Coroutines and to have Awake called
            if (!gameObject.activeInHierarchy)
            {
                Debug.LogError("GameObject must be active!");
                return;
            }
            
            // Make sure this is only called on the main thread as it can be called from any thread
            StartCoroutine(StopStreamingAudioNextFrameOnMainThreadCoroutine());
        }

        private IEnumerator StopStreamingAudioNextFrameOnMainThreadCoroutine()
        {
            // To the main thread!
            yield return null;
            
            StopStreamingAudioImmediate();
        }
        
        //================================================================================
        // Private Fields
        //================================================================================

        private const long SizeOfS16LE = 2;
        
        private readonly MemoryStream memoryStream = new MemoryStream();
        private readonly object memoryStreamLockObject = new object();
        
        private AudioSource audioSource;

        private GCHandle thisGCHandle;
        private IntPtr pulseAudioDeviceNamePtr;
        private IntPtr mainLoopPtr;
        private IntPtr contextPtr;
        private IntPtr streamPtr;

        //================================================================================
        // Private Unity Event Methods
        //================================================================================

        // OnAudioFilterRead is called every time a chunk of audio is sent to the filter (this happens frequently, every ~20ms depending on the sample rate and platform).
        // The audio data is an array of floats ranging from [-1.0f;1.0f] and contains audio from the previous filter in the chain or the AudioClip on the AudioSource.
        // If this is the first filter in the chain and a clip isn't attached to the audio source, this filter will be played as the audio source.
        // In this way you can use the filter as the audio clip, procedurally generating audio.

        private void OnAudioFilterRead(float[] data, int channels)
        {
            lock (memoryStreamLockObject)
            {
                // Get the bytes we have
                long memoryStreamLengthInBytes = memoryStream.Length;

                // Convert 16-Bit PCM in the memory stream to 32-Bit PCM in data
                int dataIndex = 0;
                long byteIndex = 0;
                long numS16LEToConvert = Math.Min(memoryStreamLengthInBytes / SizeOfS16LE, data.Length);
                byte[] memoryStreamByteBuffer = memoryStream.GetBuffer();
                for (; dataIndex < numS16LEToConvert; dataIndex++, byteIndex += SizeOfS16LE)
                {
                    data[dataIndex] = (short) (memoryStreamByteBuffer[byteIndex] | memoryStreamByteBuffer[byteIndex + 1] << 8) / 32768f;    // 16-Bit PCM normalized to [-1, 1] 32-Bit PCM   
                }
                
                // Go to the beginning of the stream
                memoryStream.Seek(0, SeekOrigin.Begin);

                // Remove transferred bytes
                long newMemoryStreamByteSize = memoryStreamLengthInBytes - byteIndex;
                Buffer.BlockCopy(memoryStreamByteBuffer, (int)byteIndex, memoryStreamByteBuffer, 0, (int)newMemoryStreamByteSize);
                memoryStream.SetLength(newMemoryStreamByteSize);

                // Move memory stream pointer back to the end for writing
                memoryStream.Seek(0, SeekOrigin.End);
            }
        }

        private void Awake()
        {
            // Make sure this survives scene loading
            DontDestroyOnLoad(this);
            
            // Create our AudioSource
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            // Not 3D audio
            audioSource.spatialBlend = 0;

            // Streaming audio clip only works if the audio source loops
            audioSource.loop = true;
            
            // Save this
            thisGCHandle = GCHandle.Alloc(this);
        }
        
        private void OnDestroy()
        {
            // Stop and cleanup almost everything
            StopStreamingAudioImmediate();
            
            // Free this gc handle
            if ((IntPtr) thisGCHandle != IntPtr.Zero)
            {
                thisGCHandle.Free();
            }
        }
        
#if UNITY_EDITOR
        // Reset is called when the user hits the Reset button in the Inspector's context menu or when adding the component the first time.
        // This function is only called in editor mode. Reset is most commonly used to give good default values in the inspector.
        // Therefore we use it to disallow adding this in the Editor.
        private void Reset()
        {
            // Tell the user why
            Debug.LogError($"{GetType().Name} can only be added via script!");
            
            // Kill it with fire
            DestroyImmediate(this);
        }
#endif

        //================================================================================
        // Private PulseAudio Static P/Invoke Callback Methods (Mostly :P)
        //================================================================================

        [MonoPInvokeCallback(typeof(pa_context_sink_source_info_callback))]
        private static void pa_context_source_info_callback(IntPtr contextPtr, IntPtr sinkSourceInfoPtr, int eol, IntPtr userDataPtr)
        {
            // Ignore empty and eol over zero means done
            if (sinkSourceInfoPtr == IntPtr.Zero || eol > 0)
            {
                return;
            }

            // Get this
            var thisPtr = GCHandleToClass<StreamSubscriptionAudioSource>(userDataPtr);

            // Validate
            if (thisPtr == null)
            {
                Debug.LogError("pa_context_source_info_callback thisPtr was null!");
                return;
            }
      
            // Marshall the source info struct
            var sinkSourceInfo = Marshal.PtrToStructure<SinkSourceInfo>(sinkSourceInfoPtr);

            // This is currently the only format that has been tested to work and it's the only format that seems to be used
            Assert.IsTrue(sinkSourceInfo.SampleSpec.SampleFormat != SampleFormat.PA_SAMPLE_S16LE, $"Audio format is unsupported... {sinkSourceInfo.SampleSpec.SampleFormat}");

            // Try/finally to deal with freeing memory
            var sampleSpecPtr = IntPtr.Zero;
            var channelMapPtr = IntPtr.Zero;
            try
            {
                // Convert the sample spec to a ptr
                sampleSpecPtr = Marshal.AllocHGlobal(Marshal.SizeOf(sinkSourceInfo.SampleSpec));
                Marshal.StructureToPtr(sinkSourceInfo.SampleSpec, sampleSpecPtr, false);

                // Convert channel map to a ptr
                channelMapPtr = Marshal.AllocHGlobal(Marshal.SizeOf(sinkSourceInfo.ChannelMap));
                Marshal.StructureToPtr(sinkSourceInfo.ChannelMap, channelMapPtr, false);

                // Create a stream
                thisPtr.streamPtr = pa_stream_new(contextPtr, string.Empty, sampleSpecPtr, channelMapPtr);
                if (thisPtr.streamPtr == IntPtr.Zero)
                {
                    Debug.LogError("pa_stream_new failed...");
                    thisPtr.StopStreamingAudioNextFrameOnMainThread();
                    return;
                }
                
                // Set a callback for the stream's state (to check for failures)
                pa_stream_set_state_callback(thisPtr.streamPtr, pa_stream_state_callback, userDataPtr);

                // Set a callback for reading audio data
                pa_stream_set_read_callback(thisPtr.streamPtr, pa_stream_read_callback, userDataPtr);

                // Attempt to start recording
                if (pa_stream_connect_record(thisPtr.streamPtr, thisPtr.pulseAudioDeviceNamePtr, IntPtr.Zero, StreamFlags.PA_STREAM_NOFLAGS) != 0)
                {
                    Debug.LogError("pa_stream_connect_record failed...");
                    thisPtr.StopStreamingAudioNextFrameOnMainThread();
                    return;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("pa_context_source_info_callback threw an exception...");
                Debug.LogError(ex);
            }
            finally
            {
                // Free the sample spec
                if (sampleSpecPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(sampleSpecPtr);
                }

                // Free the channel map
                if (channelMapPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(channelMapPtr);
                }
            }

            // Deal with AudioSource on the main thread
            thisPtr.StartCoroutine(thisPtr.CreateAndPlayStreamingAudioSourceNextFrameOnMainThread(sinkSourceInfo));
        }
        
        private IEnumerator CreateAndPlayStreamingAudioSourceNextFrameOnMainThread(SinkSourceInfo sinkSourceInfo)
        {
            // To the main thread!
            yield return null;
            
            // Create an endless AudioClip to write to
            audioSource.clip = AudioClip.Create(
                "StreamSubscriptionAudioClip", 
                sinkSourceInfo.SampleSpec.Rate * sinkSourceInfo.SampleSpec.NumChannels, sinkSourceInfo.SampleSpec.NumChannels, sinkSourceInfo.SampleSpec.Rate, 
                true);
            
            // Clear out anything we've read to decrease audio latency
            lock (memoryStreamLockObject)
            {
                memoryStream.SetLength(0);
            }
            
            // Play stream/loop
            audioSource.Play();
        }
        
        [MonoPInvokeCallback(typeof(pa_stream_notify_callback))]
        private static void pa_stream_state_callback(IntPtr streamPtr, IntPtr userDataPtr)
        {
            switch (pa_stream_get_state(streamPtr))
            {
                // Log any failures and bail
                case StreamState.PA_STREAM_FAILED:
                    Debug.LogError("pa_stream_state_callback PA_STREAM_FAILED");
                    GCHandleToClass<StreamSubscriptionAudioSource>(userDataPtr)?.StopStreamingAudioNextFrameOnMainThread();
                    return;

                // Ignore all other states
                default:
                    return;
            }
        }

        [MonoPInvokeCallback(typeof(pa_stream_request_callback))]
        private static unsafe void pa_stream_read_callback(IntPtr streamPtr, int length, IntPtr userDataPtr)
        {
            void* streamUnsafePtr = streamPtr.ToPointer();
            void* dataUnsafePtr = null;

            // Ask for a pointer to read data from
            pa_stream_peek(streamUnsafePtr, &dataUnsafePtr, &length);    // The return value doesn't matter

            // Ignore what we can't use and don't need to drop
            if (dataUnsafePtr == null || length <= 0)
            {
                return;
            }

            // From this point on we have to pa_stream_drop the data to not crash
            try
            {
                // Get this
                var thisPtr = GCHandleToClass<StreamSubscriptionAudioSource>(userDataPtr);

                // Validate
                if (thisPtr == null)
                {
                    Debug.LogError("pa_stream_read_callback thisPtr was null!");
                    return;
                }
                
                // Copy all unmanaged bytes to managed memory stream
                using (var unmanagedMemoryStream = new UnmanagedMemoryStream((byte*) dataUnsafePtr, length, length, FileAccess.Read))
                {
                    // Lock and copy
                    lock (thisPtr.memoryStreamLockObject)
                    {
                        unmanagedMemoryStream.CopyTo(thisPtr.memoryStream);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("pa_stream_read_callback threw an exception...");
                Debug.LogError(ex);
            }
            finally
            {
                // Tell pulse we're done with the data
                pa_stream_drop(streamUnsafePtr);    // The return value doesn't matter
            }
        }

        private static T GCHandleToClass<T>(IntPtr thisPtr) where T : class
        {
            return (((GCHandle) thisPtr).Target) as T;
        }
        
        //================================================================================
        // Private Pulse Static Extern P/Invoke Methods
        //================================================================================

        private const string PulseAudioLibraryFilename = "libpulse-simple.so.0"; // this is the same file FMOD is accessing

        // Main Loop

        [DllImport(PulseAudioLibraryFilename, CallingConvention = CallingConvention.Cdecl)]
        private static extern void pa_threaded_mainloop_lock(IntPtr mainloopPtr);

        [DllImport(PulseAudioLibraryFilename, CallingConvention = CallingConvention.Cdecl)]
        private static extern void pa_threaded_mainloop_unlock(IntPtr mainloopPtr);

        // Context

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void pa_context_sink_source_info_callback(IntPtr contextPtr, IntPtr sourceInfoPtr, int eol, IntPtr userdataPtr);

        [DllImport(PulseAudioLibraryFilename, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr pa_context_get_source_info_by_name(IntPtr contextPtr, IntPtr namePtr, pa_context_sink_source_info_callback callback, IntPtr userdataPtr);

        [StructLayout(LayoutKind.Sequential)]
        private struct SinkSourceInfo
        {
            public IntPtr Name;
            public int Index;
            public IntPtr Description;
            public SampleSpec SampleSpec;
            public ChannelMap ChannelMap;
            public int OwnerModule;
            public CVolume Volume;
            public int Mute;
            public uint MonitorSource;
            public IntPtr MonitorSourceName;
            public long Latency;
            public IntPtr Driver;
            public int Flags;
            public IntPtr Proplist;
            public long ConfiguredLatency;
            public uint BaseVolume;
            public int State;
            public uint VolumeStepCount;
            public uint Card;
            public uint PortCount;
            public IntPtr Ports;
            public IntPtr ActivePort;
            public byte FormatCount;
            public IntPtr Formats;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SampleSpec
        {
            public SampleFormat SampleFormat;
            public int Rate;
            public byte NumChannels;
        }

        private enum SampleFormat
        {
            PA_SAMPLE_U8,
            PA_SAMPLE_ALAW,
            PA_SAMPLE_ULAW,
            PA_SAMPLE_S16LE,
            PA_SAMPLE_S16BE,
            PA_SAMPLE_FLOAT32LE,
            PA_SAMPLE_FLOAT32BE,
            PA_SAMPLE_S32LE,
            PA_SAMPLE_S32BE,
            PA_SAMPLE_S24LE,
            PA_SAMPLE_S24BE,
            PA_SAMPLE_S24_32LE,
            PA_SAMPLE_S24_32BE,
            PA_SAMPLE_MAX,
            PA_SAMPLE_INVALID = -1
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ChannelMap
        {
            public byte NumChannels;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public int[] Positions;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CVolume
        {
            public byte NumChannels;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public int[] Values;
        }

        // Stream

        [DllImport(PulseAudioLibraryFilename, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern IntPtr pa_stream_new(IntPtr contextPtr, [MarshalAs(UnmanagedType.LPStr)] string name, IntPtr sampleSpecPtr, IntPtr channelMapPtr);

        [DllImport(PulseAudioLibraryFilename, CallingConvention = CallingConvention.Cdecl)]
        private static extern void pa_stream_unref(IntPtr streamPtr);
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void pa_stream_notify_callback(IntPtr streamPtr, IntPtr userdataPtr);

        [DllImport(PulseAudioLibraryFilename, CallingConvention = CallingConvention.Cdecl)]
        private static extern void pa_stream_set_state_callback(IntPtr streamPtr, pa_stream_notify_callback callback, IntPtr userDataPtr);

        [DllImport(PulseAudioLibraryFilename, CallingConvention = CallingConvention.Cdecl)]
        private static extern StreamState pa_stream_get_state(IntPtr streamPtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void pa_stream_request_callback(IntPtr streamPtr, int length, IntPtr userDataPtr);

        [DllImport(PulseAudioLibraryFilename, CallingConvention = CallingConvention.Cdecl)]
        private static extern void pa_stream_set_read_callback(IntPtr streamPtr, pa_stream_request_callback callback, IntPtr userDataPtr);

        [StructLayout(LayoutKind.Sequential)]
        private struct BufferAttr
        {
            public uint maxLength;
            public uint tlength;
            public uint prebuf;
            public uint minreq;
            public uint fragsize;
        }

        [Flags]
        private enum StreamFlags
        {
            PA_STREAM_NOFLAGS,
            PA_STREAM_START_CORKED,
            PA_STREAM_INTERPOLATE_TIMING,
            PA_STREAM_NOT_MONOTONIC,
            PA_STREAM_AUTO_TIMING_UPDATE,
            PA_STREAM_NO_REMAP_CHANNELS,
            PA_STREAM_NO_REMIX_CHANNELS,
            PA_STREAM_FIX_FORMAT,
            PA_STREAM_FIX_RATE,
            PA_STREAM_FIX_CHANNELS,
            PA_STREAM_DONT_MOVE,
            PA_STREAM_VARIABLE_RATE,
            PA_STREAM_PEAK_DETECT,
            PA_STREAM_START_MUTED,
            PA_STREAM_ADJUST_LATENCY,
            PA_STREAM_EARLY_REQUESTS,
            PA_STREAM_DONT_INHIBIT_AUTO_SUSPEND,
            PA_STREAM_START_UNMUTED,
            PA_STREAM_FAIL_ON_SUSPEND,
            PA_STREAM_RELATIVE_VOLUME,
            PA_STREAM_PASSTHROUGH
        }

        [DllImport(PulseAudioLibraryFilename, CallingConvention = CallingConvention.Cdecl)]
        private static extern int pa_stream_connect_record(IntPtr streamPtr, IntPtr deviceNamePtr, IntPtr bufferAttrPtr, StreamFlags flags);

        [DllImport(PulseAudioLibraryFilename, CallingConvention = CallingConvention.Cdecl)]
        private static extern int pa_stream_disconnect(IntPtr streamPtr);

        private enum StreamState
        {
            PA_STREAM_UNCONNECTED,
            PA_STREAM_CREATING,
            PA_STREAM_READY,
            PA_STREAM_FAILED,
            PA_STREAM_TERMINATED
        }

        [DllImport(PulseAudioLibraryFilename, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int pa_stream_peek(void* streamUnsafePtr, void** dataUnsafePtrPtr, int* numBytesUnsafePtr);


        [DllImport(PulseAudioLibraryFilename, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int pa_stream_drop(void* streamUnsafePtr);

        // Misc

        [DllImport(PulseAudioLibraryFilename, CallingConvention = CallingConvention.Cdecl)]
        private static extern void pa_operation_unref(IntPtr operationPtr);
    }
}
