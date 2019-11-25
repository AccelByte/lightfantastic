// Copyright (c) 2018 - 2019 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections;
using AccelByte.Models;
using AccelByte.Core;
using UnityEngine.Assertions;

namespace AccelByte.Server
{
    internal class ServerApi
    {
        private readonly string baseUrl;
        private readonly IHttpWorker httpWorker;
        private readonly string @namespace;

        internal ServerApi(string baseUrl, string nameSpace, IHttpWorker httpWorker)
        {
            Assert.IsNotNull(baseUrl, "Creating " + GetType().Name + " failed. Parameter baseUrl is null");
            Assert.IsFalse(
                string.IsNullOrEmpty(@namespace),
                "Creating " + GetType().Name + " failed. Parameter ns is null.");
            Assert.IsNotNull(httpWorker, "Creating " + GetType().Name + " failed. Parameter httpWorker is null");

            this.baseUrl = baseUrl;
            this.@namespace = nameSpace;
            this.httpWorker = httpWorker;
        }

        public IEnumerator Register(RegisterServerRequest registerServerRequest, string accessToken,
            ResultCallback<Result> callback)
        {
            Assert.IsNotNull(registerServerRequest, "Register failed. registerserverRequest is null!");
            Assert.IsNotNull(accessToken, "Can't update a slot! accessToken parameter is null!");

            var request = HttpRequestBuilder.CreatePost(this.baseUrl + "/dsm/namespaces/{namespace}/servers/register")
                .WithPathParam("namespace", this.@namespace)
                .WithContentType(MediaType.ApplicationJson)
                .WithBearerAuth(accessToken)
                .WithBody(registerServerRequest.ToUtf8Json())
                .GetResult();

            IHttpResponse response = null;

            yield return this.httpWorker.SendRequest(request, rsp => response = rsp);

            var result = response.TryParseJson<Result>();
            callback.Try(result);
        }

        public IEnumerator Shutdown(ShutdownServerNotif shutdownServerNotif, string accessToken,
            ResultCallback<Result> callback)
        {
            Assert.IsNotNull(shutdownServerNotif, "Register failed. shutdownServerNotif is null!");
            Assert.IsNotNull(accessToken, "Can't update a slot! accessToken parameter is null!");

            var request = HttpRequestBuilder.CreatePost(this.baseUrl + "/dsm/namespaces/{namespace}/servers/shutdown")
                .WithPathParam("namespace", this.@namespace)
                .WithContentType(MediaType.ApplicationJson)
                .WithBearerAuth(accessToken)
                .WithBody(shutdownServerNotif.ToUtf8Json())
                .GetResult();

            IHttpResponse response = null;

            yield return this.httpWorker.SendRequest(request, rsp => response = rsp);

            var result = response.TryParseJson<Result>();
            callback.Try(result);
        }
    }
}
