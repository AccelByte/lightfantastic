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

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Debug Log Started");
        StartCoroutine(ClearLogs());
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

    void DebugLogHandler(string logString, string stackTrace, LogType type)
    {
        logText = logString;
        string logFormat = "\n [" + type + "] : " + logText;
        logQueue.Enqueue(logFormat);
        if (type == LogType.Exception)
        {
            logFormat = "\n" + stackTrace;
            logQueue.Enqueue(logFormat);
        }
        logText = string.Empty;
        foreach (string newLog in logQueue)
        {
            logText += newLog;
        }
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
            yield return new WaitForSeconds(clearLogDelay);
            logQueue.Clear();
            logText = string.Empty;
        }
    }
}
