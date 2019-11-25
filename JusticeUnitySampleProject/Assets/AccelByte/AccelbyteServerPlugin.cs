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

        private static readonly Config config;
        private static readonly CoroutineRunner coroutineRunner;
        private static readonly UnityHttpWorker httpWorker;
        private static Server server;
        private static TokenData accessToken;

        public static Config Config { get { return AccelbyteServerPlugin.config; } }

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

            var configFile = Resources.Load("AccelByteSDKConfig");

            if (configFile == null)
            {
                throw new Exception("'AccelByteSDKConfig.json' isn't found in the Project/Assets/Resources directory");
            }

            string wholeJsonText = ((TextAsset)configFile).text;

            AccelbyteServerPlugin.config = wholeJsonText.ToObject<Config>();
            AccelbyteServerPlugin.config.Expand();
            AccelbyteServerPlugin.coroutineRunner = new CoroutineRunner();
            AccelbyteServerPlugin.httpWorker = new UnityHttpWorker();
            ILoginSession loginSession;

            loginSession = new ServerOauthLoginSession(
                AccelbyteServerPlugin.config.IamServerUrl,
                AccelbyteServerPlugin.config.Namespace,
                AccelbyteServerPlugin.config.ClientId,
                AccelbyteServerPlugin.config.ClientSecret,
                AccelbyteServerPlugin.config.RedirectUri,
                AccelbyteServerPlugin.httpWorker,
                AccelbyteServerPlugin.coroutineRunner);

            AccelbyteServerPlugin.serverCredentials = new ServerCredentials(loginSession, coroutineRunner);
        }
        public static Server GetServer()
        {
            if (AccelbyteServerPlugin.server == null)
            {
                AccelbyteServerPlugin.server = new Server(
                    new ServerApi(AccelbyteServerPlugin.config.BaseUrl, AccelbyteServerPlugin.config.Namespace, AccelbyteServerPlugin.httpWorker),
                    AccelbyteServerPlugin.serverCredentials.Session,
                    AccelbyteServerPlugin.coroutineRunner);
            }

            return AccelbyteServerPlugin.server;
        }
    }
}
