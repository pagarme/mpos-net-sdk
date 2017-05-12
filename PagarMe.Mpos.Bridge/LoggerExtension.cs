using NLog;
using System;
using System.Threading.Tasks;

namespace PagarMe.Mpos.Bridge
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

    }
}
