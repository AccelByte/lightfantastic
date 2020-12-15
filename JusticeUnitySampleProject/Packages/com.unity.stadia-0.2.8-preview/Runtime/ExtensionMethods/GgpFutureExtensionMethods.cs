using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Unity.StadiaWrapper
{
    //================================================================================
    // GgpFutureResult/GgpFutureMultipleResult
    //================================================================================

    public struct GgpFutureResult<T> where T : struct
    {
        public readonly T data;
        public readonly GgpStatus ggpStatus;

        public GgpFutureResult(T data, GgpStatus ggpStatus)
        {
            this.data = data;
            this.ggpStatus = ggpStatus;
        }
    }
    
    public struct GgpFutureMultipleResult<T>
    {
        public readonly T[] data;
        public readonly GgpStatus ggpStatus;

        public GgpFutureMultipleResult(T[] data, GgpStatus ggpStatus)
        {
            this.data = data;
            this.ggpStatus = ggpStatus;
        }
    }

    //================================================================================
    // GgpFuture Extension Methods
    //================================================================================

    public static class GgpFutureExtensionMethods
    {
        public static async Task<GgpFutureResult<T>> GetResultAsync<T>(this GgpFuture ggpFuture) where T : struct
        {
            GgpStatus ggpStatus = default;
            T result = default;
            
            await Task.Run(() =>
            {
                result = ggpFuture.GetResultBlocking<T>(out ggpStatus);
            });

            return new GgpFutureResult<T>(result, ggpStatus);
        }

        public static T GetResultBlocking<T>(this GgpFuture ggpFuture, out GgpStatus ggpStatus) where T : struct
        {
            long resultSize = ggpFuture.GetResultSize(out ggpStatus);
            
            if (!ggpStatus.IsOk())
            {
                return default;
            }

            IntPtr intPtr = IntPtr.Zero;
            
            try
            {
                intPtr = Marshal.AllocHGlobal((int)resultSize);
                
                bool success = ggpFuture.GetFutureResultBlockingRaw(intPtr, resultSize, out ggpStatus);
                
                // On success, result will contain the result of the operation, status will be "ok" and the function will return kGgpTrue.
                if (!success || !ggpStatus.IsOk())
                {
                    return default;
                }
                
                T result = Marshal.PtrToStructure<T>(intPtr);
                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(intPtr);
            }
        }
        
        public static async Task<GgpFutureMultipleResult<T>> GetMutipleResultAsync<T>(this GgpFuture ggpFuture) where T : struct
        {
            GgpStatus ggpStatus = default;
            T[] results = default;

            await Task.Run(() =>
            {
                results = ggpFuture.GetMultipleResultBlocking<T>(out ggpStatus);
            });

            return new GgpFutureMultipleResult<T>(results, ggpStatus);
        }

        public static T[] GetMultipleResultBlocking<T>(this GgpFuture ggpFuture, out GgpStatus ggpStatus) where T : struct
        {
            long resultsSize = ggpFuture.GetResultSize(out ggpStatus);
            
            if (!ggpStatus.IsOk())
            {
                return default;
            }

            IntPtr intPtr = IntPtr.Zero;
            
            try
            {
                intPtr = Marshal.AllocHGlobal((int)resultsSize);
                
                long resultsCount = ggpFuture.GetResultCount(out ggpStatus);
                bool success = StadiaNativeApis.GgpFutureGetMultipleResult(ggpFuture, intPtr, resultsSize, resultsCount, out resultsCount, out ggpStatus);
                
                // On success, result will contain the result of the operation, status will be "ok" and the function will return kGgpTrue.
                if (!success || !ggpStatus.IsOk())
                {
                    return default;
                }
                
                T[] results = new T[resultsCount];
                
                IntPtr intPtrInc = new IntPtr(intPtr.ToInt64());
                
                long resultSize = Marshal.SizeOf<T>();
                
                for (int i = 0; i < resultsCount; ++i)
                {
                    results[i] = Marshal.PtrToStructure<T>(intPtrInc);
                    intPtrInc = new IntPtr(intPtrInc.ToInt64() + resultSize);
                }
                return results;
            }
            finally
            {
                Marshal.FreeHGlobal(intPtr);
            }
        }
        
        public static async Task<bool> GetFutureSuccessAsync(this GgpFuture ggpFuture)
        {
            return await Task.Run(() =>
            {
                return ggpFuture.GetFutureSuccessBlocking(); 
            });
        }

        public static bool GetFutureSuccessBlocking(this GgpFuture ggpFuture)
        {
            return ggpFuture.GetFutureResultBlockingRaw(IntPtr.Zero, 0, out _);
        }

        public static bool IsReady(this GgpFuture ggpFuture)
        {
            return StadiaNativeApis.GgpFutureIsReady(ggpFuture);
        }

        public static long GetResultSize(this GgpFuture ggpFuture, out GgpStatus ggpStatus)
        {
            // Blocks until GgpFutureReady() returns true, then returns the size in bytes of all results.
            // On success, out_status will be "ok" and the function will return the result size in bytes (it may be 0).
            // NOTE: While a call to GgpFutureGetResultSize is on-going if the future is used to make any other call (from a different thread) the result of those operations will be kGgpStatusCode_InvalidArgument.
            // NOTE: Even if the result size returned is zero, the future will not be destroyed so the applications must still call GgpFutureGetResult, GgpFutureGetMultipleResult or GgpFutureDetach.
            return StadiaNativeApis.GgpFutureGetResultSize(ggpFuture, out ggpStatus);
        }

        public static long GetResultCount(this GgpFuture ggpFuture, out GgpStatus ggpStatus)
        {
            // Blocks until GgpFutureReady() returns true, then returns the result count.
            // On success, status will be "ok" and the function will return the number of results (it may be 0).
            // NOTE: While a call to GgpFutureGetResultCount is on-going if the future is used to make any other call (from a different thread) the result of those operations will be kGgpStatusCode_InvalidArgument.
            // NOTE: Even if the result count returned is zero, the future will not be destroyed so the applications must still call GgpFutureGetResult, GgpFutureGetMultipleResult or GgpFutureDetach.
            return StadiaNativeApis.GgpFutureGetResultCount(ggpFuture, out ggpStatus);
        }
        
        public static bool GetFutureResultBlockingRaw(this GgpFuture ggpFuture, IntPtr intPtr, long resultSize, out GgpStatus ggpStatus)
        {
            return StadiaNativeApis.GgpFutureGetResult(ggpFuture, intPtr, resultSize, out ggpStatus);
        }
        
        public static void Detach(this GgpFuture ggpFuture)
        {
            StadiaNativeApis.GgpFutureDetach(ggpFuture);
        }
    }
}