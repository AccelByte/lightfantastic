// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
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
            Assert.IsFalse(
                string.IsNullOrEmpty(@namespace),
                "Can not construct Statistic manager; ns paramater couldn't be empty");

            Assert.IsNotNull(coroutineRunner, "Can not construct Statistic manager; coroutineRunner is null!");

            this.api = api;
            this.session = session;
            this.@namespace = @namespace;
            this.coroutineRunner = coroutineRunner;
        }

        /// <summary>
        /// Create stat items of a user. Before a user can have any data in a stat item, he/she needs to have that stat item created.
        /// </summary>
        /// <param name="statItems">List of statCodes to be created for a user</param>
        /// <param name="callback">Returns all profile's StatItems via callback when completed</param>
        public void CreateUserStatItems(CreateStatItemRequest[] statItems,
            ResultCallback<StatItemOperationResult[]> callback)
        {
            Report.GetFunctionLog(this.GetType().Name);

            if (!this.session.IsValid())
            {
                callback.TryError(ErrorCode.IsNotLoggedIn);

                return;
            }

            this.coroutineRunner.Run(
                this.api.CreateUserStatItems(
                    this.@namespace,
                    this.session.UserId,
                    this.session.AuthorizationToken,
                    statItems,
                    callback));
        }

        /// <summary>
        /// Get all stat items of a user.
        /// </summary>
        /// <param name="callback">Returns all profile's StatItems via callback when completed</param>
        public void GetAllUserStatItems(ResultCallback<PagedStatItems> callback)
        {
            Report.GetFunctionLog(this.GetType().Name);

            if (!this.session.IsValid())
            {
                callback.TryError(ErrorCode.IsNotLoggedIn);

                return;
            }

            this.coroutineRunner.Run(
                this.api.GetUserStatItems(
                    this.@namespace,
                    this.session.UserId,
                    this.session.AuthorizationToken,
                    null,
                    null,
                    callback));
        }

        /// <summary>
        /// Get stat items of a user, filter by statCodes and tags
        /// </summary>
        /// <param name="statCodes">List of statCodes that will be included in the result</param>
        /// <param name="tags">List of tags that will be included in the result</param>
        /// <param name="callback">Returns all profile's StatItems via callback when completed</param>
        public void GetUserStatItems(ICollection<string> statCodes, ICollection<string> tags,
            ResultCallback<PagedStatItems> callback)
        {
            Report.GetFunctionLog(GetType().Name);

            if (!this.session.IsValid())
            {
                callback.TryError(ErrorCode.IsNotLoggedIn);

                return;
            }

            this.coroutineRunner.Run(
                this.api.GetUserStatItems(
                    this.@namespace,
                    this.session.UserId,
                    this.session.AuthorizationToken,
                    statCodes,
                    tags,
                    callback));
        }

        /// <summary>
        /// Update stat items for a users
        /// </summary>
        /// <param name="increments">Consist of one or more statCode with its increament value.
        ///     Positive increament value means it will increase the previous statCode value.
        ///     Negative increament value means it will decrease the previous statCode value. </param>
        /// <param name="callback">Returns an array of BulkStatItemOperationResult via callback when completed</param>
        public void IncrementUserStatItems(StatItemIncrement[] increments,
            ResultCallback<StatItemOperationResult[]> callback)
        {
            Report.GetFunctionLog(GetType().Name);

            if (!this.session.IsValid())
            {
                callback.TryError(ErrorCode.IsNotLoggedIn);

                return;
            }

            this.coroutineRunner.Run(
                this.api.IncrementUserStatItems(
                    this.@namespace,
                    this.session.UserId,
                    increments,
                    this.session.AuthorizationToken,
                    callback));
        }
    }
}