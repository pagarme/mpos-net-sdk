using System;
using System.Runtime.InteropServices;

namespace PagarMe.Mpos.Helpers
{
    class GCHelper
    {
        public static T ManualFree<T>(Func<Action, T> action)
        {
            var pin = default(GCHandle);

            // DO NOT REPLACE () => { pin.Free(); }
            // WITH pin.Free, NOT THE SAME
            var callback = action(() => { pin.Free(); });

            pin = GCHandle.Alloc(callback);

            return callback;
        }


    }
}
