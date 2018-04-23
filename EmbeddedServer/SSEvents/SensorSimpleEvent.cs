using EmbeddedServer.Storages;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmbeddedServer.SSEvents
{
    public class SensorSimpleEvent : SmSEvent
    {
        Sensor sensorAsTrigger;
        int valueToTrigger;
        SensorSimpleEvent(string Name, Module module, object value) : base(Name, module, value)
        {
            this.Name = Name;
            this.Module = module;
            this.ValueToSet = value;
        }

        public void TriggerAt(Sensor sensor, int value)
        {
            sensorAsTrigger = sensor;
            valueToTrigger = value;
        }

        public override bool ShouldBeInvoked()
        {
            return sensorAsTrigger.State == valueToTrigger;
        }

        public override void Invoke()
        {
            base.Invoke();
        }
    }
}
