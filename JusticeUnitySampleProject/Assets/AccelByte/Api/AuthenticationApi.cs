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

namespace AccelByte.Api
{
    internal class AuthenticationApi
    {
        private readonly string baseUrl;
        private readonly UnityHttpWorker httpWorker;
        private readonly string @namespace;

        internal AuthenticationApi(string baseUrl, string @namespace, UnityHttpWorker httpWorker)
        {
            Assert.IsNotNull(baseUrl, "Creating "+ GetType().Name + " failed. Parameter baseUrl is null");
            Assert.IsNotNull(httpWorker, "Creating "+ GetType().Name + " failed. Parameter httpWorker is null");
            this.baseUrl = baseUrl;
            this.@namespace = @namespace;
            this.httpWorker = httpWorker;
        }

        public IEnumerator ClientLogin(string clientId, string clientSecret, ResultCallback<TokenData> callback)
        {
            Assert.IsNotNull(clientId, "Can't generate client token; ClientId parameter is null!");
            Assert.IsNotNull(clientSecret, "Can't generate client token; ClientSecret parameter is null!");

            var builder = HttpRequestBuilder.CreatePost(this.baseUrl + "/oauth/token")
                .WithBasicAuth(clientId, clientSecret)
                .WithContentType(MediaType.ApplicationForm)
                .Accepts(MediaType.ApplicationJson)
                .WithFormParam("grant_type", "client_credentials");

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            Result<TokenData> result = request.TryParseResponseJson<TokenData>();
            callback.Try(result);
        }

        public IEnumerator LoginWithUsername(string clientId, string clientSecret, string username,
            string password, ResultCallback<SessionData> callback)
        {
            Assert.IsNotNull(clientId, "Can't generate token! ClientId parameter is null!");
            Assert.IsNotNull(clientSecret, "Can't generate token! ClientSecret parameter is null!");
            Assert.IsNotNull(username, "Can't generate token! UserName parameter is null!");
            Assert.IsNotNull(password, "Can't generate token! Password parameter is null!");

            string url;
            if (this.baseUrl.Contains("/iam"))
            {
                url = this.baseUrl.Substring(0, this.baseUrl.IndexOf("/iam"));
            }
            else
            {
                url = this.baseUrl;
            }

            var builder = HttpRequestBuilder.CreatePost(url + "/v1/login/password")
                .WithBasicAuth(clientId, clientSecret)
                .WithContentType(MediaType.ApplicationForm)
                .Accepts(MediaType.ApplicationJson)
                .WithFormParam("grant_type", "password")
                .WithFormParam("username", username)
                .WithFormParam("password", password)
                .WithFormParam("namespace", this.@namespace);

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            Result<SessionData> result = request.TryParseResponseJson<SessionData>();
            callback.Try(result);
        }

        public IEnumerator LoginWithDeviceId(string clientId, string clientSecret,
            string deviceType, string deviceId, ResultCallback<SessionData> callback)
        {
            Assert.IsNotNull(clientId, "Can't generate device token! ClientId parameter is null!");
            Assert.IsNotNull(clientSecret, "Can't generate device token! ClientSecret parameter is null!");
            Assert.IsNotNull(deviceType, "Can't generate device token! DeviceType parameter is null!");
            Assert.IsNotNull(deviceId, "Can't generate device token! DeviceId parameter is null!");

            string url;
            if (this.baseUrl.Contains("/iam"))
            {
                url = this.baseUrl.Substring(0, this.baseUrl.IndexOf("/iam"));
            }
            else
            {
                url = this.baseUrl;
            }

            var builder = HttpRequestBuilder.CreatePost(url + "/v1/login/platforms/{platformId}")
                .WithPathParam("platformId", deviceType)
                .WithBasicAuth(clientId, clientSecret)
                .WithContentType(MediaType.ApplicationForm)
                .Accepts(MediaType.ApplicationJson)
                .WithFormParam("device_id", deviceId)
                .WithFormParam("namespace", this.@namespace);

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            Result<SessionData> result = request.TryParseResponseJson<SessionData>();
            callback.Try(result);
        }

        public IEnumerator LoginWithOtherPlatform(string clientId, string clientSecret,
            PlatformType platformType, string platformToken, ResultCallback<SessionData> callback)
        {
            Assert.IsNotNull(clientId, "Can't generate platform token! ClientId parameter is null!");
            Assert.IsNotNull(clientSecret, "Can't generate platform token! ClientSecret parameter is null!");
            Assert.IsNotNull(platformToken, "Can't generate platform token! PlatformToken parameter is null!");

            string url;
            if (this.baseUrl.Contains("/iam"))
            {
                url = this.baseUrl.Substring(0, this.baseUrl.IndexOf("/iam"));
            }
            else
            {
                url = this.baseUrl;
            }

            var builder = HttpRequestBuilder.CreatePost(url + "/v1/login/platforms/{platformId}")
                .WithPathParam("platformId", platformType.ToString().ToLower())
                .WithBasicAuth(clientId, clientSecret)
                .WithContentType(MediaType.ApplicationForm)
                .Accepts(MediaType.ApplicationJson)
                .WithFormParam("platform_token", platformToken)
                .WithFormParam("namespace", this.@namespace);

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            Result<SessionData> result = request.TryParseResponseJson<SessionData>();
            callback.Try(result);
        }

        public IEnumerator LoginWithAuthorizationCode(string clientId, string clientSecret, string code,
            string redirectUri, ResultCallback<SessionData> callback)
        {
            Assert.IsNotNull(clientId, "Can't generate token from authorization code! clientId parameter is null!");
            Assert.IsNotNull(
                clientSecret,
                "Can't generate token from authorization code! ClientSecret parameter is null!");

            Assert.IsNotNull(code, "Can't generate token from authorization code! Code parameter is null!");
            Assert.IsNotNull(
                redirectUri,
                "Can't generate token from authorization code! RedirectUri parameter is null!");

            string url;
            if (this.baseUrl.Contains("/iam"))
            {
                url = this.baseUrl.Substring(0, this.baseUrl.IndexOf("/iam"));
            }
            else
            {
                url = this.baseUrl;
            }

            var builder = HttpRequestBuilder.CreatePost(url + "/v1/login/code")
                .WithBasicAuth(clientId, clientSecret)
                .WithContentType(MediaType.ApplicationForm)
                .Accepts(MediaType.ApplicationJson)
                .WithFormParam("grant_type", "authorization_code")
                .WithFormParam("code", code)
                .WithFormParam("redirect_uri", redirectUri);

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            Result<SessionData> result = request.TryParseResponseJson<SessionData>();
            callback.Try(result);
        }
        public IEnumerator Logout(string accessToken, ResultCallback callback)
        {
            Assert.IsNotNull(accessToken, "Logout failed! Token parameter is null!");

            string url;
            if (this.baseUrl.Contains("/iam"))
            {
                url = this.baseUrl.Substring(0, this.baseUrl.IndexOf("/iam"));
            }
            else
            {
                url = this.baseUrl;
            }

            var builder = HttpRequestBuilder.CreatePost(url + "/v1/logout")
                .WithBearerAuth(accessToken)
                .WithContentType(MediaType.ApplicationForm)
                .Accepts(MediaType.ApplicationJson)
                .WithFormParam("token", accessToken);

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            Result result = request.TryParseResponse();
            callback.Try(result);
        }
    }
}