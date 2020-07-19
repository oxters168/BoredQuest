using System.Collections;
using UnityEngine;

public class InfoLogger : MonoBehaviour
{
    string myLog;
    Queue myLogQueue = new Queue();

    public int frames = 10;
    private int framesPassed = 0;
    public TMPro.TextMeshProUGUI outputLabel;

    void OnEnable ()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable ()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        myLog = logString;
        string newString = "\n [" + type + "] : " + myLog;
        myLogQueue.Enqueue(newString);
        if (type == LogType.Exception)
        {
            newString = "\n" + stackTrace;
            myLogQueue.Enqueue(newString);
        }
        myLog = string.Empty;
        foreach(string mylog in myLogQueue)
        {
            myLog += mylog;
        }
    }

    void Update()
    {
        if (framesPassed >= frames)
        {
            framesPassed = 0;
            outputLabel.text = myLog;
        }
        framesPassed++;
    }
}
