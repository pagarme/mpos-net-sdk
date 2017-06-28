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

        public static void TryLogOnException(Action action) { TryLogOnException(action, null); }
        public static void TryLogOnException(Action action, Action<Exception> processInsteadThrow)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                handle(e, processInsteadThrow);
            }
        }

        public static T TryLogOnException<T>(Func<T> action) { return TryLogOnException(action, null); }
        public static T TryLogOnException<T>(Func<T> action, Func<Exception, T> processInsteadThrow)
        {
            try
            {
                return action();
            }
            catch (Exception e)
            {
                return handle(e, processInsteadThrow);
            }
        }

        public static async Task TryLogOnExceptionAsync(Func<Task> action) { await TryLogOnExceptionAsync(action, null); }
        public static async Task TryLogOnExceptionAsync(Func<Task> action, Action<Exception> processInsteadThrow)
        {
            try
            {
                await action();
            }
            catch (Exception e)
            {
                handle(e, processInsteadThrow);
            }
        }

        private static void handle(Exception exception, Action<Exception> processInsteadThrow)
        {
            tryLog(exception);
            if (processInsteadThrow == null) throw exception;
            processInsteadThrow(exception);
        }

        private static T handle<T>(Exception exception, Func<Exception, T> processInsteadThrow)
        {
            tryLog(exception);
            if (processInsteadThrow == null) throw exception;
            return processInsteadThrow(exception);
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
