// Copyright (c) 2018 - 2019 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;

namespace AccelByte.Server
{
    /// <summary>
    /// ServerCredentials class provides convenient interaction to user authentication and account management service (AccelByte IAM).
    /// This user class will manage ServerCredentials credentials to be used to access other services, including refreshing its token
    /// </summary>

    public class ServerCredentials
    {
        private readonly ILoginSession loginSession;
        private readonly AccelByteServerSession sessionAdapter;
        private readonly CoroutineRunner coroutineRunner;

        internal ServerCredentials(ILoginSession loginSession, CoroutineRunner coroutineRunner)
        {
            this.loginSession = loginSession;
            this.coroutineRunner = coroutineRunner;
            this.sessionAdapter = new AccelByteServerSession();
        }

        public AccelByteServerSession Session { get { return this.sessionAdapter; } }

        public void GetAccessToken(ResultCallback callback)
        {
            this.coroutineRunner.Run(GetAccessTokenAsync(callback));
        }

        private IEnumerator LoginAsync(Func<ResultCallback, IEnumerator> loginMethod, ResultCallback callback)
        {
            if (this.sessionAdapter.IsValid())
            {
                callback.TryError(ErrorCode.InvalidRequest, "User is already logged in.");

                yield break;
            }

            Result loginResult = null;

            yield return loginMethod(r => loginResult = r);

            if (loginResult.IsError)
            {
                callback.TryError(loginResult.Error);

                yield break;
            }

            this.sessionAdapter.AuthorizationToken = this.loginSession.AuthorizationToken;

            callback.TryOk();
        }

        /// <summary>
        /// GetAccessTokenAsync login using client credentials (client id and client secret) to get the token
        /// </summary>
        /// <param name="callback"> returns token data parameters </param>
        /// <returns></returns>
        private IEnumerator GetAccessTokenAsync(ResultCallback callback)
        {
            yield return this.loginSession.LoginWithClientCredential(
                callback);

            Debug.Log("ServerCredentials GetAccessTokenAsync loginSession auth token: " + this.loginSession.AuthorizationToken);
            this.sessionAdapter.AuthorizationToken = this.loginSession.AuthorizationToken;

            Debug.Log("ServerCredentials GetAccessTokenAsync sessionAdapter auth token: " + this.sessionAdapter.AuthorizationToken);

            callback.TryOk();
        }

        /// <summary>
        /// Logout current user session
        /// </summary>
        public void Logout(ResultCallback callback)
        {
            if (!this.sessionAdapter.IsValid())
            {
                callback.TryOk();

                return;
            }

            this.coroutineRunner.Run(this.loginSession.Logout(callback));
        }
    }
}