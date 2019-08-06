// Copyright (c) 2019 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using AccelByte.Models;
using AccelByte.Core;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace AccelByte.Api
{
    internal class EntitlementApi
    {
        private readonly string baseUrl;
        private readonly UnityHttpWorker httpWorker;

        internal EntitlementApi(string baseUrl, UnityHttpWorker httpWorker)
        {
            Assert.IsNotNull(baseUrl, "Creating "+ GetType().Name + " failed. Parameter baseUrl is null");
            Assert.IsNotNull(httpWorker, "Creating "+ GetType().Name + " failed. Parameter httpWorker is null");

            this.baseUrl = baseUrl;
            this.httpWorker = httpWorker;
        }

        public IEnumerator GetUserEntitlements(string @namespace, string userId, string userAccessToken, int offset,
            int limit, ResultCallback<PagedEntitlements> callback)
        {
            Assert.IsNotNull(@namespace, "Can't get user entitlements! Namespace parameter is null!");
            Assert.IsNotNull(userId, "Can't get user entitlements! UserId parameter is null!");
            Assert.IsNotNull(userAccessToken, "Can't get user entitlements! UserAccessToken parameter is null!");

            var builder = HttpRequestBuilder
                .CreateGet(this.baseUrl + "/public/namespaces/{namespace}/users/{userId}/entitlements")
                .WithPathParam("namespace", @namespace)
                .WithPathParam("userId", userId)
                .WithQueryParam("offset", offset.ToString())
                .WithQueryParam("limit", limit.ToString())
                .WithBearerAuth(userAccessToken)
                .WithContentType(MediaType.ApplicationJson)
                .Accepts(MediaType.ApplicationJson);

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            var result = request.TryParseResponseJson<PagedEntitlements>();
            callback.Try(result);
        }
    }
}