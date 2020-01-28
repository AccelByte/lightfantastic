﻿using UnityEngine;
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
    [RequireComponent(typeof(BoxCollider2D), typeof(Animator))]
    [RequireComponent(typeof(CharacterHatSetter))]
    [RequireComponent(typeof(CharacterParticleSetter))]
    [RequireComponent(typeof(CharacterSpeedSetter))]
    public class BasePlayerPawn : MovePlayerPawnBehavior
    {
        #region Field and Properties
        private Rigidbody2D rb2d;
        private Collider2D col2d;
        private bool isNetworkReady;
        private bool isInitialized;
        private BaseHoveringText hoveringText;
        private CharacterSpeedSetter speedSetter;
        private CharacterHatSetter hatSetter;
        private CharacterParticleSetter particleSetter;
        [SerializeField]
        private float speedDecayConst = 0.1f;
        [SerializeField]
        private float speedIncreaseConst = 0.1f;
        [SerializeField]
        private float maxSpeed_ = 0.1f;
        private float currSpeed;
        public float MaxSpeed { get { return maxSpeed_; } }
        #region Cloud Data
        private string userId_;
        public string UserId { get { return userId_; } }
        private string hatTitle_;
        public string HatTitle { get { return hatTitle_; } }
        private string effectTitle_;
        public string EffectTitle { get { return effectTitle_; } }
        #endregion
        #endregion //Field and Properties

        private BaseGameManager gameMgr = null;

        private void Awake()
        {
            rb2d = GetComponent<Rigidbody2D>();
            col2d = GetComponent<Collider2D>();
            hoveringText = GetComponent<BaseHoveringText>();
            currSpeed = 0.0f;
            networkStarted += OnNetworkStarted;
            gameMgr = GameObject.FindGameObjectWithTag("GameManager").GetComponent<BaseGameManager>();
            speedSetter = GetComponent<CharacterSpeedSetter>();
            hatSetter = GetComponent<CharacterHatSetter>();
            particleSetter = GetComponent<CharacterParticleSetter>();
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
            GetCloudData();
        }

        private void GetCloudData()
        {
            if (!networkObject.IsOwner)
            {
                isInitialized = true;
                return;
            }
            ABRuntimeLogic.AccelByteAuthenticationLogic abAuth = AccelByteManager.Instance.AuthLogic;
            AccelByte.Models.UserData ud = abAuth.GetUserData();
            userId_ = ud.userId;
            networkObject.SendRpc(RPC_SET_USER_ID, Receivers.Others, userId_);
            AccelByteEntitlementLogic abEntitlement = AccelByteManager.Instance.EntitlementLogic;
            abEntitlement.OnGetEntitlementCompleted += OnGetSelfEntitlementCompleted;
            abEntitlement.GetEntitlement(false);
        }

        private void OnGetSelfEntitlementCompleted(bool inMenu, AccelByte.Core.Error error)
        {
            if (inMenu)
            {
                return;
            }
            if (error != null)
            {
                Debug.LogError("[" + error.Code + "] " + error.Message);
                isInitialized = true;
                return;
            }
            AccelByteEntitlementLogic abEntitlement = AccelByteManager.Instance.EntitlementLogic;
            abEntitlement.OnGetEntitlementCompleted -= OnGetSelfEntitlementCompleted;
            Equipments.EquipmentList activeEquipments = abEntitlement.GetActiveEquipmentList();
            if (activeEquipments != null)
            {
                hatTitle_ = activeEquipments.hat != null ? activeEquipments.hat.title : "NULL";
                effectTitle_ = activeEquipments.effect != null ? activeEquipments.effect.title : "NULL";
                hatSetter.SetHatSprite(hatTitle_);
                networkObject.SendRpc(RPC_SET_ACTIVE_EQUIPMENT, Receivers.Others, new object[] { hatTitle_, effectTitle_ });
            }
            else
            {
                Debug.LogError("No Active Equipment");
            }
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
            speedSetter.SetSpeed(currSpeed * LightFantasticConfig.CURR_SPEED_MULTIPLIER_ANIMATION);
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

        public override void RPCSetUserId(RpcArgs args)
        {
            if (networkObject.IsOwner) { return; }
            string newUserId = args.GetAt<string>(0);
            userId_ = newUserId;
        }

        public override void RPCSetActiveEquipment(RpcArgs args)
        {
            if (networkObject.IsOwner) { return; }
            string newHatTitle = args.GetAt<string>(0);
            string newEffectTitle = args.GetAt<string>(1);
            hatTitle_ = newHatTitle;
            effectTitle_ = newEffectTitle;
            hatSetter.SetHatSprite(hatTitle_); 
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
