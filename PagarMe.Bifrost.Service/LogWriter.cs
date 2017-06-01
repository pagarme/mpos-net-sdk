using NLog;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PagarMe.Bifrost.Service
{
    abstract class LogWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;

        protected abstract Action<object> write { get; }
        protected static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static LogInfoWriter Info = new LogInfoWriter();
        public static LogErrorWriter Error = new LogErrorWriter();

        public override void Write(object value)
        {
            write(value);
        }

        public override void Write(bool value)
        {
            write(value);
        }

        public override void Write(char value)
        {
            write(value);
        }

        public override void Write(char[] buffer)
        {
            write(new String(buffer));
        }

        public override void Write(char[] buffer, int index, int count)
        {
            write(new String(buffer, index, count));
        }

        public override void Write(decimal value)
        {
            write(value);
        }

        public override void Write(double value)
        {
            write(value);
        }

        public override void Write(float value)
        {
            write(value);
        }

        public override void Write(int value)
        {
            write(value);
        }

        public override void Write(long value)
        {
            write(value);
        }

        public override void Write(string format, object arg0)
        {
            write(String.Format(format, arg0));
        }

        public override void Write(string format, object arg0, object arg1)
        {
            write(String.Format(format, arg0, arg1));
        }

        public override void Write(string format, object arg0, object arg1, object arg2)
        {
            write(String.Format(format, arg0, arg1, arg2));
        }

        public override void Write(string format, params object[] arg)
        {
            write(String.Format(format, arg));
        }

        public override void Write(string value)
        {
            write(value);
        }

        public override void Write(uint value)
        {
            write(value);
        }

        public override void Write(ulong value)
        {
            write(value);
        }

        public override Task WriteAsync(char value)
        {
            return Task.Run(() => { write(value); });
        }

        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            return Task.Run(() => { Write(buffer, index, count); });
        }

        public override Task WriteAsync(string value)
        {
            return Task.Run(() => { Write(value); });
        }

        public override void WriteLine()
        {
            Write(String.Empty);
        }

        public override void WriteLine(bool value)
        {
            Write(value);
        }

        public override void WriteLine(char value)
        {
            Write(value);
        }

        public override void WriteLine(char[] buffer)
        {
            Write(buffer);
        }

        public override void WriteLine(char[] buffer, int index, int count)
        {
            Write(buffer, index, count);
        }

        public override void WriteLine(decimal value)
        {
            Write(value);
        }

        public override void WriteLine(double value)
        {
            Write(value);
        }

        public override void WriteLine(float value)
        {
            Write(value);
        }

        public override void WriteLine(int value)
        {
            Write(value);
        }

        public override void WriteLine(long value)
        {
            Write(value);
        }

        public override void WriteLine(object value)
        {
            Write(value);
        }

        public override void WriteLine(string format, object arg0)
        {
            Write(format, arg0);
        }

        public override void WriteLine(string format, object arg0, object arg1)
        {
            Write(format, arg0, arg1);
        }

        public override void WriteLine(string format, object arg0, object arg1, object arg2)
        {
            Write(format, arg0, arg1, arg2);
        }

        public override void WriteLine(string format, params object[] arg)
        {
            Write(format, arg);
        }

        public override void WriteLine(string value)
        {
            Write(value);
        }

        public override void WriteLine(uint value)
        {
            Write(value);
        }

        public override void WriteLine(ulong value)
        {
            Write(value);
        }

        public override Task WriteLineAsync()
        {
            return Task.Run(() => { WriteLine(); });
        }

        public override Task WriteLineAsync(char value)
        {
            return WriteAsync(value);
        }

        public override Task WriteLineAsync(char[] buffer, int index, int count)
        {
            return WriteAsync(buffer, index, count);
        }

        public override Task WriteLineAsync(string value)
        {
            return WriteAsync(value);
        }

        public class LogInfoWriter : LogWriter
        {
            protected override Action<object> write => logger.Info;
        }

        public class LogErrorWriter : LogWriter
        {
            protected override Action<object> write => logger.Error;
        }
    }
}