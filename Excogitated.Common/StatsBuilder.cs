using Excogitated.Common.Atomic.Collections;
using Excogitated.Common.Extensions;
using System;
using System.Linq;
using System.Text;

namespace Excogitated.Common
{

    public class StatsBuilder
    {
        private readonly AtomicList<(string Name, object Value)> _stats = new AtomicList<(string, object)>();
        private readonly string _title;

        public StatsBuilder(string title = null)
        {
            _title = title;
        }

        public StatsBuilder Add(string name, object value)
        {
            _stats.Add((name ?? string.Empty, value));
            return this;
        }

        public StatsBuilder Add(string name, double count, double total)
        {
            var value = $"{Math.Round(count, 2)} {(count / total).ToPercent()}";
            _stats.Add((name ?? string.Empty, value));
            return this;
        }

        public StatsBuilder AddSeparator()
        {
            _stats.Add((string.Empty, null));
            return this;
        }

        public override string ToString()
        {
            var message = new StringBuilder(_title).AppendLine();
            var maxLength = _stats.Max(s => s.Name.Length);
            foreach (var s in _stats)
                message.Append(s.Name.PadLeft(maxLength, ' ')).Append(" : ").Append(s.Value).AppendLine();
            return message.ToString();
        }
    }
}
