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
        private ParallaxSetter parallaxSetter;
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
        private bool isGameStarted = false;

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
            parallaxSetter = gameMgr.parallaxSetter;
        }

        private void PrepareTouchButton()
        {
            #if UNITY_ANDROID || UNITY_SWITCH
            var mainHud = gameMgr.HudManager.GetComponentsInChildren<MainHUD>()[0];
            mainHud.leftRunButton_.onClick.RemoveAllListeners();
            mainHud.rightRunButton_.onClick.RemoveAllListeners();
            mainHud.leftRunButton_.onClick.AddListener(()=>IncreaseCurrSpeed());
            mainHud.rightRunButton_.onClick.AddListener(()=>IncreaseCurrSpeed());
            #endif
        }

        protected override void NetworkStart()
        {
            base.NetworkStart();
            networkObject.onDestroy += OnNetworkObjectDestroy;
            networkObject.playerNumChanged += OnPlayerNumChanged;
            networkObject.OwnerNetIdChanged += OnOwnerNetIdChanged;
            NetworkManager.Instance.Networker.disconnected += OnDisconnected;
            gameMgr.onGameStart += OnGameStart;
            Initialize();
            isNetworkReady = true;
        }

        private void Initialize()
        {
            networkObject.MaxSpeed = maxSpeed_;
            hoveringText.ChangeTextLabel("Player " + networkObject.playerNum);
            if (networkObject.IsOwner)
            {
                PrepareTouchButton();
            }
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
            Vector3 newPos = transform.position;

            // restrain the player from moving
            if (isGameStarted)
            {
                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
                {
                    IncreaseCurrSpeed();
                }
                else
                {
                    currSpeed = LinearDecay(currSpeed, dt);
                }

                newPos.x = transform.position.x + currSpeed;
            }

            if (networkObject.IsOwner)
            {
                speedSetter.SetSpeed(currSpeed);
                networkObject.SendRpc(RPC_UPDATE_POSITION, Receivers.Others, newPos);
                networkObject.SendRpc(RPC_SET_CURRENT_SPEED, Receivers.Others, currSpeed);
                transform.position = networkObject.Position = newPos;
                parallaxSetter.SetSpeed(currSpeed);
            }
        }

        private void IncreaseCurrSpeed()
        {
            currSpeed += speedIncreaseConst;
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

        public override void RPCSetCurrentSpeed(RpcArgs args)
        {
            if (networkObject != null && !networkObject.IsOwner && speedSetter != null)
            {
                var currentSpeed = args.GetAt<float>(0);
                speedSetter.SetSpeed(currentSpeed);
            }
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
                    networkObject.Banable = false;
                });
            }
            else
            {
                MainThreadManager.Run(() =>
                {
                    networkObject.SetInitialPos(initialPos);
                    networkObject.Banable = false;
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

            Debug.Log("BasePlayerPawn OnNetworkObjectDestroy unregister gamemanager gamestart");
            gameMgr.onGameStart -= OnGameStart;
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
                    Debug.Log("BasePlayerPawn OnOwnerNetIdChanged");
                    gameMgr.RegisterCharacter(newOwnerNetId, this);
                }
            });
        }

        private void OnGameStart()
        {
            Debug.Log("BasePlayerPawn OnGameStart unlock player input");
            isGameStarted = true;
        }
        #endregion //Events
    }
}
