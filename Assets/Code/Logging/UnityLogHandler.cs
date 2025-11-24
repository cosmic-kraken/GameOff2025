using UnityEngine;

public static class UnityLogHandler
{
    public static ILogHandler Default { get; }

    static UnityLogHandler()
    {
        Default = Debug.unityLogger.logHandler;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    private static void Initialize()
    {
        Debug.unityLogger.logHandler = new PrefixedLogHandler(Default);
    }
}
