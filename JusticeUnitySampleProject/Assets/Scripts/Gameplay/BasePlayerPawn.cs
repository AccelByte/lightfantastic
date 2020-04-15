using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using Game.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AccelByte.Core;

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
        private CharacterPlatformSpriteSetter platformSetter;
        private ParallaxSetter parallaxSetter;
        private float speedDecayConst = LightFantasticConfig.PLAYER_SPEED_DECAY;
        private float speedIncreaseConst = LightFantasticConfig.PLAYER_SPEED_INCREASE;
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
        private InGameHudManager hudMgr = null;
        private bool allowInput = false;

        private string playerName = "Player One";

        private void Awake()
        {
            rb2d = GetComponent<Rigidbody2D>();
            col2d = GetComponent<Collider2D>();
            hoveringText = GetComponent<BaseHoveringText>();
            currSpeed = 0.0f;
            networkStarted += OnNetworkStarted;
            gameMgr = GameObject.FindGameObjectWithTag("GameManager").GetComponent<BaseGameManager>();
            hudMgr = GameObject.FindGameObjectWithTag("HUDManager").GetComponent<InGameHudManager>();
            speedSetter = GetComponent<CharacterSpeedSetter>();
            hatSetter = GetComponent<CharacterHatSetter>();
            particleSetter = GetComponent<CharacterParticleSetter>();
            platformSetter = GetComponent<CharacterPlatformSpriteSetter>();
            parallaxSetter = gameMgr.parallaxSetter;

            AddListeners();
        }

        private void AddListeners()
        {
            if (gameMgr != null)
            {
                gameMgr.onGameEnd += OnEndGame;
            }
        }

        private void OnEndGame()
        {
            Debug.Log("BasePlayerPawn OnEndGame");

            allowInput = false;
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

        /// <summary>
        /// Get Player's displayname and entitlement data from Accelbyte's Service
        /// </summary>
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
            
            // Set mine and other player's displayName and Platform
            if (!networkObject.IsServer)
            {
                networkObject.SendRpc(RPC_SET_DISPLAY_NAME_AND_PLATFORM, Receivers.AllBuffered, new object[]
                {
                    ud.displayName, (uint) LightFantasticConfig.GetPlatform()
                });
            }
            
            AccelByteEntitlementLogic abEntitlement = AccelByteManager.Instance.EntitlementLogic;
            abEntitlement.OnGetEntitlementCompleted += OnGetSelfEntitlementCompleted;
            abEntitlement.GetEntitlement(false);
        }

        /// <summary>
        /// Callback on OnGetEntitlementCompleted event
        /// On success, setup the character's equipments based on the entitlement data
        /// </summary>
        /// <param name="inMenu"> determine is this ingameplay scene or in main menu </param>
        /// <param name="error"> accelbyte's error parameter </param>
        private void OnGetSelfEntitlementCompleted(bool inMenu, AccelByte.Core.Error error)
        {
            if (inMenu)
            {
                return;
            }
            if (error != null)
            {
                Debug.Log("[" + error.Code + "] " + error.Message);
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
                particleSetter.SetItem(effectTitle_);
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
                UpdateMinimap();
                return;
            }
            ReceiveInput(Time.deltaTime);
            UpdateMinimap();
        }

        public void ReceiveInput(float dt)
        {
            Vector3 newPos = transform.position;

            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D))
            {
                IncreaseCurrSpeed();
            }
            currSpeed = LinearDecay(currSpeed, dt);
            newPos.x = transform.position.x + currSpeed;

            if (networkObject.IsOwner)
            {
                speedSetter.SetSpeed(currSpeed);
                networkObject.SendRpc(RPC_UPDATE_POSITION, Receivers.Others, newPos);
                networkObject.SendRpc(RPC_SET_CURRENT_SPEED, Receivers.Others, currSpeed);
                transform.position = networkObject.Position = newPos;
                parallaxSetter.SetSpeed(currSpeed);
            }
        }

        private void UpdateMinimap()
        {
            if (hudMgr != null)
            {
                MainHUD hud = (MainHUD)hudMgr.GetPanel(PanelTypes.MainHud);
                if (hud != null)
                {
                    hud.UpdateMinimap(playerName, (uint)transform.position.x);
                }
                else
                {
                    Debug.LogError("BasePlayerPawn Update MainHUD is null");
                }
            }
            else
            {
                Debug.LogError("BasePlayerPawn Update hudMgr is null");
            }
        }

        private void IncreaseCurrSpeed()
        {
            // restrain the player from moving
            if (allowInput)
            {
                currSpeed += speedIncreaseConst;

                // play sfx for each button call
                AudioManager.Instance.PlaySoundFX(E_SoundFX.ButtonClick02);
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

        public void FreezePlayerOnFinish()
        {
            IEnumerator IncreaseSpeedDecayOnFinish()
            {
                while (currSpeed > 0)
                {
                    speedDecayConst *= LightFantasticConfig.PLAYER_SPEED_DECAY_MULTIPLIER_ONFINISH;
                    yield return null;
                }
            }
            
            // Block player input & decelerate player
            allowInput = false;
            StartCoroutine(IncreaseSpeedDecayOnFinish());
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
                    gameMgr.RegisterCharacter(newOwnerNetId, this);
                    networkObject.Banable = false;
                    networkObject.SendRpc(RPC_SET_READY, Receivers.Server);
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
            particleSetter.SetItem(effectTitle_); 
        }

        /// <summary>
        /// Applied for both owner and other
        /// </summary>
        /// <param name="args"></param>
        public override void RPCSetDisplayNameAndPlatform(RpcArgs args)
        {
            hoveringText.ChangeTextLabel(args.GetAt<string>(0));
            platformSetter.SetSprite((LightFantasticConfig.Platform) args.GetAt<uint>(1));

            //TODO: init minimap here!
            if (hudMgr != null)
            {
                playerName = args.GetAt<string>(0);
                ((MainHUD)hudMgr.GetPanel(PanelTypes.MainHud)).SetupMinimap(playerName);
            }
        }

        
        public override void RPCSetReady(RpcArgs args)
        {
            if (networkObject.IsServer)
            {
                gameMgr.PlayerSetReady();
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
            allowInput = true;
        }
        #endregion //Events
    }
}
