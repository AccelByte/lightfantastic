using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;

namespace AccelByte.Server
{
    public class WebServer : MonoBehaviour
    {
        private string ClaimPrefix;
        private bool IsCheckingResponse;
        private bool IsWebserverStarted;
        private int PortNumber;

        private HttpListener abHttpListener;
        private HttpListenerContext HttpContext;
        private HttpListenerRequest HttpRequest;
        private HttpListenerResponse HttpResponse;

        void Awake()
        {
            //IsCheckingResponse = true;
            //ClaimPrefix = "https://api.demo.accelbyte.io:9991/dsm/namespaces/unitySampleGame/sessions/claim/";
            //ClaimPrefix = "http://lukka.com:8080/test/";
            ClaimPrefix = "http://localhost:8080/test/";
        }

        void Start()
        {
            Debug.Log("Webserver Start...");

            StartWebserverConnection();
            ListenToRequestASync();
        }

        private void StartWebserverConnection()
        {
            Debug.Log("Webserver StartWebserverConnection Start...");
            Debug.Log(string.Format("Webserver with prefix: {0}", ClaimPrefix));

            // Set up a listener.
            abHttpListener = new HttpListener();
            abHttpListener.Prefixes.Add(ClaimPrefix);
            abHttpListener.Start();

            IsWebserverStarted = true;
        }

        private void ListenToRequestSync()
        {
            Debug.Log("Webserver Sync Webserver Listening...");
            // The GetContext method blocks while waiting for a request. 
            HttpContext = abHttpListener.GetContext();
            HttpRequest = HttpContext.Request;
            HttpResponse = HttpContext.Response;

            ConstructResponse();
            CloseWebServerConnection();
        }

        private void ListenToRequestASync()
        {
            Debug.Log("Webserver Async Webserver Listening...");
            IAsyncResult result = abHttpListener.BeginGetContext(new AsyncCallback(OnRequestCallback), abHttpListener);
        }

        // Construct a response.
        private void ConstructResponse()
        {
            Debug.Log("Webserver ConstructResponse ...");

            // Construct a response.
            string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);

            // Get a response stream and write the response to it.
            HttpResponse.ContentLength64 = buffer.Length;
            System.IO.Stream output = HttpResponse.OutputStream;
            output.Write(buffer, 0, buffer.Length);

            // Close the output stream.
            output.Close();
        }

        private void CloseWebServerConnection()
        {
            if (IsWebserverStarted)
            {
                abHttpListener.Stop();
                IsWebserverStarted = false;
                Debug.Log("Webserver CloseWebServerConnection ...");
            }
        }

        private void OnRequestCallback(IAsyncResult result)
        {
            Debug.Log("Webserver OnRequestCallback Received");
            // Call EndGetContext to complete the Async operation
            HttpContext = abHttpListener.EndGetContext(result);
            HttpRequest = HttpContext.Request;
            HttpResponse = HttpContext.Response;

            ConstructResponse();
            CloseWebServerConnection();
        }
    }
}
