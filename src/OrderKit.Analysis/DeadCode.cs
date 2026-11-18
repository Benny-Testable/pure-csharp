using System.Collections.Generic;
using System.Linq;

namespace OrderKit.Analysis
{
    /// <summary>
    /// Dead-code fixture. Nothing in the solution references any of the members
    /// below, and one statement can never execute, so a dead-code tool and the
    /// coverage tools' unexecuted-line reports both have a known answer.
    /// </summary>
    internal static class DeadCode
    {
        private const double RetiredRetailRate = 0.11;             // unused constant

        private static double DeprecatedRoundHalfUp(double value, int places)   // unused private method (IDE0051)
        {
            double factor = System.Math.Pow(10, places);
            return (long)(value * factor + 0.5) / factor;
        }

        internal static int UnreachableAfterReturn(int value)
        {
            return value * 2;
#pragma warning disable CS0162 // unreachable on purpose
            return value * 3;
#pragma warning restore CS0162
        }
    }

    /// <summary>An entire class that no caller constructs.</summary>
    internal sealed class AbandonedReportBuilder
    {
        private readonly List<string> _rows = new List<string>();

        public AbandonedReportBuilder Add(string row)
        {
            _rows.Add(row);
            return this;
        }

        public string Render() => string.Join("\n", _rows.Select(r => r));
    }
}
