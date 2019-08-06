// Copyright (c) 2018 - 2019 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections;
using AccelByte.Models;
using AccelByte.Core;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace AccelByte.Api
{
    internal class WalletApi
    {
        private readonly string baseUrl;
        private readonly UnityHttpWorker httpWorker;

        internal WalletApi(string baseUrl, UnityHttpWorker httpWorker)
        {
            Assert.IsNotNull(baseUrl, "Creating "+ GetType().Name + " failed. Parameter baseUrl is null");
            Assert.IsNotNull(httpWorker, "Creating "+ GetType().Name + " failed. Parameter httpWorker is null");

            this.baseUrl = baseUrl;
            this.httpWorker = httpWorker;
        }

        public IEnumerator GetWalletInfoByCurrencyCode(string @namespace, string userId, string userAccessToken,
            string currencyCode, ResultCallback<WalletInfo> callback)
        {
            Assert.IsNotNull(@namespace, "Can't get wallet info by currency code! Namespace parameter is null!");
            Assert.IsNotNull(userId, "Can't get wallet info by currency code! UserId parameter is null!");
            Assert.IsNotNull(
                userAccessToken,
                "Can't get wallet info by currency code! UserAccessToken parameter is null!");

            Assert.IsNotNull(currencyCode, "Can't get wallet info by currency code! CurrencyCode parameter is null!");

            var builder = HttpRequestBuilder
                .CreateGet(this.baseUrl + "/public/namespaces/{namespace}/users/me/wallets/{currencyCode}")
                .WithPathParam("namespace", @namespace)
                .WithPathParam("userId", userId)
                .WithPathParam("currencyCode", currencyCode)
                .WithBearerAuth(userAccessToken)
                .WithContentType(MediaType.ApplicationJson)
                .Accepts(MediaType.ApplicationJson);

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            var result = request.TryParseResponseJson<WalletInfo>();
            callback.Try(result);
        }
    }
}