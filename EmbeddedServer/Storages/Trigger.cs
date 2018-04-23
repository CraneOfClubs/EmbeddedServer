using System;
using System.Collections.Generic;
using System.Text;

namespace EmbeddedServer.Storages
{
    public class Trigger : Module
    {
        private bool _state;
        private bool _handled;
        private Int32 _deadline;

        public string Message()
        {
            StringBuilder buf = new StringBuilder();
            if (_handled)
            {
                buf.AppendFormat("module_name={0};set_state={1};", Name, _state);
                if (Deadline > 0)
                    buf.AppendFormat("deadline={0};", _deadline);
                else
                    buf.AppendFormat("deadline=no;");
                return buf.ToString();
            }
            else return "";
        }

        public Int32 Deadline
        {
            get
            {
                return _deadline;
            }
            set
            {
                _deadline = value;
            }
        }
        public bool NeedHandling
        {
            private set { }
            get
            {
                return _handled;
            }
        }

        public bool State
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
        public Trigger(string Name, string ExternalName) : base(Name, ExternalName)
        {
            this.Name = Name;
            this.ExternalName = ExternalName;
        }

        public override void ChangeStateFromUser(object value)
        {
            _state = (bool)value;
            _validated = false; 
        }

        public override void ChangeStateFromDevice(string value)
        {
            if (!_validated)
                return;
            if (value.Contains("off"))
            {
                _state = false;
            }
            if (value.Contains("on"))
            {
                _state = true;
            }
        }

        public override void ChangeStateFromDevice(Module m)
        {
            if (!_validated)
                return;
            _state = (m as Trigger)._state;
        }

        public override bool ValidationNeeded()
        {
            return !_validated;
        }
    }
}
