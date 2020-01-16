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
        private BaseHoveringText hoveringText;
        [SerializeField]
        private float speedDecayConst = 0.1f;
        [SerializeField]
        private float speedIncreaseConst = 0.1f;
        [SerializeField]
        private float maxSpeed_ = 0.1f;
        private float currSpeed;
        public float MaxSpeed { get { return maxSpeed_; } }

        private BaseGameManager gameMgr = null;

        private void Awake()
        {
            rb2d = GetComponent<Rigidbody2D>();
            hoveringText = GetComponent<BaseHoveringText>();
            currSpeed = 0.0f;
        }

        private void Start()
        {
            gameMgr = GameObject.FindGameObjectWithTag("GameManager").GetComponent<BaseGameManager>();
        }

        protected override void NetworkStart()
        {
            base.NetworkStart();

            if (networkObject.IsOwner)
            {
                networkObject.onDestroy += OnNetworkObjectDestroy;
            }
            networkObject.playerNumChanged += OnPlayerNumChanged;
            networkObject.OwnerNetIdChanged += OnOwnerNetIdChanged;

            Initialize();
            isNetworkReady = true;
        }

        private void Initialize()
        {
            networkObject.MaxSpeed = maxSpeed_;
            hoveringText.ChangeTextLabel("Player " + networkObject.playerNum);
            networkObject.Banable = true;
            isInitialized = true;
            Debug.Log("Player Initialized");
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

        public float LinearDecay(float inVal, float dt)
        {
            inVal -= speedDecayConst * dt;
            if (inVal < 0)
            {
                inVal = 0;
            }
            return inVal;
        }

        #region RPCs
        public override void UpdatePosition(RpcArgs args)
        {
            //Leave this empty since this is only used to check for cheating
        }

        public override void Ban(RpcArgs args)
        {
            MainThreadManager.Run(() =>
            {
                Debug.Log("Player Got BanHammered");
                networkObject.Banned = true;
            });
        }

        public override void AssignPlayerNum(RpcArgs args)
        {
            if (networkObject.IsOwner)
            {
                uint newPlayerNum = args.GetAt<uint>(0);
                Debug.Log("AssignPlayerNum: " + newPlayerNum);
                networkObject.playerNum = newPlayerNum;
                MainThreadManager.Run(() => { hoveringText.ChangeTextLabel("Player " + newPlayerNum); });
            }
        }

        public override void AssignOwnerId(RpcArgs args)
        {

            if (networkObject.IsOwner)
            {
                uint newOwnerNetId = args.GetAt<uint>(0);
                Debug.Log("AssignOwnerId: " + newOwnerNetId);
                networkObject.OwnerNetId = newOwnerNetId;
                MainThreadManager.Run(() => { gameMgr.RegisterCharacter(newOwnerNetId, this); });
            }
        }
        #endregion //RPCs

        #region Events
        private void OnNetworkObjectDestroy(NetWorker sender)
        {
            Debug.Log("Player Network object is being destroyed");
            //gameMgr.RemoveFromCharacterList(networkObject.OwnerNetId);
        }

        private void OnPlayerNumChanged(uint newPlayerNum, ulong timestep)
        {
            MainThreadManager.Run(() =>
            {
                Debug.Log("OnPlayerNumChanged -> " + newPlayerNum);
                hoveringText.ChangeTextLabel("Player " + newPlayerNum);
            });
        }
        private void OnOwnerNetIdChanged(uint newOwnerNetId, ulong timestep)
        {
            MainThreadManager.Run(() =>
            {
                Debug.Log("OnOwnerNetIdChanged -> " + newOwnerNetId);
                if (networkObject.IsOwner)
                {
                    gameMgr.RegisterCharacter(newOwnerNetId, this);
                }
            });
        }
        #endregion //Events

    }
}
