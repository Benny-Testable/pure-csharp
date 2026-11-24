using System;
using System.Collections.Generic;

namespace OrderKit.RetiredMetrics
{
    /// <summary>
    /// Control Flow Testing / Path Coverage (retired by TP-6263; data kept for re-activation).
    /// Tools: altcover (path / sequence-point coverage), coverlet (branch), lizard.
    /// Every method below is a distinct path shape; RetiredMetricsTests drives some
    /// paths and deliberately leaves others unexecuted so path gaps are measurable.
    /// </summary>
    public static class PathScenarios
    {
        /// <summary>Path Execution Tracking: four independent paths, all four tested.</summary>
        public static string ClassifyShipment(int weightKg, bool express)
        {
            if (weightKg > 30)
            {
                return express ? "heavy-express" : "heavy-standard";
            }
            return express ? "light-express" : "light-standard";
        }

        /// <summary>Partial Path Coverage Detection: 3 of 4 paths are tested; "ban" is a gap.</summary>
        public static string RiskBand(int score, bool flagged)
        {
            if (flagged && score > 90) return "ban";
            if (flagged) return "review";
            if (score > 50) return "watch";
            return "ok";
        }

        /// <summary>Nested Condition Path Testing: three levels of nesting.</summary>
        public static int DiscountPercent(string tier, int quantity, bool loyal)
        {
            if (tier == "gold")
            {
                if (quantity >= 100)
                {
                    if (loyal) return 25;
                    return 20;
                }
                return loyal ? 12 : 10;
            }
            if (tier == "silver")
            {
                if (quantity >= 100) return loyal ? 15 : 12;
                return 5;
            }
            return 0;
        }

        /// <summary>Loop Path Detection: zero, one and many iterations plus an early exit.</summary>
        public static int SumUntilNegative(IList<int> values)
        {
            int sum = 0;
            for (int i = 0; i < values.Count; i++)
            {
                if (values[i] < 0) break;
                sum += values[i];
            }
            return sum;
        }

        /// <summary>Unreachable Path Detection: the second branch can never run (CS0162).</summary>
        public static string Unreachable(int value)
        {
            const bool LegacyMode = false;
            if (LegacyMode)
            {
                return "legacy:" + value;
            }
            if (value > 10 && value < 5)
            {
                return "impossible";
            }
            return "current:" + value;
        }

        /// <summary>Exception Path Handling: normal, caught and finally paths.</summary>
        public static string ParseQuantity(string raw, List<string> log)
        {
            try
            {
                int q = int.Parse(raw);
                if (q < 0) throw new ArgumentOutOfRangeException(nameof(raw));
                return "qty:" + q;
            }
            catch (FormatException)
            {
                return "invalid";
            }
            catch (ArgumentOutOfRangeException)
            {
                return "negative";
            }
            finally
            {
                log.Add(raw);
            }
        }

        /// <summary>Multi-Function Path Tracking: one path crosses three methods.</summary>
        public static decimal Checkout(decimal subtotal, string country)
        {
            decimal taxed = ApplyTax(subtotal, country);
            return ApplyShipping(taxed, country);
        }

        private static decimal ApplyTax(decimal amount, string country) =>
            country == "IN" ? amount * 1.18m : country == "US" ? amount * 1.07m : amount;

        private static decimal ApplyShipping(decimal amount, string country) =>
            amount >= 500m ? amount : amount + (country == "IN" ? 50m : 15m);
    }
}
