using System;
using System.Collections.Generic;
using System.Text;

namespace EmbeddedServer.Storages
{
    public class VariadicTrigger : Module
    {
        private Int32 _state;
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
        public VariadicTrigger(string Name, string ExternalName) : base(Name, ExternalName)
        {
            this.Name = Name;
        }

        public override void ChangeStateFromUser(object value)
        {
            if (value is int)
                _state = (int)value;
            if (value is bool)
            {
                if ((bool)value)
                {
                    _state = 1;
                }
                else
                    _state = 0;
            }
            _validated = false;
        }

        public override void ChangeStateFromDevice(Module m)
        {
            if (!_validated)
                return;
            _state = (m as VariadicTrigger)._state;
        }

        public override void ChangeStateFromDevice(string value)
        {
            if (!_validated)
                return;
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

        public override bool ValidationNeeded()
        {
            return !_validated;
        }
    }
}
