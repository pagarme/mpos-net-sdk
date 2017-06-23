using NLog;
using NLog.Targets;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PagarMe.Generic
{
    public static class Log
    {
        public static Logger Me = LogManager.GetCurrentClassLogger();

        public static void TryLogOnException(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                tryLog(e);
                throw;
            }
        }

        public static T TryLogOnException<T>(Func<T> action)
        {
            try
            {
                return action();
            }
            catch (Exception e)
            {
                tryLog(e);
                throw;
            }
        }

        public static async Task TryLogOnExceptionAsync(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception e)
            {
                tryLog(e);
                throw;
            }
        }

        private static void tryLog(Exception exception)
        {
            try
            {
                if (exception == null)
                    return;

                if (exception is AggregateException aggregateException)
                {
                    foreach (var childException in aggregateException.InnerExceptions)
                    {
                        tryLog(childException);
                    }
                }
                else
                {
                    Me.Error(exception);
                    tryLog(exception.InnerException);
                }
            }
            catch
            {
                // Log error should not override other errors
            }
        }

        public static String GetLogFilePath()
        {
            var relativeFileName = getLogFullPath();
            return Path.GetFullPath(relativeFileName);
        }

        public static String GetLogDirectoryPath()
        {
            var relativeFileName = getLogFullPath();
            return Path.GetDirectoryName(relativeFileName);
        }

        private static string getLogFullPath()
        {
            var fileTarget =
                Me.Factory.Configuration
                    .FindTargetByName<FileTarget>("file");

            var logEventInfo = new LogEventInfo { TimeStamp = DateTime.Now };
            var relativeFileName = fileTarget.FileName.Render(logEventInfo);
            return relativeFileName;
        }
    }
}
