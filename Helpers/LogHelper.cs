using System;
using NLog;

namespace Easy.Common.NetCore.Helpers
{
    public static class LogHelper
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly string _propertyKey = "filename";

        public static void Info(string message, string filename = "")
        {
            logger.WithProperty(_propertyKey, filename).Info(message);
        }

        public static void Info(Exception ex, string message, string filename = "")
        {
            logger.WithProperty(_propertyKey, filename).Info(ex, message);
        }

        public static void Trace(string message, string filename = "")
        {
            logger.WithProperty(_propertyKey, filename).Trace(message);
        }

        public static void Trace(Exception ex, string message, string filename = "")
        {
            logger.WithProperty(_propertyKey, filename).Trace(ex, message);
        }

        public static void Debug(string message, string filename = "")
        {
            logger.WithProperty(_propertyKey, filename).Debug(message);
        }

        public static void Debug(Exception ex, string message, string filename = "")
        {
            logger.WithProperty(_propertyKey, filename).Debug(ex, message);
        }

        public static void Warn(string message, string filename = "")
        {
            logger.WithProperty(_propertyKey, filename).Warn(message);
        }

        public static void Warn(Exception ex, string message, string filename = "")
        {
            logger.WithProperty(_propertyKey, filename).Warn(ex, message);
        }

        public static void Error(string message, string filename = "")
        {
            logger.WithProperty(_propertyKey, filename).Error(message);
        }

        public static void Error(Exception ex, string message, string filename = "")
        {
            logger.WithProperty(_propertyKey, filename).Error(ex, message);
        }

        public static void Fatal(string message, string filename = "")
        {
            logger.WithProperty(_propertyKey, filename).Fatal(message);
        }

        public static void Fatal(Exception ex, string message, string filename = "")
        {
            logger.WithProperty(_propertyKey, filename).Fatal(ex, message);
        }
    }
}