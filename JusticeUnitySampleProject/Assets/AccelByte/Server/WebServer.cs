using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using AccelByte.Core;
using UnityEngine.Assertions;

namespace AccelByte.Server
{
    public class WebServer
    {
        private string ClaimPrefix;
        private bool IsCheckingResponse;
        private bool IsWebserverStarted;
        private uint PortNumber;

        private readonly string Namespace;
        private readonly string BaseUrl;

        private HttpListener abHttpListener;
        private HttpListenerContext HttpContext;
        private HttpListenerRequest HttpRequest;
        private HttpListenerResponse HttpResponse;

        const string SERVER_RESPONSE = "200";

        internal WebServer()
        {
            Debug.Log("WebServer Start: ");
            //Start();
        }

        public void Start()
        {
            Debug.Log("Webserver Start...");
            StartWebserverConnection(8080);
        }

        private void StartWebserverConnection(uint portNumber)
        {
            Assert.IsFalse(portNumber == 0, "Creating " + GetType().Name + "failed. Parameter portNumber is 0");

            this.PortNumber = portNumber;

            ClaimPrefix = "http://localhost:8080/claim/";
            //ClaimPrefix = string.Format("https://{0}:{1}/dsm/namespaces/{2}/sessions/claim/", this.BaseUrl, this.PortNumber, this.Namespace);

            Debug.Log(string.Format("Webserver StartWebserverConnection with prefix: {0}", ClaimPrefix));

            Debug.Log("Webserver StartWebserverConnection Start...");
            // Set up a listener.
            abHttpListener = new HttpListener();
            abHttpListener.Prefixes.Add(ClaimPrefix);
            abHttpListener.Start();

            ListenToRequestASync();
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

        private void ListenToRequestASync(AsyncCallback requestCallback)
        {
            Debug.Log("Webserver Async Webserver Listening...");
            IAsyncResult result = abHttpListener.BeginGetContext(requestCallback, abHttpListener);
        }

        // Construct a response.
        private void ConstructResponse()
        {
            Debug.Log("Webserver ConstructResponse ...");

            // Construct a response.
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(SERVER_RESPONSE);

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
