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
    /// Writes AudioListener data to a StreamSource PulseAudio device sink.
    /// </summary>
    [AddComponentMenu("")]                            // Hide from the Editor Inspector
    [RequireComponent(typeof(AudioListener))]         // AudioListener is the caller of OnAudioFilterRead
    [DisallowMultipleComponent]                       // OnAudioFilterRead is called once per GameObject
    public class StreamSourceAudioListener : MonoBehaviour
    {
        /// <summary>
        /// PulseAudio threaded main loop pointer. NOTE: ONE MAIN LOOP PER APPLICATION OR PULSEAUDIO WILL DEADLOCK!
        /// </summary>
        public IntPtr PulseAudioMainLoopPtr => mainLoopPtr;

        /// <summary>
        /// PulseAudio context pointer. NOTE: ONE CONTEXT PER APPLICATION OR PULSEAUDIO WILL DEADLOCK!
        /// </summary>
        public IntPtr PulseAudioContextPtr => contextPtr;

        /// <summary>
        /// Set the PulseAudio device name to use.
        /// </summary>
        /// <param name="pulseAudioDeviceName">Name of the StreamSource PulseAudio device.</param>
        public void SetDeviceName(string pulseAudioDeviceName)
        {
            // Make sure it's not already running
            StopWritingAudioImmediate();
            
            // Free the previous device name
            if (pulseAudioDeviceNamePtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(pulseAudioDeviceNamePtr);
                pulseAudioDeviceNamePtr = IntPtr.Zero;
            }

            // Save the device name as a IntPtr
            pulseAudioDeviceNamePtr = Marshal.StringToHGlobalAnsi(pulseAudioDeviceName);
        }

        /// <summary>
        /// Write AudioListener data to PulseAudio device sink.
        /// </summary>
        public void WriteAudioStream()
        {
            // Make sure the device name has been set
            if (pulseAudioDeviceNamePtr == IntPtr.Zero)
            {
                Debug.LogError("SetDeviceName must be called before WriteAudioStream!");
                return;
            }

            // GameObject must be active in order to run Coroutines and to have Awake called
            if (!gameObject.activeInHierarchy)
            {
                Debug.LogError("GameObject must be active!");
                return;
            }

            // Create a main loop
            mainLoopPtr = pa_threaded_mainloop_new();
            if (mainLoopPtr == IntPtr.Zero)
            {
                Debug.LogError("pa_mainloop_new failed...");
                StopWritingAudioImmediate();
                return;
            }

            // Create a context
            contextPtr = pa_context_new(pa_threaded_mainloop_get_api(mainLoopPtr), string.Empty);
            if (contextPtr == IntPtr.Zero)
            {
                Debug.LogError("pa_context_new failed...");
                StopWritingAudioImmediate();
                return;
            }

            // Set the context state callback
            pa_context_set_state_callback(contextPtr, pa_context_state_callback, (IntPtr) thisGCHandle);

            // Connect the context
            if (pa_context_connect(contextPtr, null, ContextFlags.NoAutoSpawn, IntPtr.Zero) != 0)
            {
                Debug.LogError("pa_context_connect failed...");
                StopWritingAudioImmediate();
                return;
            }

            // Start the main loop
            if (pa_threaded_mainloop_start(mainLoopPtr) != 0)
            {
                Debug.LogError("pa_mainloop_start failed...");
                StopWritingAudioImmediate();
            }
        }
               
        /// <summary>
        /// Stop writing audio immediately.
        /// </summary>
        public void StopWritingAudioImmediate()
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
                        pa_stream_set_write_callback(streamPtr, null, IntPtr.Zero);

                        // Unref the stream
                        pa_stream_unref(streamPtr);
                        streamPtr = IntPtr.Zero;
                    }

                    // Validate
                    if (contextPtr != IntPtr.Zero)
                    {
                        // Disconnect the context
                        pa_context_disconnect(contextPtr);

                        // Clear the callbacks
                        pa_context_set_state_callback(contextPtr, null, IntPtr.Zero);

                        // Unref the context
                        pa_context_unref(contextPtr);
                        contextPtr = IntPtr.Zero;
                    }
                }
                // Unlock the main loop
                pa_threaded_mainloop_unlock(mainLoopPtr);
                
                // Terminate the event loop thread cleanly.
                // Make sure to unlock the mainloop object before calling this function.
                pa_threaded_mainloop_stop(mainLoopPtr);

                // Free the main loop
                // If the event loop thread is still running, terminate it with pa_threaded_mainloop_stop() first.
                pa_threaded_mainloop_free(mainLoopPtr);
                mainLoopPtr = IntPtr.Zero;
            }

            // Point the memory stream at the start of its buffer but don't release the memory in case of reuse
            lock(memoryStreamLockObject)
            {
                memoryStream.SetLength(0);
            }
        }
        
          
        /// <summary>
        /// Stop writing audio next frame on the main thread.
        /// </summary>
        public void StopWritingAudioNextFrameOnMainThread()
        {
            // GameObject must be active in order to run Coroutines and to have Awake called
            if (!gameObject.activeInHierarchy)
            {
                Debug.LogError("GameObject must be active!");
                return;
            }
            
            // Make sure this is only called on the main thread as it can be called from any thread
            StartCoroutine(StopWritingAudioNextFrameOnMainThreadCoroutine());
        }

        private IEnumerator StopWritingAudioNextFrameOnMainThreadCoroutine()
        {
            // To the main thread!
            yield return null;
            
            StopWritingAudioImmediate();
        }

        //================================================================================
        // Private Fields
        //================================================================================

        private const int SizeOfS16LE = sizeof(short);
        private const int OnAudioFilterReadDataLength = 2048;
        private const int OnAudioFilterReadConvertedBytesLength = OnAudioFilterReadDataLength * SizeOfS16LE;
        private const int MaxAudioChunks = 4;
        
        private readonly MemoryStream memoryStream = new MemoryStream();
        private readonly object memoryStreamLockObject = new object();
        private readonly byte[] onAudioFilterReadConvertedBytes = new byte[OnAudioFilterReadConvertedBytesLength];
        
        private GCHandle thisGCHandle;
        private IntPtr pulseAudioDeviceNamePtr;
        private IntPtr mainLoopPtr;
        private IntPtr contextPtr;
        private IntPtr streamPtr;

        //================================================================================
        // Private Unity Event Methods
        //================================================================================

        // OnAudioFilterRead is called every time a chunk of audio is sent to the filter (this happens frequently, every ~20ms depending on the sample rate and platform).
        // The audio data is an array of floats ranging from [-1.0f;1.0f] and contains audio from the previous filter in the chain or AudioListener.
        // data.Length is always 2048. If Unity's sample rate/platform is changed, OnAudioFilterRead will just be called more or less often.
        
        private void OnAudioFilterRead(float[] data, int channels)
        {
            Assert.IsTrue(data.Length == OnAudioFilterReadDataLength, $"OnAudioFilterRead data length is {data.Length} but was expecting {OnAudioFilterReadDataLength}.");

            int byteIndex = 0;
            for (int i = 0; i < OnAudioFilterReadDataLength; i++, byteIndex += SizeOfS16LE)
            {
                short dataAsShort = (short) (data[i] * 32768f);
                onAudioFilterReadConvertedBytes[byteIndex] = (byte) (dataAsShort & 255);
                onAudioFilterReadConvertedBytes[byteIndex + 1] = (byte) (dataAsShort >> 8);
            }

            lock (memoryStreamLockObject)
            {
                // Kill latency and kill data when it's not writing to the sink
                if (memoryStream.Length > OnAudioFilterReadConvertedBytesLength * MaxAudioChunks)
                {
                    memoryStream.SetLength(0);
                }
                else
                {
                    memoryStream.Write(onAudioFilterReadConvertedBytes, 0, onAudioFilterReadConvertedBytes.Length);
                }
            }
        }

        private void Awake()
        {
            // Make sure this survives scene loading
            DontDestroyOnLoad(this);
          
            // Save this
            thisGCHandle = GCHandle.Alloc(this);
        }
        
        private void OnDestroy()
        {
            // Stop and cleanup almost everything
            StopWritingAudioImmediate();
            
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

        [MonoPInvokeCallback(typeof(pa_context_notify_callback))]
        private static void pa_context_state_callback(IntPtr contextPtr, IntPtr userDataPtr)
        {
            switch (pa_context_get_state(contextPtr))
            {
                // Only deal with ready
                case ContextState.PA_CONTEXT_READY:
                    break;

                // Log any failures and bail
                case ContextState.PA_CONTEXT_FAILED:
                    Debug.LogError("pa_context_state_callback PA_CONTEXT_FAILED");
                    GCHandleToClass<StreamSourceAudioListener>(userDataPtr)?.StopWritingAudioNextFrameOnMainThread();
                    return;

                // Ignore all other states
                default:
                    return;
            }

            // Get this
            var thisPtr = GCHandleToClass<StreamSourceAudioListener>(userDataPtr);
            
            // Validate
            if (thisPtr == null)
            {
                Debug.LogError("pa_context_state_callback thisPtr was null!");
                return;
            }
            
            // Get the sink info by way of device name
            pa_operation_unref(pa_context_get_sink_info_by_name(contextPtr, thisPtr.pulseAudioDeviceNamePtr, pa_context_sink_info_callback, userDataPtr));
        }
        
        [MonoPInvokeCallback(typeof(pa_context_sink_source_info_callback))]
        private static void pa_context_sink_info_callback(IntPtr contextPtr, IntPtr sinkSourceInfoPtr, int eol, IntPtr userDataPtr)
        {
            // Ignore empty and eol over zero means done
            if (sinkSourceInfoPtr == IntPtr.Zero || eol > 0)
            {
                return;
            }

            // Get this
            var thisPtr = GCHandleToClass<StreamSourceAudioListener>(userDataPtr);

            // Validate
            if (thisPtr == null)
            {
                Debug.LogError("pa_context_source_info_callback thisPtr was null!");
                return;
            }
      
            // Marshall the source info struct
            var sinkSourceInfo = Marshal.PtrToStructure<SinkSourceInfo>(sinkSourceInfoPtr);

            // This is currently the only format that has been tested to work and it's the only format that seems to be used
            Assert.IsTrue(sinkSourceInfo.SampleSpec.NumChannels != 2 || sinkSourceInfo.SampleSpec.SampleFormat != SampleFormat.PA_SAMPLE_S16LE, $"Audio format is unsupported... {sinkSourceInfo.SampleSpec.SampleFormat}");

            // Try/finally to deal with freeing memory
            var sampleSpecPtr = IntPtr.Zero;
            var channelMapPtr = IntPtr.Zero;
            var bufferAttrPtr = IntPtr.Zero;
            try
            {
                // Convert the sample spec to a ptr
                sampleSpecPtr = Marshal.AllocHGlobal(Marshal.SizeOf(sinkSourceInfo.SampleSpec));
                Marshal.StructureToPtr(sinkSourceInfo.SampleSpec, sampleSpecPtr, false);

                // Convert channel map to a ptr
                channelMapPtr = Marshal.AllocHGlobal(Marshal.SizeOf(sinkSourceInfo.ChannelMap));
                Marshal.StructureToPtr(sinkSourceInfo.ChannelMap, channelMapPtr, false);

                // Create a buffer attr
                var bufferAttr = new BufferAttr
                {
                    maxLength = OnAudioFilterReadDataLength * MaxAudioChunks,        // PulseAudio source uses this multiplier as default.
                    tlength = OnAudioFilterReadDataLength,
                    prebuf = OnAudioFilterReadDataLength,
                    minreq = OnAudioFilterReadDataLength / MaxAudioChunks,            // PulseAudio source uses this multiplier as default.
                    // The server sends data in blocks of fragsize bytes size.
                    // Large values diminish interactivity with other operations on the connection context but decrease control overhead.
                    // It is recommended to set this to (uint32_t) -1, which will initialize this to a value that is deemed sensible by the server.
                    // However, this value will default to something like 2s; For applications that have specific latency requirements this value
                    // should be set to the maximum latency that the application can deal with.
                    fragsize = OnAudioFilterReadDataLength 
                };
                bufferAttrPtr = Marshal.AllocHGlobal(Marshal.SizeOf(bufferAttr));
                Marshal.StructureToPtr(bufferAttr, bufferAttrPtr, false);

                // Create a stream
                thisPtr.streamPtr = pa_stream_new(contextPtr, string.Empty, sampleSpecPtr, channelMapPtr);
                if (thisPtr.streamPtr == IntPtr.Zero)
                {
                    Debug.LogError("pa_stream_new failed...");
                    thisPtr.StopWritingAudioNextFrameOnMainThread();
                    return;
                }
                
                // Set a callback for the stream's state (to check for failures)
                pa_stream_set_state_callback(thisPtr.streamPtr, pa_stream_state_callback, userDataPtr);

                // Set a callback for writing audio data
                pa_stream_set_write_callback(thisPtr.streamPtr, pa_stream_write_callback, userDataPtr);

                // Attempt to start playback
                if (pa_stream_connect_playback(thisPtr.streamPtr, thisPtr.pulseAudioDeviceNamePtr, bufferAttrPtr, StreamFlags.PA_STREAM_NOFLAGS, IntPtr.Zero, IntPtr.Zero) != 0)
                {
                    Debug.LogError("pa_stream_connect_playback failed...");
                    thisPtr.StopWritingAudioNextFrameOnMainThread();
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

                // Free the buffer attr
                if (bufferAttrPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(bufferAttrPtr);
                }
            }
        }
        
        [MonoPInvokeCallback(typeof(pa_stream_notify_callback))]
        private static void pa_stream_state_callback(IntPtr streamPtr, IntPtr userDataPtr)
        {
            switch (pa_stream_get_state(streamPtr))
            {
                // Log any failures and bail
                case StreamState.PA_STREAM_FAILED:
                    Debug.LogError("pa_stream_state_callback PA_STREAM_FAILED");
                    GCHandleToClass<StreamSourceAudioListener>(userDataPtr)?.StopWritingAudioNextFrameOnMainThread();
                    return;

                // Ignore all other states
                default:
                    return;
            }
        }
        
        [MonoPInvokeCallback(typeof(pa_stream_request_callback))]
        private static unsafe void pa_stream_write_callback(IntPtr streamPtr, UIntPtr length, IntPtr userDataPtr)    // size_t recommended to be marshalled as UIntPtr
        {
            void* streamUnsafePtr = streamPtr.ToPointer();
            int requestedLength = (int) length.ToUInt32();

            // Ask for a buffer to write to
            void* dataUnsafePtr = null;
            var nbytes = length;
            if (pa_stream_begin_write(streamUnsafePtr, &dataUnsafePtr, ref nbytes) != 0 || dataUnsafePtr == null)
            {
                Debug.LogError("pa_stream_begin_write failed...");
                return;
            }

            // Get the length of the buffer
            int dataLength = (int) nbytes.ToUInt32();

            // Get the length to write to
            int writeLength = dataLength < requestedLength ? dataLength : requestedLength;

            // Need pa_stream_write to be called for this callback to be called again (aka prevent underruns)
            try
            {
                // Get this
                var thisPtr = GCHandleToClass<StreamSourceAudioListener>(userDataPtr);

                // Validate
                if (thisPtr == null)
                {
                    Debug.LogError("pa_stream_write_callback thisPtr was null!");
                    return;
                }

                // Get what we need from this
                var memoryStreamLockObject = thisPtr.memoryStreamLockObject;
                var memoryStream = thisPtr.memoryStream;

                using (var unmanagedMemoryStream = new UnmanagedMemoryStream((byte*) dataUnsafePtr, dataLength, dataLength, FileAccess.Write))
                {
                    lock (memoryStreamLockObject)
                    {
                        // Get the number of bytes we have
                        int memoryStreamLengthInBytes = (int) memoryStream.Length;

                        // Calc the bytes to be copied
                        int numBytesToCopy = writeLength < memoryStreamLengthInBytes ? writeLength : memoryStreamLengthInBytes;

                        // Get the byte buffer
                        byte[] memoryStreamByteBuffer = memoryStream.GetBuffer();

                        // Write managed bytes to unmanaged buffer
                        unmanagedMemoryStream.Write(memoryStreamByteBuffer, 0, numBytesToCopy);

                        // Go to the beginning of the stream
                        memoryStream.Seek(0, SeekOrigin.Begin);

                        // Remove transferred bytes
                        int newMemoryStreamByteSize = memoryStreamLengthInBytes - numBytesToCopy;
                        Buffer.BlockCopy(memoryStreamByteBuffer, numBytesToCopy, memoryStreamByteBuffer, 0, newMemoryStreamByteSize);
                        memoryStream.SetLength(newMemoryStreamByteSize);

                        // Move memory stream pointer back to the end for writing
                        memoryStream.Seek(0, SeekOrigin.End);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("pa_stream_write_callback threw an exception...");
                Debug.LogError(ex);
            }
            finally
            {
                // Write the buffer
                if (pa_stream_write(streamUnsafePtr, dataUnsafePtr, dataLength < requestedLength ? nbytes : length, IntPtr.Zero, 0, SeekMode.PA_SEEK_RELATIVE) != 0)
                {
                    Debug.LogError($"pa_stream_write failed...");
                }
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
        private static extern IntPtr pa_threaded_mainloop_new();

        [DllImport(PulseAudioLibraryFilename, CallingConvention = CallingConvention.Cdecl)]
        private static extern void pa_threaded_mainloop_lock(IntPtr mainloopPtr);

        [DllImport(PulseAudioLibraryFilename, CallingConvention = CallingConvention.Cdecl)]
        private static extern void pa_threaded_mainloop_unlock(IntPtr mainloopPtr);

        [DllImport(PulseAudioLibraryFilename, CallingConvention = CallingConvention.Cdecl)]
        private static extern int pa_threaded_mainloop_start(IntPtr mainloopPtr);

        [DllImport(PulseAudioLibraryFilename, CallingConvention = CallingConvention.Cdecl)]
        private static extern void pa_threaded_mainloop_stop(IntPtr mainloopPtr);

        [DllImport(PulseAudioLibraryFilename, CallingConvention = CallingConvention.Cdecl)]
        private static extern void pa_threaded_mainloop_free(IntPtr mainloopPtr);

        [DllImport(PulseAudioLibraryFilename, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr pa_threaded_mainloop_get_api(IntPtr mainloopPtr);

        // Context

        [DllImport(PulseAudioLibraryFilename, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern IntPtr pa_context_new(IntPtr mainloopApiPtr, [MarshalAs(UnmanagedType.LPStr)] string applicationName);

        [DllImport(PulseAudioLibraryFilename, CallingConvention = CallingConvention.Cdecl)]
        private static extern void pa_context_unref(IntPtr contextPtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void pa_context_notify_callback(IntPtr contextPtr, IntPtr userdataPtr);

        [DllImport(PulseAudioLibraryFilename, CallingConvention = CallingConvention.Cdecl)]
        private static extern void pa_context_set_state_callback(IntPtr contextPtr, pa_context_notify_callback callback, IntPtr userdataPtr);

        private enum ContextFlags
        {
            None = 0,
            NoAutoSpawn = 1,
            NoFail = 2,
        }

        [DllImport(PulseAudioLibraryFilename, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int pa_context_connect(IntPtr contextPtr, [MarshalAs(UnmanagedType.LPStr)] string serverName, ContextFlags contextFlags, IntPtr spawnApiPtr);

        [DllImport(PulseAudioLibraryFilename, CallingConvention = CallingConvention.Cdecl)]
        private static extern int pa_context_disconnect(IntPtr contextPtr);

        private enum ContextState
        {
            PA_CONTEXT_UNCONNECTED,
            PA_CONTEXT_CONNECTING,
            PA_CONTEXT_AUTHORIZING,
            PA_CONTEXT_SETTING_NAME,
            PA_CONTEXT_READY,
            PA_CONTEXT_FAILED,
            PA_CONTEXT_TERMINATED
        }

        [DllImport(PulseAudioLibraryFilename, CallingConvention = CallingConvention.Cdecl)]
        private static extern ContextState pa_context_get_state(IntPtr contextPtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void pa_context_sink_source_info_callback(IntPtr contextPtr, IntPtr sourceInfoPtr, int eol, IntPtr userdataPtr);

        [DllImport(PulseAudioLibraryFilename, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr pa_context_get_sink_info_by_name(IntPtr contextPtr, IntPtr namePtr, pa_context_sink_source_info_callback callback, IntPtr userdataPtr);

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
        private delegate void pa_stream_request_callback(IntPtr streamPtr, UIntPtr length, IntPtr userDataPtr);

        [DllImport(PulseAudioLibraryFilename, CallingConvention = CallingConvention.Cdecl)]
        private static extern void pa_stream_set_write_callback(IntPtr streamPtr, pa_stream_request_callback callback, IntPtr userDataPtr);

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
        private static extern int pa_stream_connect_playback(IntPtr streamPtr, IntPtr deviceNamePtr, IntPtr bufferAttrPtr, StreamFlags flags, IntPtr volumePtr, IntPtr syncStreamPtr);

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
        private static extern unsafe int pa_stream_begin_write(void* streamUnsafePtr, void** dataUnsafePtrPtr, ref UIntPtr nbytes);

        [Flags]
        private enum SeekMode
        {
            PA_SEEK_RELATIVE,
            PA_SEEK_ABSOLUTE,
            PA_SEEK_RELATIVE_ON_READ,
            PA_SEEK_RELATIVE_END
        }

        [DllImport(PulseAudioLibraryFilename, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int pa_stream_write(void* streamUnsafePtr, void* dataUnsafePtr, UIntPtr nbytes, IntPtr freeCallback, long offset, SeekMode seekMode);
        
        // Misc

        [DllImport(PulseAudioLibraryFilename, CallingConvention = CallingConvention.Cdecl)]
        private static extern void pa_operation_unref(IntPtr operationPtr);
    }
}
