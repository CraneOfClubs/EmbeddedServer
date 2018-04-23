using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;
using System.Text;

namespace EmbeddedServer.Storages
{
    public class DeviceInfo
    {
        public Device Device { get; set; }
        public bool AwaitingRegistration { get; set; } = true;
    }

    static public class DeviceStorage
    {
        static private List<DeviceInfo> _devices = new List<DeviceInfo>();
        static public List<DeviceInfo> Devices
        {
            get
            {
                return _devices;
            }
            private set { }

        }

        static public void AddDevice(Device DeviceToAdd)
        {
            var _bufDevInfo = new DeviceInfo();
            _bufDevInfo.Device = DeviceToAdd;
            _bufDevInfo.AwaitingRegistration = !DatabaseWrapper.IsRegistered(DeviceToAdd);
            if (!_bufDevInfo.AwaitingRegistration)
            {
                DatabaseWrapper.AuthorizeDevice(DeviceToAdd);
            } else
            {
                _bufDevInfo.Device.EnqueueRegister();
            }
            _devices.Add(_bufDevInfo);
            
        }

        static public void RegisterDevice(Device device)
        {
            RegisterDevice(device.Name);
        }

        static public void RegisterDevice(string name)
        {
            var _device = _devices.Find(x => x.Device.Name == name);
            if (_device != null)
            {
                _device.AwaitingRegistration = false;
                _device.Device.Registered = true;
                DatabaseWrapper.RegisterDevice(name);
                DatabaseWrapper.AuthorizeDevice(_device.Device);
            }
        }

        static public void DeleteDevice(string deviceName)
        {
            var _device = _devices.Find(x => x.Device.Name == deviceName);
            DatabaseWrapper.DeleteDevice(_device.Device.Name);
            _devices.Remove(_device);
        }

        static public bool Contains(string name)
        {
            foreach(var x in Devices)
            {
                if (x.Device.Name == name)
                {
                    return true;
                }
            }
            return false;
        }
        static public Device GetDeviceByName(string name)
        {
            var _device = new Device(null, null);
            _device = Devices.Find(x => x.Device.Name == name).Device;
            return _device;
        }

        static public void ChangeExternalNameOfDevice(string deviceName, string setTo)
        {
            var _device = GetDeviceByName(deviceName);
            _device.ExternalName = setTo;
        }

        static public void ChangeExternalNameOfModule(string deviceName, string moduleName, string setTo)
        {
            var _module = GetDeviceByName(deviceName).Modules.Find(x => x.Name == moduleName);
            if (_module != null)
                _module.ExternalName = setTo;
        }

        static public bool DeviceExists(string name)
        {
            return Devices.Find(x => x.Device.Name == name) != null;
        }

        static public void LoadDevicesFromDb()
        {
            var _dbM = DatabaseWrapper.LoadAllDevices();
            foreach(var _d in _dbM)
            {
                var _bufDevInfo = new DeviceInfo();
                _bufDevInfo.Device = _d;
                _bufDevInfo.AwaitingRegistration = !DatabaseWrapper.IsRegistered(_d);
                _devices.Add(_bufDevInfo);
            }

        }
    }
}
