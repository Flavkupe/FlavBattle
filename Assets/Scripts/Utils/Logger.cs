using System;
using UnityEngine;

[Flags]
public enum LogSeverity
{
    Info = 1,
    Warning = 2,
    Error = 4,
    Trace = 8,
}

[Flags]
public enum LogType
{
    Misc = 1,
    Combat = 2,
    State = 4,
    GameEvents = 8,
    Init = 16,
}

public class Logger : SingletonObject<Logger>
{
    [EnumFlags]
    public LogType Filter;

    [EnumFlags]
    public LogSeverity Severity;


    private void Start()
    {
        SetSingleton(this);
    }

    public static void Log(LogType type, string message, UnityEngine.Object context = null)
    {
        if (ShouldLog(LogSeverity.Info, type))
        {
            Debug.Log(message, context);
        }
    }

    public static void Error(LogType type, string message, UnityEngine.Object context = null)
    {
        if (ShouldLog(LogSeverity.Error, type))
        {
            Debug.LogError(message, context);
        }
    }

    public static void Warning(LogType type, string message, UnityEngine.Object context = null)
    {
        if (ShouldLog(LogSeverity.Warning, type))
        {
            Debug.LogWarning(message, context);
        }
    }

    public static void Trace(LogType type, string message, UnityEngine.Object context = null)
    {
        if (ShouldLog(LogSeverity.Trace, type))
        {
            Debug.Log(message, context);
        }
    }

    private static bool ShouldLog(LogSeverity severity, LogType type)
    {
        if (Instance == null)
        {
            Debug.LogError("No instance of Logger found; logging all messages.");
            return true;
        }

        return Instance.Filter.HasFlag(type) && Instance.Severity.HasFlag(severity);
    }
}
