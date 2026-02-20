using FlaxEditor;
using System;

namespace ProceduralGraph.FlaxEngine;

internal sealed class Logger : ILogger
{
    public void LogError(string message, object? context = null)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        if (context is null)
        {
            Editor.LogError(message);
            return;
        }

        Editor.LogError($"{context} - {message}");
    }

    public void LogException(Exception exception, object? context = null)
    {
        if (exception is null)
        {
            return;
        }

        if (context is null)
        {
            Editor.LogError($"{exception}\nStack Trace:\n{exception.StackTrace}");
            return;
        }

        Editor.LogError($"{context} - {exception}\nStack Trace:\n{exception.StackTrace}");
    }

    public void LogInfo(string message, object? context = null)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        if (context is null)
        {
            Editor.Log(message);
            return;
        }

        Editor.Log($"{context} - {message}");
    }

    public void LogWarning(string message, object? context = null)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        if (context is null)
        {
            Editor.LogWarning(message);
            return;
        }

        Editor.LogWarning($"{context} - {message}");
    }
}
