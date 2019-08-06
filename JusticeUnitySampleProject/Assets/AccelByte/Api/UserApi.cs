// Copyright (c) 2018 - 2019 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections;
using AccelByte.Models;
using AccelByte.Core;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using Utf8Json;

namespace AccelByte.Api
{
    internal class UserApi
    {
        private readonly string baseUrl;
        private readonly string @namespace;
        private readonly UnityHttpWorker httpWorker;

        internal UserApi(string baseUrl, string @namespace, UnityHttpWorker httpWorker)
        {
            Assert.IsNotNull(baseUrl, "Creating " + GetType().Name + " failed. Parameter baseUrl is null");
            Assert.IsNotNull(httpWorker, "Creating " + GetType().Name + " failed. Parameter httpWorker is null");
            Assert.IsFalse(
                string.IsNullOrEmpty(@namespace),
                "Creating " + GetType().Name + " failed. Parameter ns is null.");

            this.@namespace = @namespace;
            this.baseUrl = baseUrl;
            this.httpWorker = httpWorker;
        }

        public IEnumerator Register(RegisterUserRequest registerUserRequest, ResultCallback<UserData> callback)
        {
            Assert.IsNotNull(registerUserRequest, "Can't create user! Info parameter is null!");

            string jsonInfo = string.Format(
                "{{" +
                "\"AuthType\": \"{0}\"," +
                "\"DisplayName\": \"{1}\"," +
                "\"LoginId\": \"{2}\"," +
                "\"Password\": \"{3}\"," +
                "\"Country\": \"{4}\"," +
                "\"DateOfBirth\": \"{5}\"" +
                "}}",
                registerUserRequest.AuthType,
                registerUserRequest.DisplayName,
                registerUserRequest.UserName,
                registerUserRequest.Password,
                registerUserRequest.Country,
                registerUserRequest.DateOfBirth);

            var builder = HttpRequestBuilder.CreatePost(this.baseUrl + "/v3/public/namespaces/{namespace}/users")
                .WithPathParam("namespace", this.@namespace)
                .WithContentType(MediaType.ApplicationJson)
                .WithBody(jsonInfo);

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            Result<UserData> result = request.TryParseResponseJson<UserData>();
            callback.Try(result);
        }

        public IEnumerator GetData(string userAccessToken, ResultCallback<UserData> callback)
        {
            Assert.IsNotNull(userAccessToken, "Can't get user data! UserAccessToken parameter is null!");

            var builder = HttpRequestBuilder.CreateGet(this.baseUrl + "/v2/public/users/me")
                .WithBearerAuth(userAccessToken)
                .Accepts(MediaType.ApplicationJson);

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            Result<UserData> result = request.TryParseResponseJson<UserData>();
            callback.Try(result);
        }

        public IEnumerator Update(string userAccessToken, UpdateUserRequest updateUserRequest,
            ResultCallback<UserData> callback)
        {
            Assert.IsNotNull(userAccessToken, "Can't update user! UserAccessToken parameter is null!");
            Assert.IsNotNull(updateUserRequest, "Can't update user! Request parameter is null!");

            var builder = HttpRequestBuilder.CreatePatch(this.baseUrl + "/v2/public/users/me")
                .WithBearerAuth(userAccessToken)
                .WithContentType(MediaType.ApplicationJson)
                .WithBody(JsonSerializer.Serialize(updateUserRequest))
                .Accepts(MediaType.ApplicationJson);

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            Result<UserData> result = request.TryParseResponseJson<UserData>();
            callback.Try(result);
        }

        public IEnumerator Upgrade(string userAccessToken, string userName, string password,
            ResultCallback<UserData> callback)
        {
            Assert.IsNotNull(userAccessToken, "Can't upgrade headless account! ClientAccessToken parameter is null!");
            Assert.IsNotNull(userName, "Can't upgrade headless account! UserName parameter is null!");
            Assert.IsNotNull(password, "Can't upgrade headless account! Password parameter is null!");

            var builder = HttpRequestBuilder
                .CreatePost(this.baseUrl + "/v3/public/namespaces/{namespace}/users/me/headless/verify")
                .WithPathParam("namespace", this.@namespace)
                .WithBearerAuth(userAccessToken)
                .WithContentType(MediaType.ApplicationJson)
                .WithBody(string.Format("{{\"LoginID\": \"{0}\", \"Password\": \"{1}\"}}", userName, password));

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            Result<UserData> result = request.TryParseResponseJson<UserData>();
            callback.Try(result);
        }

        public IEnumerator SendVerificationCode(string userAccessToken, VerificationContext context, string username,
            ResultCallback callback)
        {
            Assert.IsNotNull(userAccessToken, "Can't send verification code! UserAccessToken parameter is null!");
            Assert.IsNotNull(username, "Can't send verification code! Username parameter is null!");

            var builder = HttpRequestBuilder
                .CreatePost(this.baseUrl + "/v3/public/namespaces/{namespace}/users/me/code/request")
                .WithPathParam("namespace", this.@namespace)
                .WithBearerAuth(userAccessToken)
                .WithContentType(MediaType.ApplicationJson)
                .WithBody(string.Format("{{\"LoginID\": \"{0}\", \"Context\": \"{1:G}\"}}", username, context));

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            Result result = request.TryParseResponse();
            callback.Try(result);
        }

        public IEnumerator Verify(string userAccessToken, string verificationCode, string contactType,
            ResultCallback callback)
        {
            Assert.IsNotNull(userAccessToken, "Can't post verification code! UserAccessToken parameter is null!");
            Assert.IsNotNull(verificationCode, "Can't post verification code! VerificationCode parameter is null!");
            Assert.IsNotNull(contactType, "Can't post verification code! ContactType parameter is null!");

            var builder = HttpRequestBuilder
                .CreatePost(this.baseUrl + "/v3/public/namespaces/{namespace}/users/me/code/verify")
                .WithPathParam("namespace", this.@namespace)
                .WithBearerAuth(userAccessToken)
                .WithContentType(MediaType.ApplicationJson)
                .WithBody(
                    string.Format("{{\"Code\": \"{0}\", \"ContactType\": \"{1}\"}}", verificationCode, contactType));

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            Result result = request.TryParseResponse();
            callback.Try(result);
        }

        // Not implemented yet on api-gateway
        //public IEnumerator UpgradeAndVerify(string userAccessToken, string userId, string userName,
        //    string password, string verificationCode, ResultCallback<UserData> callback)
        //{
        //    Assert.IsNotNull(userAccessToken, "Can't upgrade headless account! ClientAccessToken parameter is null!");
        //    Assert.IsNotNull(userId, "Can't upgrade headless account! UserId parameter is null!");
        //    Assert.IsNotNull(userName, "Can't upgrade headless account! UserName parameter is null!");
        //    Assert.IsNotNull(password, "Can't upgrade headless account! Password parameter is null!");
        //    Assert.IsNotNull(verificationCode, "Can't upgrade headless account! Password parameter is null!");

        //    string requestBody = string.Format(
        //        "{{\"Code\":\"{0}\", \"LoginID\": \"{1}\", \"Password\": \"{2}\"}}",
        //        verificationCode,
        //        userName,
        //        password);

        //    var builder = HttpRequestBuilder
        //        .CreatePost(
        //            this.baseUrl + "/v3/public/namespaces/{namespace}/users/{userId}/headless/code/verify")
        //        .WithPathParam("namespace", this.@namespace)
        //        .WithPathParam("userId", userId)
        //        .WithBearerAuth(userAccessToken)
        //        .WithContentType(MediaType.ApplicationJson)
        //        .WithBody(requestBody);

        //    UnityWebRequest request = null;

        //    yield return this.httpWorker.SendWithRetry(builder, req => request = req);

        //    Result<UserData> result = request.TryParseResponseJson<UserData>();
        //    callback.Try(result);
        //}

        public IEnumerator SendPasswordResetCode(string userName, ResultCallback callback)
        {
            
            Assert.IsNotNull(userName, "Can't request reset password code! LoginId parameter is null!");

            var builder = HttpRequestBuilder.CreatePost(this.baseUrl + "/v2/public/namespaces/{namespace}/users/forgotPassword")
                .WithPathParam("namespace", this.@namespace)
                .WithContentType(MediaType.ApplicationJson)
                .WithBody(string.Format("{{\"LoginID\": \"{0}\"}}", userName));

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            if (request == null)
            {
                callback.Try(Result.CreateError(ErrorCode.NetworkError, "There is no response"));

                yield break;
            }

            Result result = request.TryParseResponse();
            callback.Try(result);
        }

        public IEnumerator ResetPassword(string resetCode, string userName, string newPassword, ResultCallback callback)
        {
            string jsonResetRequest = string.Format(
                "{{" + "\"Code\": \"{0}\"," + "\"LoginID\": \"{1}\"," + "\"NewPassword\": \"{2}\"" + "}}",
                resetCode,
                userName,
                newPassword);

            var builder = HttpRequestBuilder
                .CreatePost(this.baseUrl + "/v2/public/namespaces/{namespace}/users/resetPassword")
                .WithPathParam("namespace", this.@namespace)
                .WithContentType(MediaType.ApplicationJson)
                .WithBody(jsonResetRequest);

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            Result result = request.TryParseResponse();
            callback.Try(result);
        }

        public IEnumerator LinkOtherPlatform(string userAccessToken, string platformId, string ticket,
            ResultCallback callback)
        {
            Assert.IsNotNull(userAccessToken, "Can't link platfrom account! UserAccessToken parameter is null!");
            Assert.IsNotNull(platformId, "Can't link platfrom account! Email parameter is null!");
            Assert.IsNotNull(ticket, "Can't link platfrom account! Password parameter is null!");

            var builder = HttpRequestBuilder
                .CreatePost(this.baseUrl + "/v2/public/namespaces/{namespace}/users/me/platforms/{platformId}/link")
                .WithPathParam("namespace", this.@namespace)
                .WithPathParam("platformId", platformId)
                .WithFormParam("ticket", ticket)
                .WithBearerAuth(userAccessToken)
                .Accepts(MediaType.ApplicationJson)
                .WithContentType(MediaType.ApplicationForm);

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            Result result = request.TryParseResponse();
            callback.Try(result);
        }

        public IEnumerator UnlinkOtherPlatform(string userAccessToken, string platformId, ResultCallback callback)
        {
            Assert.IsNotNull(userAccessToken, "Can't unlink platfrom account! UserAccessToken parameter is null!");
            Assert.IsNotNull(platformId, "Can't unlink platfrom account! Email parameter is null!");

            var builder = HttpRequestBuilder
                .CreateDelete(
                    this.baseUrl + "/v2/public/namespaces/{namespace}/users/me/platforms/{platformId}/link")
                .WithPathParam("namespace", this.@namespace)
                .WithPathParam("platformId", platformId)
                .WithBearerAuth(userAccessToken)
                .WithContentType(MediaType.TextPlain);

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            Result result = request.TryParseResponse();
            callback.Try(result);
        }

        public IEnumerator GetPlatformLinks(string userAccessToken, ResultCallback<PlatformLink[]> callback)
        {
            Assert.IsNotNull(userAccessToken, "Can't get linked platfrom account! userAccessToken parameter is null!");

            var builder = HttpRequestBuilder
                .CreateGet(this.baseUrl + "/v3/public/namespaces/{namespace}/users/me/platforms")
                .WithPathParam("namespace", this.@namespace)
                .WithContentType(MediaType.ApplicationJson)
                .WithBearerAuth(userAccessToken)
                .Accepts(MediaType.ApplicationJson);

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            Result<PlatformLink[]> result = request.TryParseResponseJson<PlatformLink[]>();
            callback.Try(result);
        }

        public IEnumerator GetUserByLoginId(string userAccessToken, string loginId, ResultCallback<UserData> callback)
        {
            Assert.IsNotNull(userAccessToken, "Can't get user data! UserAccessToken parameter is null!");
            Assert.IsNotNull(loginId, "Can't get user data! loginId parameter is null!");

            var builder = HttpRequestBuilder
                .CreateGet(this.baseUrl + "/namespaces/{namespace}/users/byLoginId")
                .WithPathParam("namespace", this.@namespace)
                .WithBearerAuth(userAccessToken)
                .Accepts(MediaType.ApplicationJson)
                .WithContentType(MediaType.ApplicationJson)
                .WithQueryParam("loginId", loginId); ;

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            Result<UserData> result = request.TryParseResponseJson<UserData>();
            callback.Try(result);
        }

        public IEnumerator GetUserByUserId(string userAccessToken, string userId, ResultCallback<UserData> callback)
        {
            Assert.IsNotNull(userAccessToken, "Can't get user data! UserAccessToken parameter is null!");
            Assert.IsNotNull(userId, "Can't get user data! userId parameter is null!");

            var builder = HttpRequestBuilder
                .CreateGet(this.baseUrl + "/namespaces/{namespace}/users/{userId}")
                .WithPathParam("namespace", this.@namespace)
                .WithPathParam("userId", userId)
                .WithBearerAuth(userAccessToken)
                .Accepts(MediaType.ApplicationJson);

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            Result<UserData> result = request.TryParseResponseJson<UserData>();
            callback.Try(result);
        }
    }
}