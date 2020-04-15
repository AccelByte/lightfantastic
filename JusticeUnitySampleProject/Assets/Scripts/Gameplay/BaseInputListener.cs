// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Generated;

/// <summary>
///     Pools the client's input and send its to the server
/// </summary>
namespace Game
{
    public class BaseInputListener : InputListenerBehavior
    {
        public List<BaseInputFrame> framesToPlay;
        public List<BaseInputFrame> framesToValidate;

        private bool isNetworkReady;

        private uint frameNumber;
        private BaseInputFrame inputFrame = BaseInputFrame.Empty;

        // Start is called before the first frame update
        void Start()
        {
            framesToPlay = new List<BaseInputFrame>();
            framesToValidate = new List<BaseInputFrame>();
        }
        protected override void NetworkStart()
        {
            base.NetworkStart();
            isNetworkReady = true;
        }

        /// <summary>
        ///     Polling of inputs is handled in update instead of fixed update since they reset every frame
        /// </summary>
        void Update()
        {
            if (!isNetworkReady)
            {
                return;
            }

            if (!networkObject.IsServer && networkObject.IsOwner)
            {
                inputFrame = new BaseInputFrame
                {
                    right = Input.GetKeyDown(KeyCode.RightArrow),
                    left = Input.GetKeyDown(KeyCode.LeftArrow),
                    up = Input.GetKeyDown(KeyCode.UpArrow),
                    down = Input.GetKeyDown(KeyCode.DownArrow),
                    horizontal = Input.GetAxisRaw("Horizontal"),
                    vertical = Input.GetAxisRaw("Vertical")
                };
            }
        }

        /// <summary>
        ///     Store the input polled from update and sent it to the server for authoritative processing
        /// </summary>
        private void FixedUpdate()
        {
            if (!isNetworkReady)
            {
                return;
            }

            if (!networkObject.IsServer && networkObject.IsOwner)
            {
                inputFrame.frameNumber = frameNumber++;
                framesToPlay.Add(inputFrame);

                byte[] bytes = ByteArray.Serialize(inputFrame);
                networkObject.SendRpc(RPC_SYNC_INPUTS, Receivers.Server, bytes);
            }
        }

        public override void SyncInputs(RpcArgs args)
        {
            if (networkObject.IsServer)
            {
                var bytes = args.GetNext<Byte[]>();
                BaseInputFrame newest = (BaseInputFrame)ByteArray.Deserialize(bytes);
                framesToPlay.Add(newest);
            }
        }
    }
}
