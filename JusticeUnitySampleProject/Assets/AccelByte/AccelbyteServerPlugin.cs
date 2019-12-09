// Copyright (c) 2018 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using AccelByte.Core;
using AccelByte.Api;
using AccelByte.Models;
using HybridWebSocket;
using UnityEngine;

namespace AccelByte.Server
{
    public static class AccelbyteServerPlugin
    {
        private static readonly ServerCredentials serverCredentials;

        private static readonly ServerConfig serverConfig;
        private static readonly CoroutineRunner coroutineRunner;
        private static readonly UnityHttpWorker httpWorker;
        private static Server server;
        private static TokenData accessToken;

        private static readonly WebServer webServer;

        public static ServerConfig ServerConfig { get { return AccelbyteServerPlugin.serverConfig; } }

        static AccelbyteServerPlugin()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            Utf8Json.Resolvers.CompositeResolver.RegisterAndSetAsDefault(
                new [] {
                    Utf8Json.Formatters.PrimitiveObjectFormatter.Default
                },
                new[] {
                    Utf8Json.Resolvers.GeneratedResolver.Instance,
                    Utf8Json.Resolvers.BuiltinResolver.Instance,
                    Utf8Json.Resolvers.EnumResolver.Default,
                    // for unity
                    Utf8Json.Unity.UnityResolver.Instance
                }
            );
#endif

            var configFile = Resources.Load("AccelByteServerSDKConfig");

            if (configFile == null)
            {
                throw new Exception("'AccelByteServerSDKConfig.json' isn't found in the Project/Assets/Resources directory");
            }

            string wholeJsonText = ((TextAsset)configFile).text;

            Debug.Log("ServerConfig Json: " + wholeJsonText);

            AccelbyteServerPlugin.serverConfig = wholeJsonText.ToObject<ServerConfig>();
            AccelbyteServerPlugin.serverConfig.Expand();
            AccelbyteServerPlugin.coroutineRunner = new CoroutineRunner();
            AccelbyteServerPlugin.httpWorker = new UnityHttpWorker();
            ILoginSession loginSession;

            loginSession = new ServerOauthLoginSession(
                AccelbyteServerPlugin.serverConfig.IamServerUrl,
                AccelbyteServerPlugin.serverConfig.Namespace,
                AccelbyteServerPlugin.serverConfig.ClientId,
                AccelbyteServerPlugin.serverConfig.ClientSecret,
                AccelbyteServerPlugin.serverConfig.RedirectUri,
                AccelbyteServerPlugin.httpWorker,
                AccelbyteServerPlugin.coroutineRunner);

            AccelbyteServerPlugin.serverCredentials = new ServerCredentials(loginSession, AccelbyteServerPlugin.coroutineRunner);

            AccelbyteServerPlugin.webServer = new WebServer();
        }
        public static Server GetServer()
        {
            Debug.Log("AccelByteServerPlugin start get server");
            if (AccelbyteServerPlugin.server == null)
            {
                AccelbyteServerPlugin.server = new Server(
                    new ServerApi(AccelbyteServerPlugin.serverConfig.DSMServerUrl, AccelbyteServerPlugin.serverConfig.Namespace, AccelbyteServerPlugin.httpWorker),
                    AccelbyteServerPlugin.serverCredentials.Session,
                    AccelbyteServerPlugin.coroutineRunner);
                Debug.Log("AccelByteServerPlugin Getserver Create Server");
            }

            return AccelbyteServerPlugin.server;
        }

        public static ServerCredentials GetServerCredentials()
        {
            return AccelbyteServerPlugin.serverCredentials;
        }

        public static WebServer GetWebServer()
        {
            return AccelbyteServerPlugin.webServer;
        }
    }
}
