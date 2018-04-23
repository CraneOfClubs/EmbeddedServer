using EmbeddedServer.SSEvents;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmbeddedServer
{

    public static class Scheduler
    {
        private static List<SmSEvent> _events = new List<SmSEvent>();

        public static void AddEvent(SmSEvent _event)
        {
            _events.Add(_event);
        }
    
        public static List<SmSEvent> GetReadyEvents(out bool empty)
        {
            List<SmSEvent> ret = new List<SmSEvent>();
            foreach (var ev in _events)
            {
                if (ev.ShouldBeInvoked())
                {
                    ret.Add(ev);
                }
            }
            if (ret.Count == 0)
            {
                empty = true;
            }
            else
                empty = false;
            return ret;
        }

        public static void InvokeReadyEvents()
        {
            bool empty = true;
            var _events = GetReadyEvents(out empty);
            if (empty)
            {
                return;
            } else
            {
                foreach(var _ev in _events)
                {
                    _ev.Invoke();
                }
            }
        }
    }
}
