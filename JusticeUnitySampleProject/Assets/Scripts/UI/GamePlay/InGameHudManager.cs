// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Game
{
    public enum PanelTypes
    {
        RaceOver,
        Pause,
        MainHud
    }

    public class InGameHudManager : HUDManager<PanelTypes>
    {
        private BaseGameManager gameMgr;

        [SerializeField]
        private PanelTypes initialPanel = PanelTypes.MainHud;

        private bool isRaceOver;

        protected override void Awake()
        {
            base.Awake();
            mainCanvas_.sortingLayerName = "UI";
            mainCanvas_.renderMode = RenderMode.ScreenSpaceCamera;
            mainCanvas_.worldCamera = Camera.main;
            gameMgr = GameObject.FindGameObjectWithTag("GameManager").GetComponent<BaseGameManager>();
            gameMgr.HudManager = this;
        }

        protected override void Start()
        {
            base.Start();
            ShowInitialPanel();
        }

        private void ShowInitialPanel()
        {
            Debug.Log("Showing Initial Panel");
            ShowPanel(PanelTypes.MainHud, null);
        }

        public void ShowRaceOverScreen(bool isWinner)
        {
            ShowPanel(PanelTypes.RaceOver, new object[] { isWinner });
            isRaceOver = true;
        }

        public void HideRaceOverScreen()
        {
            HideTopPanel();
        }

        public void ShowPauseScreen()
        {
            ShowPanel(PanelTypes.Pause, null);
        }

        public void HidePauseScreen()
        {
            HideTopPanel();
        }

        public void DisconnectPlayer()
        {
            Debug.Log("Gameplay: DisconnectPlayer");
            gameMgr.DisconnectPlayer();
            // notify the lobby logic that the player's match is already over
            AccelByteManager.Instance.LobbyLogic.SetIsActionPhaseOver(true);
        }
    }
}
