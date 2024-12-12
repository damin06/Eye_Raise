using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Log
{
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Message(object message)
    {
        Debug.Log(message);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Error(object message)
    {
        Debug.LogError(message);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Warning(object message)
    {
        Debug.LogWarning(message);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Exception(Exception message)
    {
        Debug.LogException(message);
    }
}
