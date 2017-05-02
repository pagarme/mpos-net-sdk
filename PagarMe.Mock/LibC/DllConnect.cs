using PagarMe.Mpos;
using System.IO;
using System.IO.Ports;
using System.Reflection;
using static PagarMe.Mpos.Mpos;
using static PagarMe.Mpos.Mpos.Native;
using PagarMe.Generic;

namespace PagarMe.Mock.LibC
{
    public class DllConnect
    {
        private static INativeImport dll;
        private static SerialPort port;
        public static Stream Stream { get; private set; }

        private static DllMock dllMock;

        public static void SetMock()
        {
            var field = getField();
            dll = (INativeImport)field.GetValue(null);
            dllMock = new DllMock();
            field.SetValue(null, dllMock);

            setStream(new MemoryStream());
        }

        public static void SetMachine()
        {
            if (dll != null)
            {
                var field = getField();
                field.SetValue(null, dll);
                dll = null;
            }

            setPort();

            setStream(port.BaseStream);
        }

        private static void setPort()
        {
            disposePort();

            port = new SerialPort(Config.Port, Config.BaudRate, Parity.None, 8, StopBits.One);
            port.Open();
        }

        private static void setStream(Stream stream)
        {
            disposeStream();

            Stream = stream;
        }

        private static FieldInfo getField()
        {
            var type = typeof(Native);
            var privateStatic = BindingFlags.Static | BindingFlags.NonPublic;
            return type.GetField("Dll", privateStatic);
        }


        public static void Dispose()
        {
            disposePort();
            disposeStream();
        }

        private static void disposePort()
        {
            if (port == null)
                return;

            port.Close();
            port.Dispose();
            port = null;
        }

        private static void disposeStream()
        {
            if (DllConnect.Stream == null)
                return;

            DllConnect.Stream.Close();
            DllConnect.Stream.Dispose();
            DllConnect.Stream = null;
        }


        internal static void SetInitError(Error error)
        {
            dllMock.resultInit = error;
        }

        internal static void SetUpdateTableError(Error error)
        {
            dllMock.resultTableUpdate = error;
        }
        


    }
}
