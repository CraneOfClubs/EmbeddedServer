using EmbeddedServer.Storages;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmbeddedServer.SSEvents
{
   public class TimeEvent : SmSEvent
    {
        DateTime timeTotrigger;
        public TimeEvent(string Name, Module module, object value) : base (Name, module, value)
        {
            this.Name = Name;
            this.Module = module;
            this.ValueToSet = value;
        }

        public void TriggerAt(DateTime time)
        {
            timeTotrigger = time;
        }

        public override bool ShouldBeInvoked()
        {
            return DateTime.Now > timeTotrigger;
        }

        public override void Invoke()
        {
            Module.ChangeStateFromUser(ValueToSet);
        }
    }
}
