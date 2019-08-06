using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using Random = System.Random;

namespace AccelByte.Core
{
    public class UnityHttpWorker
    {
        public event Action<UnityWebRequest> ServerErrorOccured;
        
        public event Action<UnityWebRequest> NetworkErrorOccured;

        public IEnumerator SendWithRetry(HttpRequestBuilder requestBuilder, Action<UnityWebRequest> requestDoneCallback, 
            uint totalTimeout = 60000, uint initialDelay = 1000, uint maxDelay = 30000)
        {
            var rand = new Random();
            uint nextDelay = initialDelay;
            var stopwatch = new Stopwatch();
            UnityWebRequest request;
            stopwatch.Start();

            if (requestBuilder == null)
            {
                if (requestDoneCallback != null)
                {
                    requestDoneCallback(null);
                }
                
                yield break;
            }

            do
            {
                request = requestBuilder.ToUnityWebRequest();
                request.timeout = (int)(totalTimeout / 1000);

                yield return request.SendWebRequest();

                if (request.isNetworkError)
                {
                    Action<UnityWebRequest> netErrorHandler = this.NetworkErrorOccured;

                    if (netErrorHandler != null)
                    {
                        netErrorHandler(request);
                    }

                    if (requestDoneCallback != null)
                    {
                        requestDoneCallback(request);
                    }
                    
                    yield break;
                }

                switch ((HttpStatusCode) request.responseCode)
                {
                case HttpStatusCode.InternalServerError:
                case HttpStatusCode.BadGateway:
                case HttpStatusCode.ServiceUnavailable:
                case HttpStatusCode.GatewayTimeout:
                    Action<UnityWebRequest> serverErrorHandler = this.ServerErrorOccured;

                    if (serverErrorHandler != null)
                    {
                        serverErrorHandler(request);
                    }
                    
                    float delaySeconds = (float) (0.75f * nextDelay + 0.5 * rand.NextDouble() * nextDelay) / 1000f;

                    yield return new WaitForSeconds(delaySeconds);

                    nextDelay *= 2;

                    if (nextDelay > maxDelay)
                    {
                        nextDelay = maxDelay;
                    }

                    break;

                default:
                    if (requestDoneCallback != null)
                    {
                        requestDoneCallback(request);
                    }
                    
                    yield break;
                }
            }
            while (stopwatch.Elapsed < TimeSpan.FromMilliseconds(totalTimeout));
            
            if (requestDoneCallback != null)
            {
                requestDoneCallback(request);
            }
        }
    }
}