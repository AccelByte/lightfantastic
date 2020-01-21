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
    [RequireComponent(typeof(BoxCollider2D))]
    public class BasePlayerPawn : MovePlayerPawnBehavior
    {
        private Rigidbody2D rb2d;
        private Collider2D col2d;
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
            col2d = GetComponent<Collider2D>();
            hoveringText = GetComponent<BaseHoveringText>();
            currSpeed = 0.0f;
            networkStarted += OnNetworkStarted;
            gameMgr = GameObject.FindGameObjectWithTag("GameManager").GetComponent<BaseGameManager>();
        }

        protected override void NetworkStart()
        {
            base.NetworkStart();
            networkObject.onDestroy += OnNetworkObjectDestroy;
            networkObject.playerNumChanged += OnPlayerNumChanged;
            networkObject.OwnerNetIdChanged += OnOwnerNetIdChanged;
            NetworkManager.Instance.Networker.disconnected += OnDisconnected;
            Initialize();
            isNetworkReady = true;
        }

        private void Initialize()
        {
            networkObject.MaxSpeed = maxSpeed_;
            hoveringText.ChangeTextLabel("Player " + networkObject.playerNum);
            isInitialized = true;
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

        /// <summary>
        /// Decay of the player speed
        /// </summary>
        /// <param name="playerNetworkId">The player network ID</param>
        /// <param name="pawn">Behaviour class of the pawn</param>
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

        public override void RPCSetup(RpcArgs args)
        {
            uint newPlayerNum = args.GetAt<uint>(0);
            uint newOwnerNetId = args.GetAt<uint>(1);
            Vector3 initialPos = args.GetAt<Vector3>(2);
            if (networkObject.IsOwner)
            {
                networkObject.playerNum = newPlayerNum;
                networkObject.OwnerNetId = newOwnerNetId;
                networkObject.SetInitialPos(initialPos);
                MainThreadManager.Run(() =>
                {
                    hoveringText.ChangeTextLabel("Player " + newPlayerNum);
                    gameMgr.RegisterCharacter(newOwnerNetId, this);
                    networkObject.Banable = true;
                });
            }
            else
            {
                MainThreadManager.Run(() =>
                {
                    networkObject.SetInitialPos(initialPos);
                    networkObject.Banable = true;
                });
            }
        }

        #endregion //RPCs

        #region Events

        private void OnDisconnected(NetWorker sender)
        {
            networkObject.Destroy();
        }

        private void OnNetworkObjectDestroy(NetWorker sender)
        {
            networkObject.onDestroy -= OnNetworkObjectDestroy;
            networkObject.playerNumChanged -= OnPlayerNumChanged;
            networkObject.OwnerNetIdChanged -= OnOwnerNetIdChanged;
        }

        private void OnNetworkStarted(NetworkBehavior behavior)
        {
            networkStarted -= OnNetworkStarted;
            if (!networkObject.IsOwner)
            {
                return;
            }
            networkObject.SendRpc(MovePlayerPawnBehavior.RPC_SETUP, Receivers.All
                , new object[] { networkObject.playerNum, networkObject.OwnerNetId, transform.position });
        }

        private void OnPlayerNumChanged(uint newPlayerNum, ulong timestep)
        {
            MainThreadManager.Run(() =>
            {
                hoveringText.ChangeTextLabel("Player " + newPlayerNum);
            });
        }

        private void OnOwnerNetIdChanged(uint newOwnerNetId, ulong timestep)
        {
            MainThreadManager.Run(() =>
            {
                if (networkObject.IsOwner || networkObject.IsServer)
                {
                    gameMgr.RegisterCharacter(newOwnerNetId, this);
                }
            });
        }
        #endregion //Events
    }
}
