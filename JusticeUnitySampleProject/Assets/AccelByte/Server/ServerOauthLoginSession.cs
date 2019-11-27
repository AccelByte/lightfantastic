﻿using System;
using System.Collections;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;
using UnityEngine.Assertions;
using Random = System.Random;

namespace AccelByte.Server
{
    public class ServerOauthLoginSession : ILoginSession
    {
        private const uint MaxWaitTokenRefresh = 60000;
        private const uint WaitExpiryDelay = 100;
        private static readonly TimeSpan MaxBackoffInterval = TimeSpan.FromDays(1);

        private readonly string clientId;
        private readonly string clientSecret;
        private readonly UnityHttpWorker httpWorker;
        private readonly CoroutineRunner coroutineRunner;

        private readonly string baseUrl;
        private readonly string @namespace;
        private readonly string redirecUri;

        private Coroutine maintainAccessTokenCoroutine;
        private TokenData tokenData;
        private DateTime nextRefreshTime;
        private string clientToken;
        private DateTime clientTokenExpiryTime;

        internal ServerOauthLoginSession(string baseUrl, string @namespace, string clientId, string clientSecret,
            string redirecUri, UnityHttpWorker httpWorker, CoroutineRunner coroutineRunner)
        {
            Assert.IsNotNull(baseUrl, "Creating " + GetType().Name + " failed. Parameter baseUrl is null");
            Assert.IsNotNull(@namespace, "Creating " + GetType().Name + " failed. Namespace parameter is null!");
            Assert.IsNotNull(clientId, "Creating " + GetType().Name + " failed. ClientId parameter is null!");
            Assert.IsNotNull(clientSecret, "Creating " + GetType().Name + " failed. ClientSecret parameter is null!");
            Assert.IsNotNull(redirecUri, "Creating " + GetType().Name + " failed. RedirectUri parameter is null!");
            Assert.IsNotNull(httpWorker, "Creating " + GetType().Name + " failed. Parameter httpWorker is null");
            Assert.IsNotNull(coroutineRunner, "Creating " + GetType().Name + " failed. Parameter httpWorker is null");

            this.baseUrl = baseUrl;
            this.@namespace = @namespace;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.redirecUri = redirecUri;
            this.httpWorker = httpWorker;
            this.coroutineRunner = coroutineRunner;
        }

        public string AuthorizationToken { get { return this.tokenData != null ? this.tokenData.access_token : null; } }

        public IEnumerator LoginWithClientCredential(ResultCallback callback)
        {
            IHttpRequest request = HttpRequestBuilder.CreatePost(this.baseUrl + "/oauth/token")
                .WithBasicAuth(this.clientId, this.clientSecret)
                .WithContentType(MediaType.ApplicationForm)
                .Accepts(MediaType.ApplicationJson)
                .WithFormParam("grant_type", "client_credentials")
                .GetResult();

            IHttpResponse response = null;

            yield return this.httpWorker.SendRequest(request, rsp => response = rsp);

            Result<TokenData> result = response.TryParseJson<TokenData>();
            this.tokenData = result.Value;
            Debug.Log("LoginWithClientCredential refresh_token: " + result.Value.refresh_token);
            Debug.Log("LoginWithClientCredential Namespace: " + result.Value.Namespace);
            Debug.Log("LoginWithClientCredential access_token: " + result.Value.access_token);
            Debug.Log("LoginWithClientCredential display_name: " + result.Value.display_name);

            if (!result.IsError)
            {
                // TODO Do we need  to maintain access token
                //this.maintainAccessTokenCoroutine = this.coroutineRunner.Run(MaintainAccessToken());
                Debug.Log("Login Success");
            }
            else
            {
                Debug.Log("Login ErrorCode : " + result.Error.Code);
                Debug.Log("Login Error Message : " + result.Error.Message);
            }
        }

        public IEnumerator Logout(ResultCallback callback)
        {
            var request = HttpRequestBuilder.CreatePost(this.baseUrl + "/oauth/revoke/token")
                .WithBearerAuth(this.AuthorizationToken)
                .WithContentType(MediaType.ApplicationForm)
                .Accepts(MediaType.ApplicationJson)
                .WithFormParam("token", this.AuthorizationToken)
                .GetResult();

            IHttpResponse response = null;

            yield return this.httpWorker.SendRequest(request, rsp => response = rsp);

            this.tokenData = null;
            var result = response.TryParse();
            this.coroutineRunner.Stop(this.maintainAccessTokenCoroutine);
            callback.Try(result);
        }

        private IEnumerator RefreshToken(ResultCallback<TokenData> callback)
        {
            var request = HttpRequestBuilder.CreatePost(this.baseUrl + "/oauth/token")
                .WithBasicAuth(this.clientId, this.clientSecret)
                .WithContentType(MediaType.ApplicationForm)
                .Accepts(MediaType.ApplicationJson)
                .WithFormParam("grant_type", "refresh_token")
                .WithFormParam("refresh_token", this.tokenData.refresh_token)
                .GetResult();

            IHttpResponse response = null;

            yield return this.httpWorker.SendRequest(request, rsp => response = rsp);

            Result<TokenData> result = response.TryParseJson<TokenData>();
            this.tokenData = result.Value;
            callback.Try(result);
        }

        private IEnumerator MaintainAccessToken()
        {
            TimeSpan refreshBackoff = TimeSpan.FromSeconds(10);
            var rand = new Random();

            while (true)
            {
                if (refreshBackoff >= ServerOauthLoginSession.MaxBackoffInterval)
                {
                    yield break;
                }

                if (this.tokenData == null || DateTime.UtcNow < this.nextRefreshTime)
                {
                    yield return new WaitForSeconds(ServerOauthLoginSession.WaitExpiryDelay / 1000f);

                    continue;
                }

                Result<TokenData> refreshResult = null;

                yield return RefreshToken(result => refreshResult = result);

                if (!refreshResult.IsError)
                {
                    this.nextRefreshTime = ServerOauthLoginSession.ScheduleNormalRefresh(this.tokenData.expires_in);
                }
                else
                {
                    refreshBackoff = ServerOauthLoginSession.CalculateBackoffInterval(refreshBackoff, rand.Next(1, 60));

                    this.nextRefreshTime = DateTime.UtcNow + refreshBackoff;
                }
            }
        }

        private static DateTime ScheduleNormalRefresh(int expiresIn)
        {
            return DateTime.UtcNow + TimeSpan.FromSeconds((expiresIn + 1) * 0.8);
        }

        private static TimeSpan CalculateBackoffInterval(TimeSpan previousRefreshBackoff, int randomNum)
        {
            previousRefreshBackoff = TimeSpan.FromSeconds(previousRefreshBackoff.Seconds * 2);

            return previousRefreshBackoff + TimeSpan.FromSeconds(randomNum);
        }
    }
}