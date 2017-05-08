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
                try
                {
                    logger.Error(e);
                }
                catch
                {
                    // Log error should not override other errors
                }

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
                try
                {
                    logger.Error(e);
                }
                catch
                {
                    // Log error should not override other errors
                }

                throw;
            }
        }

    }
}
