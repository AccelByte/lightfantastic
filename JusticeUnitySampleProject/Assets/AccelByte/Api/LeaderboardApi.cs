﻿// Copyright (c) 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine.Assertions;

namespace AccelByte.Api
{
    internal class LeaderboardApi
    {
        private readonly string baseUrl;
        private readonly IHttpWorker httpWorker;

        internal LeaderboardApi(string baseUrl, IHttpWorker httpWorker)
        {
            Assert.IsNotNull(baseUrl, "Creating " + GetType().Name + " failed. Parameter baseUrl is null");
            Assert.IsNotNull(httpWorker, "Creating " + GetType().Name + " failed. Parameter httpWorker is null");

            this.baseUrl = baseUrl;
            this.httpWorker = httpWorker;
        }

        public IEnumerator QueryAllTimeLeaderboardRankingData(string @namespace, string accessToken, string leaderboardCode, int offset, int limit,
            ResultCallback<LeaderboardRankingResult> callback)
        {
            Report.GetFunctionLog(this.GetType().Name);
            Assert.IsNotNull(@namespace, "Can't get item! Namespace parameter is null!");
            Assert.IsNotNull(accessToken, "Can't get item! AccessToken parameter is null!");
            Assert.IsNotNull(leaderboardCode, "Can't get item! Leaderboard Code parameter is null!");

            var builder = HttpRequestBuilder
                .CreateGet(this.baseUrl + "/v1/public/namespaces/{namespace}/leaderboards/{leaderboardCode}/alltime")
                .WithPathParam("namespace", @namespace)
                .WithPathParam("leaderboardCode", leaderboardCode)
                .WithQueryParam("offset", (offset >= 0) ? offset.ToString() : "")
                .WithQueryParam("limit", (limit >= 0) ? limit.ToString() : "")
                .WithBearerAuth(accessToken)
                .Accepts(MediaType.ApplicationJson);

            var request = builder.GetResult();

            IHttpResponse response = null;

            yield return this.httpWorker.SendRequest(request, rsp => response = rsp);

            var result = response.TryParseJson<LeaderboardRankingResult>();

            callback.Try(result);
        }

        public IEnumerator GetUserRanking(string @namespace, string accessToken, string leaderboardCode, string userId,
            ResultCallback<UserRankingData> callback)
        {
            Report.GetFunctionLog(this.GetType().Name);
            Assert.IsNotNull(@namespace, "Can't get item! Namespace parameter is null!");
            Assert.IsNotNull(accessToken, "Can't get item! AccessToken parameter is null!");
            Assert.IsNotNull(leaderboardCode, "Can't get item! Leaderboard Code parameter is null!");
            Assert.IsNotNull(userId, "Can't get item! UserId parameter is null!");

            var builder = HttpRequestBuilder
                .CreateGet(this.baseUrl + "/v1/public/namespaces/{namespace}/leaderboards/{leaderboardCode}/users/{userId}")
                .WithPathParam("namespace", @namespace)
                .WithPathParam("leaderboardCode", leaderboardCode)
                .WithPathParam("userId", userId)
                .WithBearerAuth(accessToken)
                .Accepts(MediaType.ApplicationJson);

            var request = builder.GetResult();

            IHttpResponse response = null;

            yield return this.httpWorker.SendRequest(request, rsp => response = rsp);

            var result = response.TryParseJson<UserRankingData>();

            callback.Try(result);
        }
    }
}