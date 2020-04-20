// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

using System;
using System.Collections.Generic;
using BeardedManStudios.Forge.Networking.Unity;
using UnityEngine;
using UnityEngine.UI;

public enum MatchmakingWaitingPhase
{
    None = 0,
    FindMatch = 1, //Matchmaking Service
    ConfirmingMatch = 2, //Matchmaking Service
    WaitingDSM = 3 //DSM Service
}

[RequireComponent(typeof(Transform))]
[RequireComponent(typeof(Text))]
public class MatchmakingBoardScript : MonoBehaviour
{
    public Transform waitingTimerLayout;
    [SerializeField] private Text waitingPhaseText;
    [SerializeField] private Text timeLeftText;
    [SerializeField] private Text gameModeText;

#region CountdownStuff
    private MatchmakingWaitingPhase currentPhase;
    private bool isCountingDown;
    private float timeLeft;
    private Action onCountdownAction;
#endregion 

    public void SetGameMode(LightFantasticConfig.GAME_MODES mode)
    {
        gameModeText.text = LightFantasticConfig.GAME_MODES_VERBOSE[mode];
    }

    /// <summary>
    /// Countdown for finding match until timeout
    /// </summary>
    /// <param name="phase">If we re-call this function using same "phase", it won't override or refresh the timer</param>
    /// <param name="onTimeout">Action will be run on countdown running out</param>
    public void StartCountdown(MatchmakingWaitingPhase phase, Action onTimeout)
    {
        MainThreadTaskRunner.Instance.Run(delegate{
            if (currentPhase != phase)
            {
                currentPhase = phase;
                waitingPhaseText.text = WAITING_PHASE_VERBOSE[phase];
                timeLeft = WAITING_PHASE_TIMEOUTLIMIT[phase];
                isCountingDown = true;
                onCountdownAction = onTimeout;
            }
        });
    }
    
    /// <summary>
    /// Cleared the countdown and the queued action
    /// </summary>
    public void TerminateCountdown()
    {
        currentPhase = MatchmakingWaitingPhase.None;
        timeLeft = 0;
        isCountingDown = false;
        onCountdownAction = null;
    }

    private class TimeLeft
    {
        public int minute;
        public int second;
    }

    private TimeLeft SecondToTimeLeft(float second)
    {
        var secondRemain = 0;
        Math.DivRem((int) second, 60, out secondRemain);

        var minuteRemain = (int) second / 60;
        
        return new TimeLeft{minute = minuteRemain, second = secondRemain};
    }

    private void Update()
    {
        if (isCountingDown)
        {
            timeLeft -= Time.deltaTime;
            var timeLeftDisplay = SecondToTimeLeft(timeLeft);
            timeLeftText.text = $"{timeLeftDisplay.minute} : {timeLeftDisplay.second.ToString("00")}";
            if (timeLeft < 0)
            {
                MainThreadTaskRunner.Instance.Run(onCountdownAction);
                isCountingDown = false;
                timeLeft = LightFantasticConfig.MATCHMAKING_FINDMATCH_TIMEOUT;
                onCountdownAction = null;
            }
        }
    }
    
    private readonly static IDictionary<MatchmakingWaitingPhase, string> WAITING_PHASE_VERBOSE = new Dictionary<MatchmakingWaitingPhase, string>
    {
        {MatchmakingWaitingPhase.None, "..."},
        {MatchmakingWaitingPhase.FindMatch, "Finding Match..."},
        {MatchmakingWaitingPhase.ConfirmingMatch, "Confirming Match Ready..."},
        {MatchmakingWaitingPhase.WaitingDSM, "Entering Level..."}
    };
    
    private readonly static IDictionary<MatchmakingWaitingPhase, int> WAITING_PHASE_TIMEOUTLIMIT = new Dictionary<MatchmakingWaitingPhase, int>
    {
        {MatchmakingWaitingPhase.None, 0},
        {MatchmakingWaitingPhase.FindMatch, LightFantasticConfig.MATCHMAKING_FINDMATCH_TIMEOUT},
        {MatchmakingWaitingPhase.ConfirmingMatch, LightFantasticConfig.MATCHMAKING_CONFIRMING_READY_TIMEOUT},
        {MatchmakingWaitingPhase.WaitingDSM, LightFantasticConfig.WAITING_DEDICATED_SERVER_TIMEOUT}
    };
}
