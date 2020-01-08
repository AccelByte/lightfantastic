using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using Game.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Game
{
    /// <summary>
    ///     The networked compoenent of the player pawn, 
    ///     handles all communications over the network and updateing the entity's state
    /// </summary>
    public class BasePlayerPawn : MovePlayerPawnBehavior
    {
        private Rigidbody2D rb2d;
        private bool isNetworkReady;
        private bool isInitialized;
        private bool isPlayerNumAssigned;
        private BaseHoveringText hoveringText;
        [SerializeField]
        private float speedDecayConst = 0.1f;
        [SerializeField]
        private float speedIncreaseConst = 0.1f;
        [SerializeField]
        private float maxSpeed_ = 0.1f;
        private float currSpeed;
        public float MaxSpeed { get { return maxSpeed_; } }

        private void Awake()
        {
            rb2d = GetComponent<Rigidbody2D>();
            hoveringText = GetComponent<BaseHoveringText>();
            currSpeed = 0.0f;
        }

        protected override void NetworkStart()
        {
            base.NetworkStart();
            isNetworkReady = true;
            StartCoroutine("PlayerInitialization");
        }

        private bool Initialize()
        {
            Debug.Log("Start Initialization");
            networkObject.OwnerNetId = networkObject.MyPlayerId;
            networkObject.MaxSpeed = maxSpeed_;
            isInitialized = true;
            return isInitialized;
        }

        void Update()
        {
            if (!isNetworkReady || !isInitialized || networkObject.Banned)
            {
                return;
            }
            if (!networkObject.IsOwner)
            {
                transform.position = networkObject.Position;
                return;
            }
            ReceiveInput(Time.deltaTime);
        }

        public override void StartAssignPlayerNum(RpcArgs args)
        {
            MainThreadManager.Run(() =>
            {
                AssignPlayerNum(args.GetAt<uint>(0));
            });
        }

        public override void Ban(RpcArgs args)
        {
            MainThreadManager.Run(() =>
            {
                Debug.Log("Player Got BanHammered");
                networkObject.Banned = true;
            });
        }

        public override void UpdatePosition(RpcArgs args)
        {
            //Leave this empty since this is only used to check for cheating
        }

        public void ReceiveInput(float dt)
        {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
            {
                currSpeed += speedIncreaseConst;
            }
            else
            {
                currSpeed = LinearDecay(currSpeed, dt);
            }
            Vector3 newPos = transform.position;
            newPos.x = transform.position.x + currSpeed;
            if (networkObject.IsOwner)
            {
                networkObject.SendRpc(RPC_UPDATE_POSITION, Receivers.Others, newPos);
                transform.position = networkObject.Position = newPos;
            }
        }

        private void AssignPlayerNum(uint newPlayerNum)
        {
            if (networkObject.IsOwner)
            {
                networkObject.playerNum = newPlayerNum;
            }
            hoveringText.ChangeTextLabel("Player " + newPlayerNum);
            isPlayerNumAssigned = true;
        }

        public float LinearDecay(float inVal, float dt)
        {
            inVal -= speedDecayConst * dt;
            if (inVal < 0)
            {
                inVal = 0;
            }
            return inVal;
        }

        IEnumerator PlayerInitialization()
        {
            while (!isInitialized || !isPlayerNumAssigned)
            {
                if (!isInitialized)
                {
                    Initialize();
                }
                if (!networkObject.IsOwner && networkObject.playerNum != 0 && !isPlayerNumAssigned)
                {
                    AssignPlayerNum(networkObject.playerNum);
                }
                yield return null;
            }
            networkObject.Banable = true;
            Debug.Log("Player Initialization Complete");
        }
    }
}
