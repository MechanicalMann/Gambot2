using System;
using Gambot.Core;

namespace Gambot.Bot
{
    public class NLogLogger : Gambot.Core.ILogger
    {
        private readonly NLog.ILogger _logger;

        public string Name { get; }

        public NLogLogger(string name)
        {
            Name = name;
            _logger = NLog.LogManager.GetLogger(name);
        }

        public void Trace(string message, params object[] formatArgs)
        {
            _logger.Trace(message, formatArgs);
        }

        public void Trace(Exception ex, string message, params object[] formatArgs)
        {
            _logger.Trace(ex, message, formatArgs);
        }

        public void Debug(string message, params object[] formatArgs)
        {
            _logger.Debug(message, formatArgs);
        }

        public void Debug(Exception ex, string message, params object[] formatArgs)
        {
            _logger.Debug(ex, message, formatArgs);
        }

        public void Info(string message, params object[] formatArgs)
        {
            _logger.Info(message, formatArgs);
        }

        public void Info(Exception ex, string message, params object[] formatArgs)
        {
            _logger.Info(ex, message, formatArgs);
        }

        public void Warn(string message, params object[] formatArgs)
        {
            _logger.Warn(message, formatArgs);
        }

        public void Warn(Exception ex, string message, params object[] formatArgs)
        {
            _logger.Warn(ex, message, formatArgs);
        }

        public void Error(string message, params object[] formatArgs)
        {
            _logger.Error(message, formatArgs);
        }

        public void Error(Exception ex, string message, params object[] formatArgs)
        {
            _logger.Error(ex, message, formatArgs);
        }

        public void Fatal(string message, params object[] formatArgs)
        {
            _logger.Fatal(message, formatArgs);
        }

        public void Fatal(Exception ex, string message, params object[] formatArgs)
        {
            _logger.Fatal(ex, message, formatArgs);
        }

        public ILogger GetChildLog(string name)
        {
            return new NLogLogger($"{Name}.{name}");
        }
    }
}