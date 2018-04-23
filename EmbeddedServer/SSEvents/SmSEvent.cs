using EmbeddedServer.Storages;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmbeddedServer.SSEvents
{
    public class SmSEvent
    {
        public string Name { get; set; }
        public Module Module { get; set; }
        public object ValueToSet { get; set; }
        public SmSEvent(string Name, Module module, object value)
        {
            this.Name = Name;
            this.Module = module;
            this.ValueToSet = value;
        }

        public virtual bool ShouldBeInvoked()
        {
            return false;
        }

        public virtual void Invoke()
        {
            Module.ChangeStateFromUser(ValueToSet);
        }
    }
}
