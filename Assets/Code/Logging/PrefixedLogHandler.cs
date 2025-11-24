using System;
using System.Diagnostics;
using UnityEngine;

public class PrefixedLogHandler : ILogHandler
{
    private readonly ILogHandler inner;

    public PrefixedLogHandler(ILogHandler inner)
    {
        this.inner = inner;
    }

    public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
    {
        string className = GetClassNameFromContextOrStack(context);
        string newFormat = $"[{className}] {format}";
        inner.LogFormat(logType, context, newFormat, args);
    }

    public void LogException(Exception exception, UnityEngine.Object context)
    {
        inner.LogException(exception, context);
    }

    private static string GetClassNameFromContextOrStack(UnityEngine.Object context)
    {
        // If you call Debug.Log("msg", this), we can use the context’s type directly
        if (context != null)
            return context.GetType().Name;

        // Fallback: inspect the call stack to guess the calling class
        var stackTrace = new StackTrace(4, false); // skip a few internal frames
        var frame = stackTrace.GetFrame(0);
        var method = frame?.GetMethod();
        var type = method?.DeclaringType;

        // Find last "at" in stack trace to get the most relevant caller
        var methodName = stackTrace.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)[0];
        if (methodName != null)
        {
            int atIndex = methodName.LastIndexOf(" at ");
            if (atIndex >= 0)
            {
                string fullMethodName = methodName.Substring(atIndex + 4);
                string className = fullMethodName.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)[0];
                return className;
            }
        }

        return type != null ? type.Name : "Unknown";
    }

}
