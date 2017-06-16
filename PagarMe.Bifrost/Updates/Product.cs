using NLog;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace PagarMe.Bifrost.Updates
{
    public class Product
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static String GetCode(String filename)
        {
            try
            {
                var content = File.ReadAllText(filename);

                var guidFormat = "{[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}}";
                var pattern = $@"ProductName({guidFormat})";
                var regex = new Regex(pattern);
                var matches = regex.Matches(content);

                return matches[0].Groups[1].Value;
            }
            catch (Exception e)
            {
                logger.Error("Program registry may be not found:");
                logger.Error(e);
                return null;
            }
        }
    }
}
