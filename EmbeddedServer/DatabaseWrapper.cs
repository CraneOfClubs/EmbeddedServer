using EmbeddedServer.Storages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MySql.Data.EntityFrameworkCore;
using System.Text;
using System.Linq;

namespace EmbeddedServer
{
    //public static const string ConnectionString = @"server=localhost;UserId=root;Password=Phyxdload715;database=smartdevices;characterset=utf8;SslMode=none;Convert Zero Datetime=True";
    [Table("devices")]
    public class DeviceEntity
    {
        [Column(name: "internal_name")]
        public String InternalName { get; set; }
        [Column(name: "external_name")]
        public String ExternalName { get; set; }
        [Column(name: "registered")]
        public bool? Registered { get; set; }
        [Column(name: "auth_token")]
        public String AuthToken { get; set; }
        [Required, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(name: "iddevices")]
        public Int64 DeviceId { get; set; }

        [Column(name: "last_sync")]
        public DateTime LastSyncTime { get; set; }
        [Column(name: "local_time")]
        public Int64 DeviceLocalTime { get; set; }
    }

    [Table("modules")]
    public class ModuleEntity
    {
        [Column(name: "device_name")]
        public String DeviceName { get; set; }
        [Column(name: "external_name")]
        public String ExternalName { get; set; }
        [Column(name: "module_name")]
        public String ModuleName { get; set; }
        [Column(name: "state")]
        public Int64 ModuleState { get; set; }
        [Required, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(name: "idmodules")]
        public Int64 ModuleId { get; set; }
        [Column(name: "type")]
        public String ModuleType { get; set; }
    }

    [Table("sensor_data")]
    public class SensorEntity
    {
        [Column(name: "device_name")]
        public String DeviceName { get; set; }
        [Column(name: "module_name")]
        public String ModuleName { get; set; }
        [Column(name: "sync_time")]
        public DateTime LastSyncTime { get; set; }
        [Required, Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(name: "idsensor_data")]
        public Int64 ModuleId { get; set; }

        [Column(name: "value")]
        public Int64 Value { get; set; }
    }


    public class DeviceContext : DbContext
    {
        public virtual DbSet<DeviceEntity> Devices { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(DatabaseWrapper.ConnectionString);
        }
    }

    public class ModuleContext : DbContext
    {
        public virtual DbSet<ModuleEntity> Modules { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(DatabaseWrapper.ConnectionString);
        }
    }

    public class SensorDataContext : DbContext
    {
        public virtual DbSet<SensorEntity> SensorData { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(DatabaseWrapper.ConnectionString);
        }
    }

    static public class DatabaseWrapper
    {
        public const string ConnectionString = @"server=localhost;UserId=root;Password=Phyxdload715;database=smartdevices;characterset=utf8;SslMode=none;Convert Zero Datetime=True";

        static public bool IsRegistered(Device device)
        {
            using (var context = new DeviceContext())
            {
                var test = context.Devices.Where(b => b.InternalName == device.Name);
                var _devices = test.ToList<DeviceEntity>();
                if (_devices.Count == 0)
                {
                    StoreDevice(device);
                    return false;
                } else
                {
                    if (_devices[0].Registered == true)
                    {
                            return true;
                    }
                }
            }
            return false;
        }

        static public void RegisterDevice(string name)
        {
            using (var context = new DeviceContext())
            {
                var test = context.Devices.Where(b => b.InternalName == name);
                var _devices = test.ToList<DeviceEntity>();
                _devices[0].Registered = true;
                context.SaveChanges();
            }
        }
        static public bool AuthorizeDevice(Device device)
        {
            if (!IsRegistered(device))
                return false;
            string auth = Guid.NewGuid().ToString();

            using (var context = new DeviceContext())
            {
                var _device = context.Devices.Where(b => b.InternalName == device.Name).ToList<DeviceEntity>()[0];
                _device.AuthToken = auth;
                context.SaveChanges();
                device.EnqueueAuth(auth);
            }
            return true;
        }

        static public List<Module> GetModulesByDevice(Device device)
        {
            return GetModulesByDevice(device.Name);
        }

        static public List<Module> GetModulesByDevice(string deviceName)
        {
            var ret = new List<Module>();
            using (var context = new ModuleContext())
            {
                var _modules = context.Modules.Where(b => b.DeviceName == deviceName).ToList<ModuleEntity>();
                foreach (var _moduleEntity in _modules)
                {
                    if (_moduleEntity.ModuleType == "bool_trigger")
                    {
                        var bufModule = new Trigger(_moduleEntity.ModuleName, _moduleEntity.ExternalName);
                        bufModule.State = (_moduleEntity.ModuleState != 0);
                        ret.Add(bufModule);
                    }
                    if (_moduleEntity.ModuleType == "variadic_trigger")
                    {
                        var bufModule = new VariadicTrigger(_moduleEntity.ModuleName, _moduleEntity.ExternalName);
                        bufModule.State = (int)_moduleEntity.ModuleState;
                        ret.Add(bufModule);
                    }
                    if (_moduleEntity.ModuleType == "sensor")
                    {
                        var bufModule = new Sensor(_moduleEntity.ModuleName, _moduleEntity.ExternalName);
                        bufModule.State = (int)_moduleEntity.ModuleState;
                        ret.Add(bufModule);
                    }
                }
            }
            return ret;
        }

        static public void DeleteModule(string deviceName, string moduleName)
        {
            using (var context = new ModuleContext())
            {
                var _modules = context.Modules.Where(b => b.DeviceName == deviceName && b.ModuleName == moduleName).ToList<ModuleEntity>();
                if (_modules.Count > 0)
                {
                    foreach (var m in _modules)
                    {
                        context.Remove(m);
                    }
                }
                context.SaveChanges();
            }
        }

        static public void DeleteDevice(string deviceName)
        {
            var modules = GetModulesByDevice(deviceName);
            using (var context = new DeviceContext())
            {
                var _devices = context.Devices.Where(b => b.InternalName == deviceName).ToList<DeviceEntity>();
                if (_devices.Count > 0)
                {
                    foreach (var d in _devices)
                    {
                        context.Remove(d);
                    }
                }
                context.SaveChanges();
            }
            foreach(var m in modules)
            {
                DeleteModule(deviceName, m.Name);
            }
        }

        static public bool IsDeviceExist(string deviceName)
        {
            using (var context = new DeviceContext())
            {
                return context.Devices.Where(b => b.InternalName == deviceName).Count() > 0;
            }
        }

        static public DeviceInfo LoadDeviceFromDB(string deviceName)
        {
            var bufDeviceInfo = new DeviceInfo();
            using (var context = new DeviceContext())
            {
                var _bufDevEntity = context.Devices.Where(b => b.InternalName == deviceName).ToList<DeviceEntity>()[0];
                var _bufDevice = new Device(_bufDevEntity.InternalName, _bufDevEntity.ExternalName);
                _bufDevice.AuthToken = _bufDevEntity.AuthToken;
                _bufDevice.CurrentTime = 0;
                _bufDevice.Registered = _bufDevEntity.Registered;
                if (_bufDevEntity.Registered == true)
                {
                    bufDeviceInfo.AwaitingRegistration = false;
                }
                else
                    bufDeviceInfo.AwaitingRegistration = true;
                bufDeviceInfo.Device = _bufDevice;
                var moduleList = GetModulesByDevice(_bufDevice);
                foreach (var xM in moduleList)
                {
                    _bufDevice.AddModule(xM);
                }
            }

            return bufDeviceInfo;
        }
        static public void StoreDevice(Device device)
        {
            using (var context = new DeviceContext())
            {
                if (context.Devices.Where(b => b.InternalName == device.Name).Count() == 0)
                {
                    var _bufDeviceEntity = new DeviceEntity();
                    _bufDeviceEntity.DeviceLocalTime = device.CurrentTime;
                    _bufDeviceEntity.InternalName = device.Name;
                    _bufDeviceEntity.Registered = device.Registered;
                    _bufDeviceEntity.LastSyncTime = DateTime.Now;
                    _bufDeviceEntity.AuthToken = null;
                    context.Add(_bufDeviceEntity);
                    context.SaveChanges();
                }

            }
            using (var context = new ModuleContext())
            {
                foreach (var _module in device.Modules)
                {
                    var _ModulesEntity = context.Modules.Where(b => (b.DeviceName == device.Name && b.ModuleName == _module.Name)).ToList<ModuleEntity>();
                    var _moduleEntity = new ModuleEntity();
                    if (_ModulesEntity.Count() == 0)
                    {
                        _moduleEntity = new ModuleEntity();
                    } else
                    {
                        _moduleEntity = _ModulesEntity[0];
                    }

                    _moduleEntity.DeviceName = device.Name;
                    _moduleEntity.ModuleName = _module.Name;
                    _moduleEntity.ExternalName = _module.ExternalName;

                    if (_module is Trigger)
                    {
                        _moduleEntity.ModuleType = "bool_trigger";
                        _moduleEntity.ModuleState = (_module as Trigger).State ? 1 : 0;
                    }

                    if (_module is VariadicTrigger)
                    {
                        _moduleEntity.ModuleType = "variadic_trigger";
                        _moduleEntity.ModuleState = (_module as VariadicTrigger).State;
                    }
                    if (_module is Sensor)
                    {
                        _moduleEntity.ModuleType = "sensor";
                        _moduleEntity.ModuleState = (_module as Sensor).State;
                        DatabaseWrapper.StoreSensor(_module as Sensor, device);
                    }
                    if (_ModulesEntity.Count() == 0)
                    {
                        context.Add(_moduleEntity);
                    }
                }
                context.SaveChanges();
            }

        }

        static public List<Device> LoadAllDevices()
        {
            var ret = new List<Device>();
            using (var context = new DeviceContext())
            {
                foreach (var _dE in context.Devices)
                {
                    var bufDev = new Device(_dE.InternalName, _dE.ExternalName);
                    bufDev.Registered = _dE.Registered;
                    bufDev.AuthToken = _dE.AuthToken;
                    bufDev.CurrentTime = 0;
                    bufDev.LastSync = _dE.LastSyncTime;
                    foreach(var m in GetModulesByDevice(bufDev))
                    {
                        bufDev.AddModule(m);
                    }
                    ret.Add(bufDev);
                    context.SaveChanges();
                }
            }
            return ret;
        }

        static public void ChangeExternalNameOfDevice(string deviceName, string setTo)
        {
            using (var context = new DeviceContext())
            {
                var _device = context.Devices.Where(b => b.InternalName == deviceName).ToList<DeviceEntity>()[0];
                _device.ExternalName = setTo;
                context.SaveChanges();
            }
        }

        static public void ChangeExternalNameOfModule(string deviceName, string moduleName, string setTo)
        {
            using (var context = new ModuleContext())
            {
                var _modules = context.Modules.Where(b => b.DeviceName == deviceName && b.ModuleName == moduleName).ToList<ModuleEntity>()[0];
                _modules.ExternalName = setTo;
                context.SaveChanges();
            }
        }
        static public void StoreModule(Module module, Device device)
        {

        }

        static public void StoreSensor(Sensor sensor, Device device)
        {
            using (var context = new SensorDataContext())
            {
                var _sensorEntity = new SensorEntity();
                _sensorEntity.DeviceName = device.Name;
                _sensorEntity.LastSyncTime = DateTime.Now;
                _sensorEntity.Value = sensor.State;
                _sensorEntity.ModuleName = sensor.Name;
                context.SensorData.Add(_sensorEntity);
                context.SaveChanges();
            }
        }

        static public List<Tuple<DateTime, long>> GetSensorLog(Sensor sensor, Device device, DateTime start, DateTime finish)
        {
            var ret = new List<Tuple<DateTime, long>>();
            using (var context = new SensorDataContext())
            {
                var _res = context.SensorData.Where(d => (d.DeviceName == device.Name && d.ModuleName == sensor.Name 
                    && d.LastSyncTime > start && d.LastSyncTime < finish)).ToList<SensorEntity>();
                foreach (var entry in _res)
                {
                    var bufEntry = new Tuple<DateTime, long>(entry.LastSyncTime, entry.Value);
                    ret.Add(bufEntry);
                   
                }
                ret.Sort();
            }
            return ret;
        }

        static public bool CheckAuth(Device device)
        {
            if (!IsRegistered(device))
                return false;

            using (var context = new DeviceContext())
            {
                var _device = context.Devices.Where(b => b.InternalName == device.Name).ToList<DeviceEntity>()[0];
                if (_device.AuthToken == device.AuthToken)
                    return true;
            }
            return false;
        }
    }
}
