using System.Collections.Generic;

namespace CommandoSharpPlus.CommandStuff
{
    public class ArgumentCollector
    {
        private Dictionary<string, object> Args { get; set; }
        public ArgumentCollector(Dictionary<string, object> args) => Args = args;
        private object this[string argKey]
        {
            get => Args[argKey];
            set => Args[argKey] = value;
        }
        public T Get<T>(string argKey) => (T) this[argKey];
    }
}