using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

namespace PagarMe.Mpos.Devices
{
    public class DeviceManager : IDisposable
    {
        private bool _disposed;
        private Dictionary<string, IDevice> _devices;
        private int _baudRate;

        public DeviceManager(int baudRate)
        {
            _devices = new Dictionary<string, IDevice>();
            _baudRate = baudRate;

            update();
            updateTask();
        }

        public IDevice GetById(string deviceId)
        {
            lock (_devices)
                return _devices[deviceId];
        }

        public IDevice[] FindAvailableDevices()
        {
            lock (_devices)
                return _devices.Values.ToArray();
        }

        public void Dispose()
        {
            _disposed = true;
            _devices = null;
        }

        private Task updateTask()
        {
            return Task.Run(() =>
            {
                if (_disposed)
                    return;

                update();

                Task.Delay(1000).ContinueWith(_ => updateTask());
            });
        }

        private void update()
        {
            var serialPorts = SerialPort.GetPortNames();
            

            lock (_devices)
            {
                var devices = serialPorts.Select(getBySerialPort).ToList();
                var toRemove = _devices.Keys.Except(devices.Select(x => x.Id)).ToArray();

                foreach (var device in devices)
                    _devices[device.Id] = device;

                foreach (var id in toRemove)
                    _devices.Remove(id);
            }
        }

        // TODO: Need to improve this to check if it's a new device on the same port
        private IDevice getBySerialPort(string port)
        {
            foreach (var device in _devices.Values)
            {
                if (device is SerialDevice && ((SerialDevice)device).Port == port)
                    return device;
            }

            return new SerialDevice(port, _baudRate);
        }
    }
}