using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlavBattle.Trace
{
    public interface IHasTraceData
    {
        TraceData GetTrace();
    }

    public class TraceData
    {
        public bool IsTopLevel { get; private set; } = true;

        private TraceData(string name, params TraceData[] data)
        {
            Name = name;
            if (data != null)
            {
                _data = new List<TraceData>(data.Where(a => a != null));
                foreach (var item in _data)
                {
                    item.IsTopLevel = false;
                }
            }
        }

        private TraceData(string name, string detail, params TraceData[] data)
            : this(name, data)
        {   
            Detail = detail;
        }

        public static TraceData TopLevelTrace(string name, string detail, params TraceData[] data)
        {
            var trace = new TraceData(name, detail, data);
            trace.IsTopLevel = true;
            return trace;
        }

        public static TraceData TopLevelTrace(string name, params TraceData[] data)
        {
            return TopLevelTrace(name, null, data);
        }

        public static TraceData ChildTrace(string name, string detail, params TraceData[] data)
        {
            var trace = new TraceData(name, detail, data);
            trace.IsTopLevel = false;
            return trace;
        }

        public static TraceData ChildTrace(string name, params TraceData[] data)
        {
            return ChildTrace(name, null, data);
        }

        public static TraceData ChildTraceWithContext(string name, UnityEngine.Object context, params TraceData[] data)
        {
            var trace = ChildTrace(name, null, data);
            trace.Context = context;
            return trace;
        }

        public void Add(TraceData data)
        {
            _data.Add(data);
        }

        public IReadOnlyList<TraceData> Data => _data.AsReadOnly();
        private List<TraceData> _data = new List<TraceData>();

        public string Name { get; set; }

        /// <summary>
        /// Unique ID to differentiate different foldouts with same name.
        /// If null, will fallback to Name for foldouts
        /// </summary>
        public string Key { get; set; }

        public string Detail { get; set; }

        public UnityEngine.Object Context { get; set; }
    }
}
