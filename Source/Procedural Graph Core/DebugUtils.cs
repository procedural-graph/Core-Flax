using FlaxEditor;
using System;

namespace ProceduralGraph;

internal static class DebugUtils
{
    public static void LogException(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception, nameof(exception));
        Editor.LogError($"{exception}\nStack Trace:\n{exception.StackTrace}");
    }

    public static void LogException<T>(Exception exception, T context)
    {
        ArgumentNullException.ThrowIfNull(exception, nameof(exception));
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        Editor.LogError($"{context} - {exception}\nStack Trace:\n{exception.StackTrace}");
    }

    public static void LogException<T>(Exception exception, T context, string statusText)
    {
        ArgumentNullException.ThrowIfNull(exception, nameof(exception));
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(statusText, nameof(statusText));
        Editor.LogError($"{statusText} {context} - {exception}\nStack Trace:\n{exception.StackTrace}");
    }
}
