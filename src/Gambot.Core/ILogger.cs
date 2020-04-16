using System;

namespace Gambot.Core
{
    public interface ILogger
    {
        void Trace(string message, params object[] formatArgs);
        void Trace(Exception ex, string message, params object[] formatArgs);

        void Debug(string message, params object[] formatArgs);
        void Debug(Exception ex, string message, params object[] formatArgs);

        void Info(string message, params object[] formatArgs);
        void Info(Exception ex, string message, params object[] formatArgs);

        void Warn(string message, params object[] formatArgs);
        void Warn(Exception ex, string message, params object[] formatArgs);

        void Error(string message, params object[] formatArgs);
        void Error(Exception ex, string message, params object[] formatArgs);

        void Fatal(string message, params object[] formatArgs);
        void Fatal(Exception ex, string message, params object[] formatArgs);

        ILogger GetChildLog(string name);
    }
}