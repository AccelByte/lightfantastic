// Copyright (c) 2018 - 2019 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine.Assertions;
using UnityEngine.Networking;

namespace AccelByte.Api
{
    public class CloudStorageApi
    {
        private readonly string baseUrl;
        private readonly UnityHttpWorker httpWorker;

        internal CloudStorageApi(string baseUrl, UnityHttpWorker httpWorker)
        {
            Assert.IsNotNull(baseUrl, "Creating "+ GetType().Name + " failed. Parameter baseUrl is null");
            Assert.IsNotNull(httpWorker, "Creating "+ GetType().Name + " failed. Parameter httpWorker is null");
            this.baseUrl = baseUrl;
            this.httpWorker = httpWorker;
        }

        public IEnumerator GetAllSlots(string @namespace, string userId, string accessToken,
            ResultCallback<Slot[]> callback)
        {
            Assert.IsNotNull(@namespace, "Can't get all slots! namespace parameter is null!");
            Assert.IsNotNull(userId, "Can't get all slots! userId parameter is null!");
            Assert.IsNotNull(accessToken, "Can't get all slots! accessToken parameter is null!");

            var builder = HttpRequestBuilder
                .CreateGet(this.baseUrl + "/public/namespaces/{namespace}/users/{userId}/slots")
                .WithPathParam("namespace", @namespace)
                .WithPathParam("userId", userId)
                .WithBearerAuth(accessToken)
                .WithContentType(MediaType.ApplicationJson)
                .Accepts(MediaType.ApplicationJson);

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            var result = request.TryParseResponseJson<Slot[]>();
            callback.Try(result);
        }

        public IEnumerator GetSlot(string @namespace, string userId, string accessToken, string slotId,
            ResultCallback<byte[]> callback)
        {
            Assert.IsNotNull(@namespace, "Can't get the slot! namespace parameter is null!");
            Assert.IsNotNull(userId, "Can't get the slot! userId parameter is null!");
            Assert.IsNotNull(accessToken, "Can't get the slot! accessToken parameter is null!");
            Assert.IsNotNull(slotId, "Can't get the slot! slotId parameter is null!");

            var builder = HttpRequestBuilder
                .CreateGet(this.baseUrl + "/public/namespaces/{namespace}/users/{userId}/slots/{slotId}")
                .WithPathParam("namespace", @namespace)
                .WithPathParam("userId", userId)
                .WithPathParam("slotId", slotId)
                .WithBearerAuth(accessToken)
                .WithContentType(MediaType.ApplicationJson)
                .Accepts(MediaType.OctedStream);

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            Result<byte[]> result;

            switch ((HttpStatusCode) request.responseCode)
            {
            case HttpStatusCode.OK:
                result = Result<byte[]>.CreateOk(request.downloadHandler.data);

                break;

            default:
                result = Result<byte[]>.CreateError((ErrorCode) request.responseCode);

                break;
            }

            callback.Try(result);
        }

        public IEnumerator CreateSlot(string @namespace, string userId, string accessToken, byte[] data,
            string filename, ResultCallback<Slot> callback)
        {
            Assert.IsNotNull(@namespace, "Can't create a slot! namespace parameter is null!");
            Assert.IsNotNull(userId, "Can't create a slot! userId parameter is null!");
            Assert.IsNotNull(accessToken, "Can't create a slot! accessToken parameter is null!");
            Assert.IsNotNull(data, "Can't create a slot! data parameter is null!");
            Assert.IsNotNull(filename, "Can't create a slot! filename parameter is null!");

            string checkSum;

            using (MD5 md5 = MD5.Create())
            {
                byte[] computeHash = md5.ComputeHash(data);
                checkSum = BitConverter.ToString(computeHash).Replace("-", "");
            }

            FormDataContent formDataContent = new FormDataContent();
            formDataContent.Add(data, filename);

            var builder = HttpRequestBuilder
                .CreatePost(this.baseUrl + "/public/namespaces/{namespace}/users/{userId}/slots")
                .WithPathParam("namespace", @namespace)
                .WithPathParam("userId", userId)
                .WithQueryParam("checksum", checkSum)
                .Accepts(MediaType.ApplicationJson)
                .WithBearerAuth(accessToken)
                .WithContentType(formDataContent.GetMediaType())
                .WithBody(formDataContent);

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            var result = request.TryParseResponseJson<Slot>();
            callback.Try(result);
        }

        public IEnumerator UpdateSlot(string @namespace, string userId, string accessToken, string slotId, byte[] data,
            string filename, ResultCallback<Slot> callback)
        {
            Assert.IsNotNull(@namespace, "Can't update a slot! namespace parameter is null!");
            Assert.IsNotNull(userId, "Can't update a slot! userId parameter is null!");
            Assert.IsNotNull(accessToken, "Can't update a slot! accessToken parameter is null!");
            Assert.IsNotNull(data, "Can't update a slot! data parameter is null!");
            Assert.IsNotNull(filename, "Can't update a slot! filename parameter is null!");
            Assert.IsNotNull(slotId, "Can't update a slot! slotId parameter is null!");

            string checkSum;

            using (MD5 md5 = MD5.Create())
            {
                byte[] computeHash = md5.ComputeHash(data);
                checkSum = BitConverter.ToString(computeHash).Replace("-", "");
            }

            FormDataContent formDataContent = new FormDataContent();
            formDataContent.Add(data, filename);

            var builder = HttpRequestBuilder
                .CreatePut(this.baseUrl + "/public/namespaces/{namespace}/users/{userId}/slots/{slotId}")
                .WithPathParam("namespace", @namespace)
                .WithPathParam("userId", userId)
                .WithPathParam("slotId", slotId)
                .WithQueryParam("checksum", checkSum)
                .Accepts(MediaType.ApplicationJson)
                .WithBearerAuth(accessToken)
                .WithContentType(formDataContent.GetMediaType())
                .WithBody(formDataContent);

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            var result = request.TryParseResponseJson<Slot>();
            callback.Try(result);
        }

        public IEnumerator UpdateSlotMetadata(string @namespace, string userId, string accessToken, string slotId,
            string[] tags, string label, string customMetadata, ResultCallback<Slot> callback)
        {
            Assert.IsNotNull(@namespace, "Can't update a slot! namespace parameter is null!");
            Assert.IsNotNull(userId, "Can't update a slot! userId parameter is null!");
            Assert.IsNotNull(accessToken, "Can't update a slot! accessToken parameter is null!");
            Assert.IsNotNull(slotId, "Can't update a slot! slotId parameter is null!");

            FormDataContent customAttribute = new FormDataContent();
            customAttribute.Add("customAttribute", customMetadata);

            var builder = HttpRequestBuilder
                .CreatePut(this.baseUrl + "/public/namespaces/{namespace}/users/{userId}/slots/{slotId}/metadata")
                .WithPathParam("namespace", @namespace)
                .WithPathParam("userId", userId)
                .WithPathParam("slotId", slotId)
                .WithQueryParam("tags", tags)
                .WithQueryParam("label", label)
                .Accepts(MediaType.ApplicationJson)
                .WithBearerAuth(accessToken)
                .WithContentType(customAttribute.GetMediaType())
                .WithBody(customAttribute);

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            var result = request.TryParseResponseJson<Slot>();
            callback.Try(result);
        }

        public IEnumerator DeleteSlot(string @namespace, string userId, string accessToken, string slotId,
            ResultCallback callback)
        {
            Assert.IsNotNull(@namespace, "Can't create a slot! namespace parameter is null!");
            Assert.IsNotNull(userId, "Can't create a slot! userId parameter is null!");
            Assert.IsNotNull(accessToken, "Can't create a slot! accessToken parameter is null!");
            Assert.IsNotNull(slotId, "Can't create a slot! fileSection parameter is null!");

            var builder = HttpRequestBuilder
                .CreateDelete(this.baseUrl + "/public/namespaces/{namespace}/users/{userId}/slots/{slotId}")
                .WithPathParam("namespace", @namespace)
                .WithPathParam("userId", userId)
                .WithPathParam("slotId", slotId)
                .Accepts(MediaType.ApplicationJson)
                .WithBearerAuth(accessToken);

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            var result = request.TryParseResponse();
            callback.Try(result);
        }
    }
}