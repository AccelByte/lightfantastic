// Copyright (c) 2019 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
#if !UNITY_WEBGL || UNITY_EDITOR
using WebSocketSharp;

#endif

namespace HybridWebSocket
{
    public delegate void OnOpenHandler();

    public delegate void OnMessageHandler(string data);

    public delegate void OnErrorHandler(string errorMsg);

    public delegate void OnCloseHandler(ushort closeCode);

    public enum WsState { Connecting, Open, Closing, Closed }

    public enum WsCloseCode
    {
        /* Do NOT use NotSet - it's only purpose is to indicate that the close code cannot be parsed. */
        NotSet = 0,
        Normal = 1000,
        Away = 1001,
        ProtocolError = 1002,
        UnsupportedData = 1003,
        Undefined = 1004,
        NoStatus = 1005,
        Abnormal = 1006,
        InvalidData = 1007,
        PolicyViolation = 1008,
        TooBig = 1009,
        MandatoryExtension = 1010,
        ServerError = 1011,
        TlsHandshakeFailure = 1015
    }

    public interface IWebSocket
    {
        void Connect();
        void Close(WsCloseCode code = WsCloseCode.Normal, string reason = null);
        void Send(string message);

        void Ping();
        WsState ReadyState { get; }
        event OnOpenHandler OnOpen;
        event OnMessageHandler OnMessage;
        event OnErrorHandler OnError;
        event OnCloseHandler OnClose;
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    public static class JslibInterop
    {
        private delegate void InteropOnOpenHandler(uint instanceId);

        private delegate void InteropOnMessageHandler(uint instanceId, IntPtr messagePtr);

        private delegate void InteropOnErrorHandler(uint instanceId, IntPtr errorMsgPtr);

        private delegate void InteropOnCloseHandler(uint instanceId, ushort closeCode);

        private static Dictionary<uint, OnOpenHandler> onOpenHandlers =  new Dictionary<uint, OnOpenHandler>();
        private static Dictionary<uint, OnMessageHandler> onMessageHandlers = new Dictionary<uint, OnMessageHandler>();
        private static Dictionary<uint, OnErrorHandler> onErrorHandlers = new Dictionary<uint, OnErrorHandler>();
        private static Dictionary<uint, OnCloseHandler> onCloseHandlers = new Dictionary<uint, OnCloseHandler>();

        static JslibInterop()
        {
            JslibInterop.WsSetEventHandlers(
                JslibInterop.WsHandleOnOpen,
                JslibInterop.WsHandleOnMessage,
                JslibInterop.WsHandleOnError,
                JslibInterop.WsHandleOnClose);
        }

        [DllImport("__Internal")]
        private static extern uint WsSetEventHandlers(InteropOnOpenHandler onOpen, InteropOnMessageHandler onMessage,
            InteropOnErrorHandler onError, InteropOnCloseHandler onClose);

        [DllImport("__Internal")] public static extern uint WsCreate(string url, string protocols);

        [DllImport("__Internal")] public static extern uint WsOpen(uint objectId);

        [DllImport("__Internal")] public static extern uint WsClose(uint objectId, int code, string reason);

        [DllImport("__Internal")] public static extern uint WsSend(uint objectId, string message);

        [DllImport("__Internal")] public static extern uint WsDestroy(uint objectId);

        [DllImport("__Internal")] public static extern uint WsGetReadyState(uint objectId);
        
        public static void WsResetEvents(uint objectId)
        {
            JslibInterop.onOpenHandlers.Remove(objectId);
            JslibInterop.onMessageHandlers.Remove(objectId);
            JslibInterop.onErrorHandlers.Remove(objectId);
            JslibInterop.onCloseHandlers.Remove(objectId);
        }

        public static void WsAddOnOpen(uint objectId, OnOpenHandler callback)
        {
            if (!JslibInterop.onOpenHandlers.ContainsKey(objectId))
            {
                JslibInterop.onOpenHandlers[objectId] = null;
            }
            
            JslibInterop.onOpenHandlers[objectId] += callback;
        }

        public static void WsRemoveOnOpen(uint objectId, OnOpenHandler callback)
        {
            if (!JslibInterop.onOpenHandlers.ContainsKey(objectId)) return;
            
            JslibInterop.onOpenHandlers[objectId] -= callback;
        }

        public static void WsAddOnMessage(uint objectId, OnMessageHandler callback)
        {
            if (!JslibInterop.onMessageHandlers.ContainsKey(objectId))
            {
                JslibInterop.onMessageHandlers[objectId] = null;
            }

            JslibInterop.onMessageHandlers[objectId] += callback;
        }

        public static void WsRemoveOnMessage(uint objectId, OnMessageHandler callback)
        {
            if (!JslibInterop.onMessageHandlers.ContainsKey(objectId)) return;
            
            JslibInterop.onMessageHandlers[objectId] -= callback;
        }

        public static void WsAddOnError(uint objectId, OnErrorHandler callback)
        {
            if (!JslibInterop.onErrorHandlers.ContainsKey(objectId))
            {
                JslibInterop.onErrorHandlers[objectId] = null;
            }

            JslibInterop.onErrorHandlers[objectId] += callback;
        }

        public static void WsRemoveOnError(uint objectId, OnErrorHandler callback)
        {
            if (!JslibInterop.onErrorHandlers.ContainsKey(objectId)) return;

            JslibInterop.onErrorHandlers[objectId] -= callback;
        }

        public static void WsAddOnClose(uint objectId, OnCloseHandler callback)
        {
            if (!JslibInterop.onCloseHandlers.ContainsKey(objectId))
            {
                JslibInterop.onCloseHandlers[objectId] = null;
            }

            JslibInterop.onCloseHandlers[objectId] += callback;
        }

        public static void WsRemoveOnClose(uint objectId, OnCloseHandler callback)
        {
            if (!JslibInterop.onCloseHandlers.ContainsKey(objectId)) return;

            JslibInterop.onCloseHandlers[objectId] -= callback;
        }

        [MonoPInvokeCallback(typeof(InteropOnOpenHandler))]
        private static void WsHandleOnOpen(uint objectId)
        {
            OnOpenHandler handler;

            if (!JslibInterop.onOpenHandlers.TryGetValue(objectId, out handler)) return;

            if (handler == null) return;
            
            handler();                    
        }

        [MonoPInvokeCallback(typeof(InteropOnMessageHandler))]
        private static void WsHandleOnMessage(uint objectId, IntPtr messagePtr)
        {
            OnMessageHandler handler;

            if (!JslibInterop.onMessageHandlers.TryGetValue(objectId, out handler)) return;

            if (handler == null) return;

            handler(Marshal.PtrToStringAuto(messagePtr));                    
        }

        [MonoPInvokeCallback(typeof(InteropOnErrorHandler))]
        private static void WsHandleOnError(uint objectId, IntPtr errorPtr)
        {
            OnErrorHandler handler;

            if (!JslibInterop.onErrorHandlers.TryGetValue(objectId, out handler)) return;
            
            if (handler == null) return;

            handler(Marshal.PtrToStringAuto(errorPtr));                    
        }

        [MonoPInvokeCallback(typeof(InteropOnCloseHandler))]
        private static void WsHandleOnClose(uint objectId, ushort closeCode)
        {
            OnCloseHandler handler;

            if (!JslibInterop.onCloseHandlers.TryGetValue(objectId, out handler)) return;

            if (handler == null) return;
            
            handler(closeCode);                    
        }
    }

    public class WebSocket : IWebSocket
    {
        private readonly uint objectId;

        public WebSocket(string url, string protocols)
        {
            this.objectId = JslibInterop.WsCreate(url, protocols);
        }

        ~WebSocket()
        {
            JslibInterop.WsResetEvents(this.objectId);
            JslibInterop.WsDestroy(this.objectId);
        }

        public WsState ReadyState 
        { 
            get
            {
                uint state = JslibInterop.WsGetReadyState(this.objectId);

                switch (state)
                {
                case 0:
                    return WsState.Connecting;

                case 1:
                    return WsState.Open;

                case 2:
                    return WsState.Closing;

                case 3:
                    return WsState.Closed;

                default:
                    throw new WebSocketInvalidStateException("Unrecognized websocket ready state.");
                }
            }
        }

        public void Connect()
        {
            JslibInterop.WsOpen(this.objectId);
        }

        public void Close(WsCloseCode code = WsCloseCode.Normal, string reason = null)
        {
            JslibInterop.WsClose(this.objectId, (int)code, reason);
        }

        public void Send(string message)
        {
            JslibInterop.WsSend(this.objectId, message);
        }

        public void Ping()
        {
            JslibInterop.WsSend(this.objectId, "");
        }

        public event OnOpenHandler OnOpen
        {
            add { JslibInterop.WsAddOnOpen(this.objectId, value); }
            remove { JslibInterop.WsRemoveOnOpen(this.objectId, value); }
        }

        public event OnMessageHandler OnMessage
        {
            add { JslibInterop.WsAddOnMessage(this.objectId, value); }
            remove { JslibInterop.WsRemoveOnMessage(this.objectId, value); }
        }

        public event OnErrorHandler OnError
        {
            add { JslibInterop.WsAddOnError(this.objectId, value); }
            remove { JslibInterop.WsRemoveOnError(this.objectId, value); }
        }

        public event OnCloseHandler OnClose
        {
            add { JslibInterop.WsAddOnClose(this.objectId, value); }
            remove { JslibInterop.WsRemoveOnClose(this.objectId, value); }
        }
    }
#else

    public class WebSocket : IWebSocket
    {
        private readonly WebSocketSharp.WebSocket webSocket;

        public event OnOpenHandler OnOpen;
        public event OnMessageHandler OnMessage;
        public event OnErrorHandler OnError;
        public event OnCloseHandler OnClose;

        public WebSocket(string url, string protocols = null)
            : this(new WebSocketSharp.WebSocket(url, protocols)) { }

        public WebSocket(WebSocketSharp.WebSocket webSocket)
        {
            try
            {
                this.webSocket = webSocket;

                this.webSocket.OnOpen += (sender, ev) =>
                {
                    OnOpenHandler handler = this.OnOpen;

                    if (handler != null)
                    {
                        handler();
                    }
                };

                this.webSocket.OnMessage += (sender, ev) =>
                {
                    if (ev.RawData == null) return;

                    OnMessageHandler handler = this.OnMessage;

                    if (handler != null)
                    {
                        handler(ev.Data);
                    }
                };

                this.webSocket.OnError += (sender, ev) =>
                {
                    OnErrorHandler handler = this.OnError;

                    if (handler != null)
                    {
                        handler(ev.Message);
                    }
                };

                this.webSocket.OnClose += (sender, ev) =>
                {
                    OnCloseHandler handler = this.OnClose;

                    if (handler != null)
                    {
                        handler(ev.Code);
                    }
                };
            }
            catch (Exception e)
            {
                throw new WebSocketUnexpectedException("Websocket cannot be created.", e);
            }
        }

        ~WebSocket()
        {
            this.OnOpen = null;
            this.OnMessage = null;
            this.OnError = null;
            this.OnClose = null;
        }

        public WsState ReadyState
        {
            get
            {
                switch (this.webSocket.ReadyState)
                {
                    case WebSocketState.Open: return WsState.Open;
                    case WebSocketState.Closed: return WsState.Closed;
                    case WebSocketState.Closing: return WsState.Closing;
                    case WebSocketState.Connecting: return WsState.Connecting;
                    default: throw new WebSocketInvalidStateException("Unrecognized websocket ready state.");
                }
            }
        }

        public void Connect()
        {
            switch (this.webSocket.ReadyState)
            {
            case WebSocketState.Open: return;
            case WebSocketState.Closing: throw new WebSocketInvalidStateException("WebSocket is closing.");
            case WebSocketState.Closed: break;
            default:

                try
                {
                    this.webSocket.Connect();
                }
                catch (Exception e)
                {
                    throw new WebSocketUnexpectedException("Websocket failed to connect.", e);
                }

                if (this.webSocket.ReadyState != WebSocketState.Open)
                {
                    throw new WebSocketUnexpectedException("Websocket failed to connect.");
                }

                break;
            }
        }

        public void Send(string message)
        {
            // Check state
            if (this.webSocket.ReadyState != WebSocketState.Open)
                throw new WebSocketInvalidStateException("Websocket is not open.");

            try
            {
                this.webSocket.SendAsync(message, null);
            }
            catch (Exception e)
            {
                throw new WebSocketUnexpectedException("Unexpected websocket exception.", e);
            }
        }

        public void Ping()
        {
            this.webSocket.SendAsync("", null);
        }

        public void Close(WsCloseCode code = WsCloseCode.Normal, string reason = null)
        {
            if (this.webSocket.ReadyState == WebSocketState.Closing ||
                this.webSocket.ReadyState == WebSocketState.Closed)
            {
                return;
            }

            try
            {
                this.webSocket.Close((ushort) code, reason);
            }
            catch (Exception e)
            {
                throw new WebSocketUnexpectedException("Failed to close the connection.", e);
            }
        }
    }
#endif

    public abstract class WebSocketException : Exception
    {
        public WebSocketException(string message, Exception innerException = null)
            : base(message, innerException) { }
    }

    public class WebSocketUnexpectedException : WebSocketException
    {
        public WebSocketUnexpectedException(string message, Exception innerException = null)
            : base(message, innerException) { }
    }

    public class WebSocketInvalidStateException : WebSocketException
    {
        public WebSocketInvalidStateException(string message, Exception innerException = null)
            : base(message, innerException) { }
    }
}