using System;
using System.Collections.Generic;
using System.Text;

namespace EmbeddedServer.Storages
{
    public enum ModType
    {
        TRIGGER,
        SENSOR,
        OTHER
    }


    public class Module
    {
        protected Int32 _requestedValue;
        protected bool _validated;
        //public Int32 RequestedValue
        //{
        //    set
        //    {
        //        _validated = false;
        //        _requestedValue = value;

        //    }
        //    get
        //    {
        //        _validated = true;
        //        return _requestedValue;
        //    }
        //}

        public virtual void ChangeStateFromUser(object value)
        {

        }
        public virtual void ChangeStateFromDevice(string value)
        {

        }

        public virtual void ChangeStateFromDevice(Module m)
        {

        }

        public virtual bool ValidationNeeded()
        {
            return false;
        }

        public void Validate()
        {
            _validated = true;
        }

        public Module(string Name, string ExternalName)
        {
            _validated = true;
            this.Name = Name;
            this.ExternalName = ExternalName;
        }
        public string Name { get; set; }
        public string ExternalName { get; set; }
    }
}
