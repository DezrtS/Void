using System.Runtime.InteropServices;

public static class EventLoggerInterop
{
    [DllImport("GameAnalyticsLogger.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void InitializeLogger(string filename);
    
    [DllImport("GameAnalyticsLogger.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void ShutdownLogger();

    [DllImport("GameAnalyticsLogger.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void CreateEventLog(string eventName, string[] keys, string[] values, int count);
}
