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

        private static void tryLog(Logger logger, Exception e)
        {
            try
            {
                while (e != null)
                {
                    logger.Error(e);
                    e = e.InnerException;
                }
            }
            catch
            {
                // Log error should not override other errors
            }
        }

    }
}
