using NLog;
using NLog.Targets;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PagarMe.Generic
{
    public static class LoggerExtension
    {
        public static void TryLogOnException(this Logger logger, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                tryLog(logger, e);
                throw;
            }
        }

        public static T TryLogOnException<T>(this Logger logger, Func<T> action)
        {
            try
            {
                return action();
            }
            catch (Exception e)
            {
                tryLog(logger, e);
                throw;
            }
        }

        public static async Task TryLogOnExceptionAsync(this Logger logger, Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception e)
            {
                tryLog(logger, e);
                throw;
            }
        }

        private static void tryLog(Logger logger, Exception exception)
        {
            try
            {
                if (exception == null)
                    return;

                if (exception is AggregateException aggregateException)
                {
                    foreach (var childException in aggregateException.InnerExceptions)
                    {
                        tryLog(logger, childException);
                    }
                }
                else
                {
                    logger.Error(exception);
                    tryLog(logger, exception.InnerException);
                }
            }
            catch
            {
                // Log error should not override other errors
            }
        }

        public static String GetLogFilePath(this Logger logger)
        {
            var relativeFileName = logger.getLogFullPath();
            return Path.GetFullPath(relativeFileName);
        }

        public static String GetLogDirectoryPath(this Logger logger)
        {
            var relativeFileName = logger.getLogFullPath();
            return Path.GetDirectoryName(relativeFileName);
        }

        private static string getLogFullPath(this Logger logger)
        {
            var fileTarget =
                logger.Factory.Configuration
                    .FindTargetByName<FileTarget>("file");

            var logEventInfo = new LogEventInfo { TimeStamp = DateTime.Now };
            var relativeFileName = fileTarget.FileName.Render(logEventInfo);
            return relativeFileName;
        }
    }
}
