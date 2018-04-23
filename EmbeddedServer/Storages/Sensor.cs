using System;
using System.Collections.Generic;
using System.Text;

namespace EmbeddedServer.Storages
{
    public class Sensor : Module
    {
        private Int32 _state;
        private bool _handled;

        public string Message()
        {
            StringBuilder buf = new StringBuilder();
            if (_handled)
            {
                return "";
                //buf.AppendFormat("module_name={0};set_state={1};", Name, _state);
                //if (Deadline > 0)
                //    buf.AppendFormat("deadline={0};", _deadline);
                //else
                //    buf.AppendFormat("deadline=no;");
                //return buf.ToString();
            }
            else return "";
        }

        public override void ChangeStateFromDevice(string value)
        {
            if (value.Contains("off"))
            {
                _state = 0;
            }
            if (value.Contains("on"))
            {
                _state = 1;
            }
            Int32 val = 0;
            if (Int32.TryParse(value, out val))
            {
                _state = val;
            }
        }

        public override void ChangeStateFromUser(object value)
        {

        }

        public override void ChangeStateFromDevice(Module m)
        {
            _state = (m as Sensor)._state;
        }

        public bool NeedHandling
        {
            private set { }
            get
            {
                return _handled;
            }
        }

        public Int32 State
        {
            set
            {
                _state = value;
            }
            get
            {
                return _state;
            }
        }
        public Sensor(string Name, string ExternalName) : base(Name, ExternalName)
        {
            this.Name = Name;
        }

        public override bool ValidationNeeded()
        {
            return false;
        }
    }
}
