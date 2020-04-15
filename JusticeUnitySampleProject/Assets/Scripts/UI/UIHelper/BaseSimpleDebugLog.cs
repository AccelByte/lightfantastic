// Copyright (c) 2019 - 2020 AccelByte Inc. All Rights Reserved.
// This is licensed software from AccelByte Inc, for limitations
// and restrictions contact your company contract manager.

#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseSimpleDebugLog : MonoBehaviour
{
    [SerializeField]
    private float clearLogDelay;
    private string logText;
    private Queue logQueue = new Queue();
    private bool isEnabled = false;

    private List<string> LogList;

    void Awake()
    {
        LogList = new List<string>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Debug Log Started");
        //StartCoroutine(ClearLogs());
    }

    void OnEnable()
    {
        Application.logMessageReceived += DebugLogHandler;
        isEnabled = true;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= DebugLogHandler;
        isEnabled = false;
    }

    //void DebugLogHandler(string logString, string stackTrace, LogType type)
    //{
    //    logText = logString;
    //    string logFormat = "\n [" + type + "] : " + logText;

    //    logQueue.Enqueue(logFormat);
    //    if (type == LogType.Exception)
    //    {
    //        logFormat = "\n" + stackTrace;
    //        logQueue.Enqueue(logFormat);
    //    }

    //    logText = string.Empty;
    //    foreach (string newLog in logQueue)
    //    {
    //        logText += newLog;
    //    }
    //}

    void DebugLogHandler(string logString, string stackTrace, LogType type)
    {
        logText = logString;
        string logFormat = "\n [" + type + "] : " + logText;

        if (LogList.Count >= 30)
        {
            LogList.RemoveAt(0);
        }

        LogList.Add(logFormat);
        if (type == LogType.Exception)
        {
            logFormat = "\n" + stackTrace;
            LogList.Add(logFormat);
        }

        logText = string.Empty;
        foreach (string newLog in LogList)
        {
            logText += newLog;
        }
    }

    void WriteInChatBox(string chat)
    {
        string textChat = "";

        if (LogList.Count >= 10)
        {
            LogList.RemoveAt(0);
        }
        LogList.Add(chat);

        foreach (var s in LogList)
        {
            textChat += s + "\n";
        }
        logText = textChat;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnGUI()
    {
        GUILayout.Label(logText);
    }

    IEnumerator ClearLogs()
    {
        while (isEnabled)
        {
            yield return new WaitForSecondsRealtime(clearLogDelay);
            logQueue.Clear();
            logText = string.Empty;
        }
    }
}
