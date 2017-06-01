using System;

namespace PagarMe.Generic
{
    public static class StringExtension
    {
        public static String CleanSubject(this String @string)
        {
            return @string.Replace("CN=", "");
        }
    }
}
