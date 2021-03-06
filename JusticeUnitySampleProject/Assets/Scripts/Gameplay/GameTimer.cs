// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using Game.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameTimer : GameTimerBehavior
{
    public delegate void TimerUpdated(int newValue);
    public event TimerUpdated timerUpdated;

    public delegate void TimerExpired();
    public event TimerExpired onTimerExpired;

    private int length_ = (int) LightFantasticConfig.RACE_LENGTH_SECOND;
    [SerializeField]
    private bool isCountdown_;
    public bool IsCountdown { get { return isCountdown_; } }
    private int currentSec_;

    private bool expired_;
    public bool IsExpired { get { return expired_; } }

    private Game.InGameHudManager inGameHudMgr_;
    private MainHUD mainHud_;

    protected override void NetworkStart()
    {
        base.NetworkStart();
        Initialize();
    }

    private void Initialize()
    {
        Debug.Log("Game Timer is running");
        inGameHudMgr_ = GameObject.FindGameObjectWithTag("HUDManager").GetComponent<Game.InGameHudManager>();
        mainHud_ = inGameHudMgr_.FindPanel<MainHUD>();
        mainHud_.AttachTimer(this);
        expired_ = false;
        networkObject.secChanged += OnSecChanged;
        if (networkObject.IsServer && networkObject.IsOwner)
        {
            if (isCountdown_)
            {
                currentSec_ = length_;
            }
            else
            {
                currentSec_ = 0;
            }
            networkObject.sec = currentSec_;
            timerUpdated?.Invoke(currentSec_);
            //MainThreadManager.Run(() => { StartCoroutine(TimerRoutine()); });
        }
    }

    private void StartTimer()
    {
        if (networkObject.IsServer && networkObject.IsOwner)
        {
            MainThreadManager.Run(() => { StartCoroutine(TimerRoutine()); });
        }
    }

    private void Tick()
    {
        if (!expired_)
        {
            currentSec_ = isCountdown_ ? currentSec_ - 1 : currentSec_ + 1;
            networkObject.sec = currentSec_;
            timerUpdated?.Invoke(currentSec_);
            if (isCountdown_ && currentSec_ == 0)
            {
                expired_ = true;
                onTimerExpired?.Invoke();
            }
            else if (!isCountdown_ && currentSec_ == length_)
            {
                expired_ = true;
                onTimerExpired?.Invoke();
            }
        }
    }

    #region Events
    private void OnSecChanged(int newValue, ulong timestep)
    {
        timerUpdated?.Invoke(newValue);
    }
    #endregion

    IEnumerator TimerRoutine()
    {
        while (true)
        {
            Tick();
            yield return new WaitForSecondsRealtime(1.0f);
        }
    }

    public void StartGameTimer()
    {
        Debug.Log("GameTimer StartCountDown start timer");
        StartTimer();
    }
}
