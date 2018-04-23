using EmbeddedServer.Storages;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmbeddedServer.SSEvents
{
    public enum Predicate {
        GREATERTHAN,
        LESSTHAN,
        EQUAL
    }
    public class SensorEvent : SmSEvent
    {
        Sensor sensorAsTrigger;
        int valueToTrigger;
        Predicate predicate;
        SensorEvent(string Name, Module module, object value) : base(Name, module, value)
        {
            this.Name = Name;
            this.Module = module;
            this.ValueToSet = value;
        }

        public void TriggerAt(Sensor sensor, int value, Predicate predicate)
        {
            sensorAsTrigger = sensor;
            valueToTrigger = value;
            this.predicate = predicate;
        }

        public override bool ShouldBeInvoked()
        {
            if (predicate == Predicate.EQUAL)
            {
                return valueToTrigger == sensorAsTrigger.State;
            }
            if (predicate == Predicate.GREATERTHAN)
            {
                return valueToTrigger < sensorAsTrigger.State;
            }
            if (predicate == Predicate.LESSTHAN)
            {
                return valueToTrigger > sensorAsTrigger.State;
            }
            return false;
        }
    }
}
