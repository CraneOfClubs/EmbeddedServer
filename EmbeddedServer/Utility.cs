using EmbeddedServer.Storages;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmbeddedServer
{
    public static class Utility
    {
        public static bool WrapStates(string State)
        {
            if (State.Contains("off"))
            {
                return false;
            }
            else
                return true;
        }

        public static string UnWrapStates(bool State)
        {
            if (State)
            {
                return "on";
            }
            else
                return "off";
        }

        public static string UnWrapStates(Module module)
        {
            if (module is Trigger)
            {
                if ((module as Trigger).State)
                {
                    return "on";
                }
                else
                    return "off";
            }
            if (module is VariadicTrigger)
            {
                return (module as VariadicTrigger).State.ToString();
            }
            if (module is Sensor)
            {
                return (module as Sensor).State.ToString();
            }
            return "";

        }
    }
}
