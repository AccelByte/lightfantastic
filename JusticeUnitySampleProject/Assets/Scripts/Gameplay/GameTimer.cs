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

    [SerializeField]
    private int length_;
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
}
