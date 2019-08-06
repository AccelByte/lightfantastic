// Copyright (c) 2018 - 2019 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine;
using Random = System.Random;

namespace AccelByte.Api
{
    public interface ISession
    {
        bool IsAuthenticated { get; }
        string SessionId { get; }
        string UserId { get; }
    }

    /// <summary>
    /// User class provides convenient interaction to user authentication and account management service (AccelByte IAM).
    /// This user class will manage user credentials to be used to access other services, including refreshing its token
    /// </summary>
    public class User
    {
        public class AccelByteSession : ISession
        {
            public bool IsAuthenticated
            {
                get { return !string.IsNullOrEmpty(this.SessionId) && !string.IsNullOrEmpty(this.UserId); }
            }

            public string SessionId { get; set; }
            public string UserId { get; set; }
        }

        //Constants
        private const string AuthorizationCodeEnvironmentVariable = "JUSTICE_AUTHORIZATION_CODE";

        //Readonly members
        private readonly AuthenticationApi authApi;
        private readonly UserApi userApi;
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string redirectUri;
        private readonly CoroutineRunner coroutineRunner;
        private readonly AccelByteSession session;

        private UserData userDataCache;

        public ISession Session { get { return this.session; } }

        internal User(AuthenticationApi authApi, UserApi userApi, string @namespace, string clientId,
            string clientSecret, string redirectUri, CoroutineRunner coroutineRunner)
        {
            this.authApi = authApi;
            this.userApi = userApi;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.redirectUri = redirectUri;
            this.coroutineRunner = coroutineRunner;
            this.session = new AccelByteSession();
        }

        /// <summary>
        /// Login to AccelByte account with username (e.g. email) and password
        /// </summary>
        /// <param name="username">Could be email or phone (right now, only email supported)</param>
        /// <param name="password">Password to login</param>
        /// <param name="callback">Returns Result via callback when completed</param>
        public void LoginWithUserName(string username, string password, ResultCallback callback)
        {
            this.coroutineRunner.Run(LoginWithUserNameAsync(username, password, callback));
        }

        private IEnumerator LoginWithUserNameAsync(string email, string password, ResultCallback callback)
        {
            if (this.session.IsAuthenticated)
            {
                ResultCallback logoutCallback = null;
                yield return this.authApi.Logout(this.session.SessionId, logoutCallback);
            }

            Result<SessionData> loginResult = null;

            yield return this.authApi.LoginWithUsername(
                this.clientId,
                this.clientSecret,
                email,
                password,
                result => { loginResult = result; });

            if (loginResult.IsError)
            {
                callback.TryError(loginResult.Error);

                yield break;
            }

            this.session.SessionId = loginResult.Value.session_id;

            Result<UserData> userDataResult = null;

            yield return GetDataAsync(result => userDataResult = result);

            if (userDataResult.IsError)
            {
                callback.TryError(userDataResult.Error);
                
                yield break;
            }

            this.userDataCache = userDataResult.Value;
            this.session.UserId = this.userDataCache.UserId;

            callback.TryOk();
        }

        /// <summary>
        /// Login with token from non AccelByte platforms. This will automatically register a user if the user
        /// identified by its platform type and platform token doesn't exist yet. A user registered with this method
        /// is called a headless account because it doesn't have username yet.
        /// </summary>
        /// <param name="platformType">Other platform type</param>
        /// <param name="platformToken">Token for other platfrom type</param>
        /// <param name="callback">Returns Result via callback when completed</param>
        public void LoginWithOtherPlatform(PlatformType platformType, string platformToken, ResultCallback callback)
        {
            this.coroutineRunner.Run(LoginWithOtherPlatformAsync(platformType, platformToken, callback));
        }

        private IEnumerator LoginWithOtherPlatformAsync(PlatformType platformType, string platformToken,
            ResultCallback callback)
        {
            if (this.session.IsAuthenticated)
            {
                ResultCallback logoutCallback = null;
                yield return this.authApi.Logout(this.session.SessionId, logoutCallback);
            }

            Result<SessionData> loginResult = null;

            yield return this.authApi.LoginWithOtherPlatform(
                this.clientId,
                this.clientSecret,
                platformType,
                platformToken,
                result => loginResult = result);

            if (loginResult.IsError)
            {
                callback.TryError(loginResult.Error);

                yield break;
            }

            this.session.SessionId = loginResult.Value.session_id;

            Result<UserData> userDataResult = null;

            yield return GetDataAsync(result => userDataResult = result);

            if (userDataResult.IsError)
            {
                callback.TryError(userDataResult.Error);
                
                yield break;
            }

            this.userDataCache = userDataResult.Value;
            this.session.UserId = this.userDataCache.UserId;

            callback.TryOk();
        }

        /// <summary>
        /// Login With AccelByte Launcher. Use this only if you integrate your game with AccelByte Launcher
        /// </summary>
        /// <param name="callback">Returns Result via callback when completed</param>
        public void LoginWithLauncher(ResultCallback callback)
        {
            string authCode = Environment.GetEnvironmentVariable(User.AuthorizationCodeEnvironmentVariable);

            this.coroutineRunner.Run(LoginWithLauncherAsync(authCode, callback));
        }

        private IEnumerator LoginWithLauncherAsync(string authCode, ResultCallback callback)
        {
            if (this.session.IsAuthenticated)
            {
                ResultCallback logoutCallback = null;
                yield return this.authApi.Logout(this.session.SessionId, logoutCallback);
            }

            Result<SessionData> loginResult = null;

            yield return this.authApi.LoginWithAuthorizationCode(
                this.clientId,
                this.clientSecret,
                authCode,
                this.redirectUri,
                result => { loginResult = result; });

            if (loginResult.IsError)
            {
                callback.TryError(loginResult.Error);

                yield break;
            }

            this.session.SessionId = loginResult.Value.session_id;

            Result<UserData> userDataResult = null;

            yield return GetDataAsync(result => userDataResult = result);

            if (userDataResult.IsError)
            {
                callback.TryError(userDataResult.Error);
                
                yield break;
            }

            this.userDataCache = userDataResult.Value;
            this.session.UserId = this.userDataCache.UserId;

            callback.TryOk();
        }

        /// <summary>
        /// Login with device id. A user registered with this method is called a headless account because it doesn't
        /// have username yet.
        /// </summary>
        /// <param name="callback">Returns Result via callback when completed</param>
        public void LoginWithDeviceId(ResultCallback callback)
        {
            DeviceProvider deviceProvider = DeviceProvider.GetFromSystemInfo();

            this.coroutineRunner.Run(LoginWithDeviceIdAsync(deviceProvider, callback));
        }

        private IEnumerator LoginWithDeviceIdAsync(DeviceProvider deviceProvider, ResultCallback callback)
        {
            if (this.session.IsAuthenticated)
            {
                ResultCallback logoutCallback = null;
                yield return this.authApi.Logout(this.session.SessionId, logoutCallback);
            }

            Result<SessionData> loginResult = null;

            yield return this.authApi.LoginWithDeviceId(
                this.clientId,
                this.clientSecret,
                deviceProvider.DeviceType,
                deviceProvider.DeviceId,
                result => loginResult = result);

            if (loginResult.IsError)
            {
                callback.TryError(loginResult.Error);

                yield break;
            }

            this.session.SessionId = loginResult.Value.session_id;

            Result<UserData> userDataResult = null;

            yield return GetDataAsync(result => userDataResult = result);

            if (userDataResult.IsError)
            {
                callback.TryError(userDataResult.Error);
                
                yield break;
            }

            this.userDataCache = userDataResult.Value;
            this.session.UserId = this.userDataCache.UserId;

            callback.TryOk();
        }

        /// <summary>
        /// Logout current user session
        /// </summary>
        public void Logout(ResultCallback callback)
        {
            if (!this.session.IsAuthenticated)
            {
                callback.TryOk();

                return;
            }

            this.coroutineRunner.Run(this.authApi.Logout(this.session.SessionId, callback));
            this.session.SessionId = null;
            this.session.UserId = null;
        }

        /// <summary>
        /// Register a user by giving username, password, and displayName 
        /// </summary>
        /// <param name="userName">Username to identify (and verify) user with (email or phone)</param>
        /// <param name="password">Password to login</param>
        /// <param name="displayName">Any string can be used as display name, make it more flexible than Usernam</param>
        /// <param name="callback">Returns a Result that contains UserData via callback</param>
        public void Register(string userName, string password, string displayName, string country, DateTime dateOfBirth, ResultCallback<UserData> callback)
        {
            var registerUserRequest = new RegisterUserRequest
            {
                AuthType = AuthenticationType.EMAILPASSWD, //Right now, it's hardcoded to email
                UserName = userName,
                Password = password,
                DisplayName = displayName,
                Country = country, 
                DateOfBirth = dateOfBirth.ToString("yyyy-MM-dd")
            };

            this.coroutineRunner.Run(this.userApi.Register(registerUserRequest, callback));
        }

        /// <summary>
        /// Get current logged in user data. It will return cached user data if it has been called before
        /// </summary>
        /// <param name="callback">Returns a Result that contains UserData via callback</param>
        public void GetData(ResultCallback<UserData> callback)
        {
            if (!this.session.IsAuthenticated)
            {
                callback.TryError(ErrorCode.IsNotLoggedIn);

                return;
            }

            this.coroutineRunner.Run(GetDataAsync(callback));
        }

        private IEnumerator GetDataAsync(ResultCallback<UserData> callback)
        {
            Result<UserData> result = null;

            yield return this.userApi.GetData(this.session.SessionId, r => result = r);

            if (!result.IsError)
            {
                if (this.userDataCache != result.Value)
                {
                    this.userDataCache = result.Value;
                }
                else
                {
                    if (this.userDataCache != null)
                    {

                        callback.TryOk(this.userDataCache);

                        yield break;
                    }
                }
            }

            callback.Try(result);
        }

        /// <summary>
        /// Refresh currrent cached user data.
        /// </summary>
        /// <param name="callback">Returns a Result that contains UserData via callback</param>
        public void RefreshData(ResultCallback<UserData> callback)
        {
            if (!this.session.IsAuthenticated)
            {
                callback.TryError(ErrorCode.IsNotLoggedIn);

                return;
            }

            this.userDataCache = null;

            this.coroutineRunner.Run(GetDataAsync(callback));
        }

        /// <summary>
        /// Update some user information (e.g. language or country)
        /// </summary>
        /// <param name="updateRequest">Set its field if you want to change it, otherwise leave it</param>
        /// <param name="callback">Returns a Result that contains UserData via callback when completed</param>
        public void Update(UpdateUserRequest updateRequest, ResultCallback<UserData> callback)
        {
            if (!this.session.IsAuthenticated)
            {
                callback.TryError(ErrorCode.IsNotLoggedIn);

                return;
            }

            this.coroutineRunner.Run(UpdateAsync(updateRequest, callback));
        }

        private IEnumerator UpdateAsync(UpdateUserRequest updateRequest, ResultCallback<UserData> callback)
        {
            Result<UserData> updateResult = null;

            yield return this.userApi.Update(this.session.SessionId, updateRequest, result => updateResult = result);

            if (!updateResult.IsError)
            {
                this.userDataCache = updateResult.Value;
            }

            callback.Try(updateResult);
        }

        /// <summary>
        /// Upgrade a headless account with username and password. User must be logged in before this method can be
        /// used.
        /// </summary>
        /// <param name="userName">Username the user is upgraded to</param>
        /// <param name="password">Password to login with username</param>
        /// <param name="callback">Returns a Result that contains UserData via callback when completed</param>
        public void Upgrade(string userName, string password, ResultCallback<UserData> callback)
        {
            if (!this.session.IsAuthenticated)
            {
                callback.TryError(ErrorCode.IsNotLoggedIn);

                return;
            }

            this.coroutineRunner.Run(UpgradeAsync(userName, password, callback));
        }

        private IEnumerator UpgradeAsync(string username, string password, ResultCallback<UserData> callback)
        {
            Result<UserData> result = null;

            yield return this.userApi.Upgrade(this.session.SessionId, username, password, r => result = r);

            if (!result.IsError)
            {
                this.userDataCache = result.Value;
            }

            callback.Try(result);
        }

        /// <summary>
        /// Trigger an email that contains verification code to be sent to user's email. User must be logged in.
        /// </summary>
        /// <param name="callback">Returns a Result via callback when completed</param>
        public void SendVerificationCode(ResultCallback callback)
        {
            if (!this.session.IsAuthenticated)
            {
                callback.TryError(ErrorCode.IsNotLoggedIn);

                return;
            }

            this.coroutineRunner.Run(
                this.userApi.SendVerificationCode(
                    this.session.SessionId,
                    VerificationContext.UserAccountRegistration,
                    this.userDataCache.LoginId,
                    callback));
        }

        /// <summary>
        /// Verify a user via an email registered as its username. User must be logged in.
        /// </summary>
        /// <param name="verificationCode">Verification code received from user's email</param>
        /// <param name="callback">Returns Result via callback when completed</param>
        public void Verify(string verificationCode, ResultCallback callback)
        {
            if (!this.session.IsAuthenticated)
            {
                callback.TryError(ErrorCode.IsNotLoggedIn);

                return;
            }

            //TODO: Hard-coded contact type, if phone is activated in the future, we should add UserName to User
            //class and determine whether it's email or phone by regex.
            this.coroutineRunner.Run(this.userApi.Verify(this.session.SessionId, verificationCode, "email", callback));
        }

        /// <summary>
        /// Trigger an email that contains verification code to be sent to user's email, if he/she wants to also verify
        /// while upgrading.
        /// </summary>
        /// <param name="userName">Username to upgrade to (headless/device account doesn't have username)</param>
        /// <param name="callback">Returns Result via callback when completed</param>
        public void SendUpgradeVerificationCode(string userName, ResultCallback callback)
        {
            if (!this.session.IsAuthenticated)
            {
                callback.TryError(ErrorCode.IsNotLoggedIn);

                return;
            }

            this.coroutineRunner.Run(
                this.userApi.SendVerificationCode(
                    this.session.SessionId,
                    VerificationContext.UpgradeHeadlessAccount,
                    userName,
                    callback));
        }

        //---------------------not implemented yet on api-gateway----------------------
        /// <summary>
        /// Upgrade and verify the upgraded account. User must be logged in
        /// </summary>
        /// <param name="userName">Username to upgrade</param>
        /// <param name="password">Password to login</param>
        /// <param name="verificationCode">Verification code retrieved from email or phone </param>
        /// <param name="callback">Returns Result that contains UserData via callback when completed</param>
        //public void UpgradeAndVerify(string userName, string password, string verificationCode,
        //    ResultCallback<UserData> callback)
        //{
        //    if (!this.session.IsAuthenticated)
        //    {
        //        callback.TryError(ErrorCode.IsNotLoggedIn);

        //        return;
        //    }

        //    this.coroutineRunner.Run(UpgradeAndVerifyAsync(userName, password, verificationCode, callback));
        //}

        //private IEnumerator UpgradeAndVerifyAsync(string userName, string password, string verificationCode,
        //    ResultCallback<UserData> callback)
        //{
        //    Result<UserData> upgradeAndVerifyResult = null;

        //    yield return this.userApi.UpgradeAndVerify(
        //        this.session.SessionId,
        //        this.session.UserId,
        //        userName,
        //        password,
        //        verificationCode,
        //        result => { upgradeAndVerifyResult = result; });

        //    if (!upgradeAndVerifyResult.IsError)
        //    {
        //        this.userDataCache = upgradeAndVerifyResult.Value;

        //        yield return null;
        //    }

        //    callback.Try(upgradeAndVerifyResult);
        //}
        //--------------------------------------------------------------------

            /// <summary>
            /// Trigger an email that contains reset password code to be sent to user
            /// </summary>
            /// <param name="userName">Username to be sent reset password code to.</param>
            /// <param name="callback">Returns a Result via callback when completed</param>
            public void SendResetPasswordCode(string userName, ResultCallback callback)
        {
            this.coroutineRunner.Run(this.userApi.SendPasswordResetCode(userName, callback));
        }

        /// <summary>
        /// Reset password for a username
        /// </summary>
        /// <param name="resetCode">Reset password code</param>
        /// <param name="userName">Username with forgotten password</param>
        /// <param name="newPassword">New password</param>
        /// <param name="callback">Returns a Result via callback when completed</param>
        public void ResetPassword(string resetCode, string userName, string newPassword, ResultCallback callback)
        {
            this.coroutineRunner.Run(this.userApi.ResetPassword(resetCode, userName, newPassword, callback));
        }

        /// <summary>
        /// Link other platform to the currently logged in user. 
        /// </summary>
        /// <param name="platformType">Other platform's type (Google, Steam, Facebook, etc)</param>
        /// <param name="platformTicket">Ticket / token from other platform to be linked to </param>
        /// <param name="callback">Returns a Result via callback when completed</param>
        public void LinkOtherPlatform(PlatformType platformType, string platformTicket, ResultCallback callback)
        {
            if (!this.session.IsAuthenticated)
            {
                callback.TryError(ErrorCode.IsNotLoggedIn);

                return;
            }

            this.coroutineRunner.Run(
                this.userApi.LinkOtherPlatform(
                    this.session.SessionId,
                    platformType.ToString().ToLower(),
                    platformTicket,
                    callback));
        }

        /// <summary>
        /// Unlink other platform that has been linked to the currently logged in user. The change will take effect
        /// after user has been re-login.
        /// </summary>
        /// <param name="platformType">Other platform's type (Google, Steam, Facebook, etc)</param>
        /// <param name="platformTicket">Ticket / token from other platform to unlink from this user</param>
        /// <param name="callback">Returns a result via callback when completed</param>
        public void UnlinkOtherPlatform(PlatformType platformType, string platformTicket, ResultCallback callback)
        {
            if (!this.session.IsAuthenticated)
            {
                callback.TryError(ErrorCode.IsNotLoggedIn);

                return;
            }

            this.coroutineRunner.Run(
                this.userApi.UnlinkOtherPlatform(this.session.SessionId, platformType.ToString(), callback));
        }

        /// <summary>
        /// Get array of other platforms this user linked to
        /// </summary>
        /// <param name="callback">Returns a Result that contains PlatformLink array via callback when
        /// completed.</param>
        public void GetPlatformLinks(ResultCallback<PlatformLink[]> callback)
        {
            if (!this.session.IsAuthenticated)
            {
                callback.TryError(ErrorCode.IsNotLoggedIn);

                return;
            }

            this.coroutineRunner.Run(this.userApi.GetPlatformLinks(this.session.SessionId, callback));
        }

        /// <summary>
        /// Get user data from another user by login id or email
        /// </summary>
        /// <param name="loginId"> email or login id that needed to get user data</param>
        /// <param name="callback"> Return a Result that contains UserData when completed. </param>
        public void GetUserByLoginId(string loginId, ResultCallback<UserData> callback)
        {
            if (!this.session.IsAuthenticated)
            {
                callback.TryError(ErrorCode.IsNotLoggedIn);

                return;
            }

            this.coroutineRunner.Run(this.userApi.GetUserByLoginId(this.session.SessionId, loginId, callback));
        }

        /// <summary>
        /// Get user data from another user by user id
        /// </summary>
        /// <param name="userId"> user id that needed to get user data</param>
        /// <param name="callback"> Return a Result that contains UserData when completed. </param>
        public void GetUserByUserId(string userId, ResultCallback<UserData> callback)
        {
            if (!this.session.IsAuthenticated)
            {
                callback.TryError(ErrorCode.IsNotLoggedIn);

                return;
            }

            this.coroutineRunner.Run(this.userApi.GetUserByUserId(this.session.SessionId, userId, callback));
        }
    }
}