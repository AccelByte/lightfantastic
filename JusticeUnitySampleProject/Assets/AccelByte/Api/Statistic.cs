// Copyright (c) 2019 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System.Collections.Generic;
using AccelByte.Core;
using AccelByte.Models;
using UnityEngine.Assertions;

namespace AccelByte.Api
{
    public class Statistic
    {
        private readonly StatisticApi api;
        private readonly ISession session;
        private readonly string @namespace;
        private readonly CoroutineRunner coroutineRunner;

        internal Statistic(StatisticApi api, ISession session, string @namespace, CoroutineRunner coroutineRunner)
        {
            Assert.IsNotNull(api, "Can not construct Statistic manager; api is null!");
            Assert.IsNotNull(session, "Can not construct Statistic manager; session parameter can not be null");
            Assert.IsFalse(string.IsNullOrEmpty(@namespace), "Can not construct Statistic manager; ns paramater couldn't be empty");
            Assert.IsNotNull(coroutineRunner, "Can not construct Statistic manager; coroutineRunner is null!");

            this.api = api;
            this.session = session;
            this.@namespace = @namespace;
            this.coroutineRunner = coroutineRunner;
        }

        /// <summary>
        /// Get All StatItems of specified profile
        /// </summary>
        /// <param name="profileId">User's profileId that about to get</param>
        /// <param name="callback">Returns all profile's StatItems via callback when completed</param>
        public void GetAllStatItems(string profileId, ResultCallback<StatItemPagingSlicedResult> callback)
        {
            if (!this.session.IsAuthenticated)
            {
                callback.TryError(ErrorCode.IsNotLoggedIn);

                return;
            }

            this.coroutineRunner.Run(
                this.api.GetAllStatItems(this.@namespace, this.session.UserId, profileId, this.session.SessionId, callback));
        }

        /// <summary>
        /// Get All StatItems of specified profile search by statCode(s)
        /// </summary>
        /// <param name="profileId">ProfileId that about to get</param>
        /// <param name="statCodes">One or more statCode(s) that about to get</param>
        /// <param name="callback">Returns an array of statItemInfo via callback when completed</param>
        public void GetStatItemsByStatCodes(string profileId, ICollection<string> statCodes, ResultCallback<StatItemInfo[]> callback)
        {
            if (!this.session.IsAuthenticated)
            {
                callback.TryError(ErrorCode.IsNotLoggedIn);

                return;
            }

            this.coroutineRunner.Run(
                this.api.GetStatItemsByStatCodes(this.@namespace, this.session.UserId, profileId, this.session.SessionId, statCodes, callback));
        }

    }
}