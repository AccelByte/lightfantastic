﻿// Copyright (c) 2018 - 2019 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections;
using System.Net;
using AccelByte.Models;
using AccelByte.Core;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using Utf8Json;

namespace AccelByte.Api
{
    internal class TelemetryApi
    {
        private readonly string baseUrl;
        private readonly UnityHttpWorker httpWorker;
        private readonly uint agentType;
        private readonly string deviceId;

        public TelemetryApi(string baseUrl, UnityHttpWorker httpWorker)
        {
            Assert.IsNotNull(baseUrl, "Creating "+ GetType().Name + " failed. Parameter baseUrl is null");
            Assert.IsNotNull(httpWorker, "Creating "+ GetType().Name + " failed. Parameter httpWorker is null");

            this.baseUrl = baseUrl;
            this.httpWorker = httpWorker;

            switch (Application.platform)
            {
            case RuntimePlatform.WindowsPlayer:
                this.agentType = 70;

                break;

            case RuntimePlatform.OSXPlayer:
                this.agentType = 80;

                break;

            case RuntimePlatform.LinuxPlayer:
                this.agentType = 90;

                break;

            case RuntimePlatform.Android:
                this.agentType = 110;

                break;

            case RuntimePlatform.IPhonePlayer:
                this.agentType = 120;

                break;

            case RuntimePlatform.XboxOne:
                this.agentType = 130;

                break;

            case RuntimePlatform.PS4:
                this.agentType = 140;

                break;

            case RuntimePlatform.Switch:
                this.agentType = 170;

                break;

            case RuntimePlatform.tvOS:
                this.agentType = 200;

                break;

            case RuntimePlatform.WSAPlayerX86:
                this.agentType = 210;

                break;

            case RuntimePlatform.WSAPlayerX64:
                this.agentType = 211;

                break;

            case RuntimePlatform.WSAPlayerARM:
                this.agentType = 212;

                break;

            case RuntimePlatform.WebGLPlayer:
                this.agentType = 220;

                break;
            }

            this.deviceId = DeviceProvider.GetFromSystemInfo().DeviceId;
        }

        public IEnumerator SendEvent<T>(string @namespace, string clientId, string userID, TelemetryEventTag eventTag,
            T eventData, ResultCallback callback) where T : class
        {
            Assert.IsTrue(
                typeof(T).IsSerializable,
                "Event data of type " + typeof(T) + " is not serializable. Try add [Serializable] attribute.");

            string nowTime = DateTime.UtcNow.ToString("O");
            string strEventData;

            if (eventData is string)
            {
                strEventData = "\"" + eventData + "\"";
            }
            else
            {
                strEventData = JsonSerializer.ToJsonString(eventData);
            }

            string jsonString = string.Format(
                "{{" +
                "\"AgentType\": {0}," +
                "\"AppID\": {1}," +
                "\"ClientID\": \"{2}\"," +
                "\"Data\": {3}," +
                "\"DeviceID\": \"{4}\"," +
                "\"EventID\": {5}," +
                "\"EventLevel\": {6}," +
                "\"EventTime\": \"{7}\"," +
                "\"EventType\": {8}," +
                "\"UUID\": \"{9:N}\"," +
                "\"UX\": {10}," +
                "\"UserID\": \"{11}\"" +
                "}}",
                this.agentType,
                eventTag.AppId,
                clientId,
                strEventData,
                this.deviceId,
                eventTag.Id,
                eventTag.Level,
                nowTime,
                eventTag.Type,
                Guid.NewGuid(),
                eventTag.UX,
                userID);

            var builder = HttpRequestBuilder
                .CreatePost(
                    this.baseUrl +
                    "/public/namespaces/{namespace}/events/gameclient/{appID}/{eventType}/{eventLevel}/{eventID}")
                .WithPathParam("namespace", @namespace)
                .WithPathParam("appID", eventTag.AppId.ToString())
                .WithPathParam("eventType", eventTag.Type.ToString())
                .WithPathParam("eventLevel", eventTag.Level.ToString())
                .WithPathParam("eventID", eventTag.Id.ToString())
                .WithContentType(MediaType.ApplicationJson)
                .WithBody(jsonString);

            UnityWebRequest request = null;

            yield return this.httpWorker.SendWithRetry(builder, req => request = req);

            var result = request.TryParseResponse();
            callback.Try(result);
        }
    }
}