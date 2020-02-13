using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeardedManStudios.Forge.Networking.Generated;
using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;

public class CountDown : GameStartCountDownBehavior
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
        Debug.Log("Count down Timer is running");
        inGameHudMgr_ = GameObject.FindGameObjectWithTag("HUDManager").GetComponent<Game.InGameHudManager>();
        mainHud_ = inGameHudMgr_.FindPanel<MainHUD>();
        mainHud_.AttachCountDown(this);
        expired_ = false;
        networkObject.secondChanged += OnSecondChanged;
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
            networkObject.second = currentSec_;
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
            networkObject.second = currentSec_;
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

    public override void CountDownStart(RpcArgs args)
    {
        // Start count down
    }

    #region Events
    private void OnSecondChanged(int newValue, ulong timestep)
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

    public void StartCountDown()
    {
        Debug.Log("CountDown StartCountDown start timer");
        StartTimer();
    }
}
