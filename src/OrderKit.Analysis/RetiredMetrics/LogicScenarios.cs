using System;

namespace OrderKit.RetiredMetrics
{
    /// <summary>
    /// Structural Analysis / Cyclomatic Complexity (Decision, Condition, Logic Coverage,
    /// Maintainability), Branch Coverage / Logic Error Detection, Statement Coverage /
    /// Test Completeness Evaluation, Mutation Score / Code Logic Validation
    /// (all retired by TP-6263). Tools: lizard, coverlet, stryker-net.
    /// </summary>
    public static class LogicScenarios
    {
        /// <summary>Decision / Condition / Logic Coverage: one decision, three conditions,
        /// eight combinations. Tests cover four of them.</summary>
        public static bool CanShip(bool paid, bool inStock, bool addressVerified) =>
            paid && (inStock || addressVerified);

        /// <summary>Logic Error Detection: the boundary is deliberately off by one
        /// (orders of exactly 1000 should qualify); a branch test at 1000 exposes it.</summary>
        public static bool QualifiesForFreeShipping(int amount) => amount > 1000;

        /// <summary>Mutation Score / Code Logic Validation: every operator here is
        /// pinned by an exact-value test, so arithmetic and relational mutants die.</summary>
        public static int LoyaltyPoints(int amount, bool member)
        {
            int points = amount / 10;
            if (member) points = points * 2;
            if (amount >= 5000) points += 100;
            return points;
        }

        /// <summary>Test Completeness Evaluation: statements after the guard are only
        /// partly executed by the tests.</summary>
        public static string Grade(int score)
        {
            if (score < 0 || score > 100) throw new ArgumentOutOfRangeException(nameof(score));
            if (score >= 90) return "A";
            if (score >= 75) return "B";
            if (score >= 60) return "C";
            if (score >= 40) return "D";
            return "F";
        }

        /// <summary>Maintainability Analysis: a long, high-CCN switch-and-if method.</summary>
        public static decimal ShippingCost(string zone, int weight, bool fragile, bool express, bool weekend)
        {
            decimal cost;
            switch (zone)
            {
                case "local": cost = 40m; break;
                case "regional": cost = 80m; break;
                case "national": cost = 120m; break;
                case "international": cost = 600m; break;
                default: cost = 150m; break;
            }
            if (weight > 5) cost += 10m * (weight - 5);
            if (weight > 20) cost += 200m;
            if (fragile) cost *= 1.2m;
            if (express && weekend) cost *= 2m;
            else if (express) cost *= 1.5m;
            else if (weekend) cost += 30m;
            if (zone == "international" && fragile && weight > 20) cost += 500m;
            return Math.Round(cost, 2);
        }
    }
}
