// Copyright (c) 2018 - 2019 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine.Assertions;

//TODO: Do authentication using server creadentials

namespace AccelByte.Server
{
    public class AccelByteServerSession : ISession
    {
        public string AuthorizationToken { get; set; }
    }

    public class Server
    {
        //Constants
        private const string AuthorizationCodeEnvironmentVariable = "JUSTICE_AUTHORIZATION_CODE";

        //Readonly members
        private readonly CoroutineRunner coroutineRunner;
        private readonly AccelByteServerSession sessionAdapter;
        private readonly string @namespace;
        private readonly ServerApi api;

        private string podName;

        internal Server(ServerApi server, AccelByteServerSession session, CoroutineRunner coroutineRunner)
        {
            Assert.IsNotNull(server, "api parameter can not be null.");
            Assert.IsNotNull(session, "session parameter can not be null");
            Assert.IsNotNull(coroutineRunner, "coroutineRunner parameter can not be null. Construction failed");

            this.coroutineRunner = coroutineRunner;
            this.sessionAdapter = session;
            this.api = server;
            this.podName = Environment.GetEnvironmentVariable("POD_NAME");
        }

        /// <summary>
        /// Register server ready to DSM
        /// </summary>
        /// <param name="podName">Podname from DSM available in environment variable</param>
        /// <param name="port">UDP port that used for multiplayer connection</param>
        /// <param name="callback">Returns a Result via callback when completed</param>
        public void RegisterServer(int portNumber, ResultCallback<Result> callback)
        {
            Assert.IsNotNull(podName, "Can't Register server; podName is null!");

            if (!this.sessionAdapter.IsValid())
            {
                callback.TryError(ErrorCode.IsNotLoggedIn);
                return;
            }

            RegisterServerRequest registerServerRequest = new RegisterServerRequest
            {
                podName = this.podName,
                port = portNumber
            };

            this.coroutineRunner.Run(this.api.Register(
                        registerServerRequest,
                        this.sessionAdapter.AuthorizationToken,
                        callback));
        }

        /// <summary>
        /// Register server ready to DSM
        /// </summary>
        /// <param name="shutdownServer">Signaling DSM that the server will go down</param>
        /// <param name="podname">Podname from DSM available in environment variable</param>
        /// <param name="callback">Returns a Result via callback when completed</param>
        public void ShutdownServer(bool shutdownServer, string podname, string sessionID, ResultCallback<Result> callback)
        {
            Assert.IsFalse(string.IsNullOrEmpty(podname), "Can't ShutdownServer; podname paramater couldn't be empty");
            Assert.IsFalse(string.IsNullOrEmpty(sessionID), "Can't ShutdownServer; sessionID paramater couldn't be empty");

            if (!this.sessionAdapter.IsValid())
            {
                callback.TryError(ErrorCode.IsNotLoggedIn);
                return;
            }

            var shutdownServerNotif = new ShutdownServerNotif
            {
                killServer = shutdownServer,
                podName = podname,
                sessionId = sessionID
            };

            this.coroutineRunner.Run(this.api.Shutdown(
                        shutdownServerNotif,
                        this.sessionAdapter.AuthorizationToken,
                        callback));
        }
    }
}
