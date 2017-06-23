using PagarMe.Generic;
using System;
using System.Threading.Tasks;

namespace PagarMe.Bifrost.Updates
{
    public abstract class Updater
    {
        static Updater()
        {
            instance = getInstance();
        }

        private static Updater getInstance()
        {
            if (ProgramEnvironment.IsUnix)
            {
                return null;
            }

            return new WindowsUpdater();
        }

        public static async Task CheckAndUpdate(Func<Boolean> lockCoreOperations)
        {
            if (instance == null)
            {
                return;
            }

            var hasUpdate = await check();

            if (hasUpdate)
            {
                await releaseUpdate(lockCoreOperations);
            }
        }

        private static async Task<Boolean> check()
        {
            try
            {
                return await Task.Run(() =>
                {
                    lock (instance)
                    {
                        Log.Me.Info("Update check starting");
                        var updateCheckResult = instance.Check();

                        if (!updateCheckResult.HasValue)
                        {
                            Log.Me.Warn("Update check exited with error");
                            return false;
                        }

                        if (!updateCheckResult.Value)
                        {
                            Log.Me.Info($"Bifrost up to date");
                            return false;
                        }

                        return true;
                    }
                });
            }
            catch (Exception e)
            {
                Log.Me.Error(e);
                return false;
            }
        }

        private static async Task releaseUpdate(Func<Boolean> lockCoreOperations)
        {
            while (!lockCoreOperations())
            {
                await Task.Yield();
            }

            lock (instance)
            {
                Log.Me.Info($"Start upgrading");

                var upgraded = instance.Update();

                if (upgraded)
                {
                    Log.Me.Info($"Finished upgrading");
                }
                else
                {
                    Log.Me.Error($"Failed on upgrading service. If the service stopped working, try to restart.");
                }
            }
        }

        private static Updater instance;

        protected abstract Boolean? Check();
        protected abstract Boolean Update();
    }
}
