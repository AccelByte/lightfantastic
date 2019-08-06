// Copyright (c) 2019 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using Utf8Json;

namespace AccelByte.Api
{
    public class StatisticApi
    {
        private readonly string baseUrl;
        private readonly UnityHttpWorker httpWorker;
        internal StatisticApi(string baseUrl, UnityHttpWorker httpWorker)
        {
            Assert.IsNotNull(baseUrl, "Creating " + GetType().Name + " failed. Parameter baseUrl is null");
            Assert.IsNotNull(httpWorker, "Creating " + GetType().Name + " failed. Parameter httpWorker is null");
            this.baseUrl = baseUrl;
            this.httpWorker = httpWorker;
        }

        public IEnumerator GetAllStatItems(string @namespace, string userId, string profileId, string accessToken,
            ResultCallback<StatItemPagingSlicedResult> callback)
        {
            Assert.IsNotNull(@namespace, "Can't get all stat items! namespace parameter is null!");
            Assert.IsNotNull(userId, "Can't get all stat items! userIds parameter is null!");
            Assert.IsNotNull(accessToken, "Can't get all stat items! accessToken parameter is null!");
            Assert.IsNotNull(profileId, "Can't get all stat items! profileId parameter is null!");

            var builder = HttpRequestBuilder
                .CreateGet(this.baseUrl + "/public/namespaces/{namespace}/users/{userId}/profiles/{profileId}/statitems")
                .WithPathParam("namespace", @namespace)
                .WithPathParam("userId", userId)
                .WithPathParam("profileId", profileId)
                .WithBearerAuth(accessToken)
                .WithContentType(MediaType.ApplicationJson)
                .Accepts(MediaType.ApplicationJson);

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            Result<StatItemPagingSlicedResult> result = request.TryParseResponseJson<StatItemPagingSlicedResult>();

            callback.Try(result);
        }

        public IEnumerator GetStatItemsByStatCodes(string @namespace, string userId, string profileId, string accessToken, ICollection<string> statCodes,
            ResultCallback<StatItemInfo[]> callback)
        {
            Assert.IsNotNull(@namespace, "Can't get stat items! namespace parameter is null!");
            Assert.IsNotNull(userId, "Can't get stat items! userIds parameter is null!");
            Assert.IsNotNull(accessToken, "Can't get stat items! accessToken parameter is null!");
            Assert.IsNotNull(profileId, "Can't get stat items! profileId parameter is null!");

            var builder = HttpRequestBuilder
                .CreateGet(this.baseUrl + "/public/namespaces/{namespace}/users/{userId}/profiles/{profileId}/statitems/byStatCodes")
                .WithPathParam("namespace", @namespace)
                .WithPathParam("userId", userId)
                .WithPathParam("profileId", profileId)
                .WithQueryParam("statCodes", statCodes)
                .WithBearerAuth(accessToken)
                .WithContentType(MediaType.ApplicationJson)
                .Accepts(MediaType.ApplicationJson);

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            Result<StatItemInfo[]> result = request.TryParseResponseJson<StatItemInfo[]>();

            callback.Try(result);
        }
    }
}