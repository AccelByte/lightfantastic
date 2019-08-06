// Copyright (c) 2018 - 2019 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections;
using System.Collections.Generic;
using System.Net;
using AccelByte.Models;
using AccelByte.Core;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using Utf8Json;

namespace AccelByte.Api
{
    internal class OrdersApi
    {
        private readonly string baseUrl;
        private readonly UnityHttpWorker httpWorker;

        internal OrdersApi(string baseUrl, UnityHttpWorker httpWorker)
        {
            Assert.IsNotNull(baseUrl, "Creating "+ GetType().Name + " failed. Parameter baseUrl is null");
            Assert.IsNotNull(httpWorker, "Creating "+ GetType().Name + " failed. Parameter httpWorker is null");

            this.baseUrl = baseUrl;
            this.httpWorker = httpWorker;
        }

        public IEnumerator CreateOrder(string @namespace, string userId, string userAccessToken,
            OrderRequest orderRequest, ResultCallback<OrderInfo> callback)
        {
            Assert.IsNotNull(@namespace, "Can't create order! Namespace parameter is null!");
            Assert.IsNotNull(userId, "Can't create order! UserId parameter is null!");
            Assert.IsNotNull(userAccessToken, "Can't create order! UserAccessToken parameter is null!");
            Assert.IsNotNull(orderRequest, "Can't create order! OrderRequest parameter is null!");

            var builder = HttpRequestBuilder
                .CreatePost(this.baseUrl + "/public/namespaces/{namespace}/users/{userId}/orders")
                .WithPathParam("namespace", @namespace)
                .WithPathParam("userId", userId)
                .WithBearerAuth(userAccessToken)
                .WithContentType(MediaType.ApplicationJson)
                .WithBody(JsonSerializer.Serialize(orderRequest))
                .Accepts(MediaType.ApplicationJson);

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            var result = request.TryParseResponseJson<OrderInfo>();
            callback.Try(result);
        }

        public IEnumerator GetUserOrder(string @namespace, string userId, string userAccessToken, string orderNumber,
            ResultCallback<OrderInfo> callback)
        {
            Assert.IsNotNull(@namespace, "Can't get user's order! Namespace parameter is null!");
            Assert.IsNotNull(userId, "Can't get user's order! UserId parameter is null!");
            Assert.IsNotNull(userAccessToken, "Can't get user's order! UserAccessToken parameter is null!");
            Assert.IsNotNull(orderNumber, "Can't get user's order! OrderNumber parameter is null!");

            var builder = HttpRequestBuilder
                .CreateGet(this.baseUrl + "/public/namespaces/{namespace}/users/{userId}/orders/{orderNo}")
                .WithPathParam("namespace", @namespace)
                .WithPathParam("userId", userId)
                .WithPathParam("orderNo", orderNumber)
                .WithBearerAuth(userAccessToken)
                .WithContentType(MediaType.ApplicationJson)
                .Accepts(MediaType.ApplicationJson);

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            var result = request.TryParseResponseJson<OrderInfo>();
            callback.Try(result);
        }

        public IEnumerator GetUserOrders(string @namespace, string userId, string userAccessToken, uint page, uint size,
            ResultCallback<PagedOrderInfo> callback)
        {
            Assert.IsNotNull(@namespace, "Can't get user's order! Namespace parameter is null!");
            Assert.IsNotNull(userId, "Can't get user's order! UserId parameter is null!");
            Assert.IsNotNull(userAccessToken, "Can't get user's order! UserAccessToken parameter is null!");

            var builder = HttpRequestBuilder
                .CreateGet(this.baseUrl + "/public/namespaces/{namespace}/users/{userId}/orders")
                .WithPathParam("namespace", @namespace)
                .WithPathParam("userId", userId)
                .WithBearerAuth(userAccessToken)
                .WithQueryParam("page", page.ToString())
                .WithQueryParam("size", size.ToString())
                .WithContentType(MediaType.ApplicationJson)
                .Accepts(MediaType.ApplicationJson);

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            var result = request.TryParseResponseJson<PagedOrderInfo>();
            callback.Try(result);
        }

        public IEnumerator GetUserOrderHistory(string @namespace, string userId, string userAccessToken, string orderNo,
            ResultCallback<OrderHistoryInfo[]> callback)
        {
            Assert.IsNotNull(@namespace, "Can't get user's order history! Namespace parameter is null!");
            Assert.IsNotNull(userId, "Can't get user's order history! UserId parameter is null!");
            Assert.IsNotNull(userAccessToken, "Can't get user's order history! UserAccessToken parameter is null!");
            Assert.IsNotNull(orderNo, "Can't get user's order history! OrderNo parameter is null!");

            var builder = HttpRequestBuilder
                .CreateGet(this.baseUrl + "/public/namespaces/{namespace}/users/{userId}/orders/{orderNo}/history")
                .WithPathParam("namespace", @namespace)
                .WithPathParam("userId", userId)
                .WithPathParam("orderNo", orderNo)
                .WithBearerAuth(userAccessToken)
                .WithContentType(MediaType.ApplicationJson)
                .Accepts(MediaType.ApplicationJson);

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            var result = request.TryParseResponseJson<OrderHistoryInfo[]>();
            callback.Try(result);
        }
    }
}