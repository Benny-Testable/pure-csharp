using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace OrderKit.RetiredMetrics
{
    /// <summary>
    /// Static Code Analysis / Lint / Rule Violations (retired by TP-6263).
    /// Tools: roslyn-analyzers (CA*/IDE*), sonar-cs (S*). The folder's .editorconfig is
    /// the Configuration File Handling / Custom Rule Validation input.
    /// </summary>
    public class LintScenarios
    {
        // Naming Convention Validation: public field, lower-case and underscore names.
        public int retry_count = 3;
        public const string defaultzone = "local";

        // Code Style Rule Validation: field could be readonly, explicit 'this', no braces.
        private List<string> _audit = new List<string>();

        /// <summary>Unused Variable Detection: unused local, unused parameter, unused private method.</summary>
        public int unusedLocals(int value, string ignored)
        {
            int neverRead = value * 3;
            var alsoUnused = DateTime.UtcNow;
            return value;
        }

        private static void NeverCalled() { }

        /// <summary>Multiple Violations Detection: several rules fire in one method
        /// (empty catch, string concatenation in a loop, magic numbers, catch-all).</summary>
        public string BuildReport(IList<string> rows)
        {
            string result = "";
            for (int i = 0; i < rows.Count; i++)
                result = result + rows[i] + ";";
            try
            {
                if (result.Length > 4096) result = result.Substring(0, 4096);
            }
            catch (Exception)
            {
            }
            this._audit.Add(result);
            return result;
        }

        /// <summary>Complexity Rule Detection: nested ternaries and a long condition.</summary>
        public static string Tier(int a, int b, int c) =>
            a > 10 ? (b > 10 ? (c > 10 ? "AAA" : "AAB") : (c > 10 ? "ABA" : "ABB")) : (b > 10 && c > 10 || a < 0 && b < 0 ? "BAA" : "BBB");

        /// <summary>False Positive Prevention: a justified suppression the analyzer must honour.</summary>
        [SuppressMessage("Security", "CA5394:Do not use insecure randomness",
            Justification = "Shuffles demo data only; not a security context.")]
        public static int ShuffleSeed() => new Random().Next();

        /// <summary>Violation Reporting Validation: obsolete API use produces a stable,
        /// reportable diagnostic (CS0618 / SYSLIB0021).</summary>
        [Obsolete("Use Tier instead.")]
        public static string OldTier(int a) => Tier(a, a, a);

        public static string CallsObsolete() => OldTier(5);
    }
}
