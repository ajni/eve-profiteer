using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace eZet.EveProfiteer {


    public class TraceEvent {


        private const int IdLength = 4;

        public int Id { get; set; }

        public string Name { get; set; }

        public string DisplayFormat { get; set; }

        public TraceEvent(int id, string name) {
            Id = id;
            Name = name;
            DisplayFormat = name;
        }

        public TraceEvent(int id, string name, string displayFormat) {
            Id = id;
            Name = name;
            DisplayFormat = displayFormat;
        }

    }


    public static class EventManager {

        public static readonly IDictionary<int, TraceEvent> Events = new Dictionary<int, TraceEvent>();


        [Conditional("TRACE")]
        public static void TraceEvent(this TraceSource source, TraceEventType eventType, TraceEvent traceEvent) {
            source.TraceEvent(eventType, traceEvent.Id, traceEvent.DisplayFormat);
        }

        [Conditional("TRACE")]
        public static void TraceEvent(this TraceSource source, TraceEventType eventType, TraceEvent traceEvent, params object[] args) {
            source.TraceEvent(eventType, traceEvent.Id, traceEvent.DisplayFormat, args);
        }

        public static TraceEvent Event(int id) {
            return Events[id];
        }

        public static TraceEvent Event(string name) {
            return Events.Values.Single(t => t.Name == name);
        }

        public static void Add(TraceEvent traceEvent) {
            Events.Add(traceEvent.Id, traceEvent);
        }

        public static void AddRange(params TraceEvent[] traceEvents) {
            foreach (var e in traceEvents) {
                Events.Add(e.Id, e);
            }
        }

        public static void Add(int id, string name, string message = null) {
            if (message == null) message = "";
            Events.Add(id, new TraceEvent(id, name, message));
        }
    }

}