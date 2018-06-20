using System;
using System.Net;
using System.Threading.Tasks;

namespace PagarMe.Mpos
{
    public class TLS
    {
        /// <summary>
        /// Changes the default protocol to TLS 1.2
        /// to run the request that needs this protocol
        /// then return to previous configuration
        /// </summary>
        public static async Task<T> RunWithTLS12<T>(Func<Task<T>> requestCode)
        {
            var defaultProtocol = ServicePointManager.SecurityProtocol;

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var result = await requestCode();

            ServicePointManager.SecurityProtocol = defaultProtocol;

            return result;
        }
    }
}
