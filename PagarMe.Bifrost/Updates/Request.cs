using Newtonsoft.Json;
using NLog;
using PagarMe.Bifrost.WebSocket;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace PagarMe.Bifrost.Updates
{
    public class RequestMaker
    {
        public RequestMaker(String mainUrl)
        {
            this.mainUrl = mainUrl;
        }

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly string mainUrl;

        public async Task<T> GetObjectFromUrl<T>(String path = null)
        {
            var json = await getContentFromUrl(path);
            return JsonConvert.DeserializeObject<T>(json, SnakeCase.Settings);
        }

        private async Task<String> getContentFromUrl(String path = null)
        {
            return await getFromUrl((stream) =>
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }, path);
        }

        private async Task<T> getFromUrl<T>(Func<Stream, T> action, String path = null)
        {
            var request = WebRequest.Create($"{mainUrl}/{path}");

            var response = (HttpWebResponse)await request.GetResponseAsync();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                logger.Warn($"Update failed: {mainUrl}/{path} returned status code {response.StatusCode}");
                return default(T);
            }

            using (var stream = response.GetResponseStream())
            {
                return action(stream);
            }
        }

        public async Task<Boolean> DownloadBinaryFromUrl(String filename, String path = null)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var client = new WebClient();
                    client.DownloadFile($"{mainUrl}/{path}", filename);
                    return true;
                }
                catch (Exception e)
                {
                    logger.Error(e);
                    return false;
                }
            });
        }
    }
}
