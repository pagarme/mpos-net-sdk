using PagarMe.Generic;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace PagarMe.Bifrost.Updates
{
    public class Product
    {
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
                Log.Me.Error("Program registry may be not found:");
                Log.Me.Error(e);
                return null;
            }
        }
    }
}
