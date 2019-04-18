using System;
using System.IO;
using System.Linq;

namespace PagarMe.Mpos
{
    public class PgDebugLog
    {
        private static String path =>
            $"{DateTime.Now:yyyy-MM-dd}.log";

        private static String header =>
            $"{DateTime.Now:HH:mm:ss.fff}";

        public static void Write(Exception exception)
        {
            if (exception is AggregateException agg)
            {
                agg.InnerExceptions.ToList().ForEach(Write);
            }
            else if (exception.InnerException != null)
            {
                Write(exception.InnerException);
            }
            else
            {
                write("EXCEPTION", exception);
            }
        }

        public static void Write(object text)
        {
            write("EXTERNAL", text);
        }

        internal static void WriteLocal(object text)
        {
            write("INTERNAL", text);
        }

        private static void write(String context, object text)
        {
            text = $"{context} {header} {text}";

#if DEBUG
            Console.WriteLine(text);
            File.AppendAllText(path, $"{text}\\n");
#endif
        }
    }
}
