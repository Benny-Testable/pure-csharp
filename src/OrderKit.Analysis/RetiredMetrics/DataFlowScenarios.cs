using System;
using System.Collections.Generic;

namespace OrderKit.RetiredMetrics
{
    /// <summary>
    /// Data Flow Testing / All Definition + All Uses Coverage (retired by TP-6263).
    /// Tool: cs_all_defs_uses (dataflow.json), with coverlet line hits for def/use coverage.
    /// </summary>
    public static class DataFlowScenarios
    {
        /// <summary>Definition-Use Mapping / Definition-Use Pair Identification:
        /// rate and fee each have one definition and two uses (c-use and p-use).</summary>
        public static decimal InvoiceTotal(decimal net, decimal rate)
        {
            decimal fee = net * rate;          // def fee
            if (fee > 100m)                     // p-use fee
            {
                fee = 100m;                     // redefinition of fee
            }
            return net + fee;                   // c-use fee
        }

        /// <summary>Multiple Definitions Handling: status is defined on three paths
        /// and used once; each def reaches the same use.</summary>
        public static string Status(int paid, int due)
        {
            string status;
            if (paid >= due) status = "settled";
            else if (paid > 0) status = "partial";
            else status = "open";
            return status.ToUpperInvariant();
        }

        /// <summary>Partial Uses Coverage Detection: the p-use of limit on the
        /// "over" branch is never exercised by the tests.</summary>
        public static string CreditCheck(decimal balance, decimal limit)
        {
            decimal headroom = limit - balance;
            if (headroom < 0m) return "over by " + (-headroom);
            if (headroom < limit * 0.1m) return "near";
            return "ok";
        }

        /// <summary>Unreachable Use Detection: temp is defined but its only use
        /// sits after an unconditional return (CS0162).</summary>
        public static int Unreachable(int x)
        {
            int temp = x * 2;
            return x;
#pragma warning disable CS0162
            return temp;
#pragma warning restore CS0162
        }

        /// <summary>Edge Case Handling: definitions on null, empty and boundary inputs.</summary>
        public static int SafeLength(string text, int max)
        {
            int length = text == null ? 0 : text.Length;
            if (max <= 0) return 0;
            return Math.Min(length, max);
        }

        /// <summary>Reporting Validation / Coverage Reporting Validation: counts
        /// defs and uses it saw so a report can be cross-checked.</summary>
        public static IDictionary<string, int> Tally(IEnumerable<string> events)
        {
            var counts = new Dictionary<string, int>();
            foreach (string e in events)
            {
                int current;
                counts.TryGetValue(e, out current);   // def current (out)
                counts[e] = current + 1;              // use current
            }
            return counts;
        }
    }
}
