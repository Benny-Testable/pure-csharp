using System;
using System.Collections.Generic;
using NUnit.Framework;
using OrderKit.RetiredMetrics;

namespace OrderKit.Tests
{
    /// <summary>
    /// Drives the RetiredMetrics fixtures. Gaps are deliberate and listed in
    /// RETIRED-METRICS.md: RiskBand "ban", DataFlow CreditCheck "over",
    /// Grade "D"/"F", CanShip combinations with paid=false, ExportInvoice.
    /// </summary>
    [TestFixture]
    public class RetiredMetricsTests
    {
        [TestCase(31, true, "heavy-express")]
        [TestCase(31, false, "heavy-standard")]
        [TestCase(30, true, "light-express")]
        [TestCase(1, false, "light-standard")]
        public void ClassifyShipmentCoversEveryPath(int kg, bool express, string expected) =>
            Assert.AreEqual(expected, PathScenarios.ClassifyShipment(kg, express));

        [TestCase(10, true, "review")]
        [TestCase(60, false, "watch")]
        [TestCase(10, false, "ok")]
        public void RiskBandLeavesBanPathUntested(int score, bool flagged, string expected) =>
            Assert.AreEqual(expected, PathScenarios.RiskBand(score, flagged));

        [TestCase("gold", 100, true, 25)]
        [TestCase("gold", 100, false, 20)]
        [TestCase("gold", 1, true, 12)]
        [TestCase("silver", 100, false, 12)]
        [TestCase("bronze", 1, false, 0)]
        public void DiscountPercentNestedPaths(string tier, int qty, bool loyal, int expected) =>
            Assert.AreEqual(expected, PathScenarios.DiscountPercent(tier, qty, loyal));

        [Test]
        public void SumUntilNegativeLoopPaths()
        {
            Assert.AreEqual(0, PathScenarios.SumUntilNegative(new List<int>()));
            Assert.AreEqual(5, PathScenarios.SumUntilNegative(new List<int> { 5 }));
            Assert.AreEqual(6, PathScenarios.SumUntilNegative(new List<int> { 1, 2, 3, -1, 9 }));
        }

        [Test]
        public void UnreachableReturnsCurrent() =>
            Assert.AreEqual("current:7", PathScenarios.Unreachable(7));

        [Test]
        public void ParseQuantityExceptionPaths()
        {
            var log = new List<string>();
            Assert.AreEqual("qty:4", PathScenarios.ParseQuantity("4", log));
            Assert.AreEqual("invalid", PathScenarios.ParseQuantity("x", log));
            Assert.AreEqual("negative", PathScenarios.ParseQuantity("-2", log));
            Assert.AreEqual(3, log.Count);
        }

        [Test]
        public void CheckoutCrossesThreeMethods()
        {
            Assert.AreEqual(168m, PathScenarios.Checkout(100m, "IN"));
            Assert.AreEqual(642m, PathScenarios.Checkout(600m, "US"));
        }

        [Test]
        public void InvoiceTotalDefUsePairs()
        {
            Assert.AreEqual(110m, DataFlowScenarios.InvoiceTotal(100m, 0.1m));
            Assert.AreEqual(1100m, DataFlowScenarios.InvoiceTotal(1000m, 0.5m));
        }

        [TestCase(10, 10, "SETTLED")]
        [TestCase(5, 10, "PARTIAL")]
        [TestCase(0, 10, "OPEN")]
        public void StatusMultipleDefinitions(int paid, int due, string expected) =>
            Assert.AreEqual(expected, DataFlowScenarios.Status(paid, due));

        [Test]
        public void CreditCheckPartialUses()
        {
            Assert.AreEqual("ok", DataFlowScenarios.CreditCheck(10m, 100m));
            Assert.AreEqual("near", DataFlowScenarios.CreditCheck(95m, 100m));
        }

        [Test]
        public void DataFlowEdgeCases()
        {
            Assert.AreEqual(0, DataFlowScenarios.SafeLength(null, 5));
            Assert.AreEqual(0, DataFlowScenarios.SafeLength("abc", 0));
            Assert.AreEqual(2, DataFlowScenarios.SafeLength("abc", 2));
            Assert.AreEqual(4, DataFlowScenarios.Unreachable(4));
            var t = DataFlowScenarios.Tally(new[] { "a", "b", "a" });
            Assert.AreEqual(2, t["a"]);
            Assert.AreEqual(1, t["b"]);
        }

        [Test]
        public void CanShipPartialConditionCoverage()
        {
            Assert.IsTrue(LogicScenarios.CanShip(true, true, false));
            Assert.IsTrue(LogicScenarios.CanShip(true, false, true));
            Assert.IsFalse(LogicScenarios.CanShip(true, false, false));
            Assert.IsFalse(LogicScenarios.CanShip(false, true, true));
        }

        [Test]
        public void FreeShippingBoundary()
        {
            Assert.IsTrue(LogicScenarios.QualifiesForFreeShipping(1001));
            Assert.IsFalse(LogicScenarios.QualifiesForFreeShipping(999));
        }

        [Test]
        public void LoyaltyPointsExactValues()
        {
            Assert.AreEqual(10, LogicScenarios.LoyaltyPoints(100, false));
            Assert.AreEqual(20, LogicScenarios.LoyaltyPoints(100, true));
            Assert.AreEqual(998, LogicScenarios.LoyaltyPoints(4999, true));
            Assert.AreEqual(600, LogicScenarios.LoyaltyPoints(5000, false));
            Assert.AreEqual(1100, LogicScenarios.LoyaltyPoints(5000, true));
        }

        [Test]
        public void GradePartialStatements()
        {
            Assert.AreEqual("A", LogicScenarios.Grade(95));
            Assert.AreEqual("B", LogicScenarios.Grade(80));
            Assert.AreEqual("C", LogicScenarios.Grade(60));
            Assert.Throws<ArgumentOutOfRangeException>(() => LogicScenarios.Grade(101));
        }

        [Test]
        public void ShippingCostSamples()
        {
            Assert.AreEqual(40m, LogicScenarios.ShippingCost("local", 1, false, false, false));
            Assert.AreEqual(195m, LogicScenarios.ShippingCost("regional", 10, false, true, false));
        }

        [Test]
        public void ReconcileBatchFindsIssues()
        {
            var batches = new List<Dictionary<string, int>>
            {
                new Dictionary<string, int> { { "A", -5 }, { "ADJ1", -1 }, { "B", 20000 }, { "C", 1 } },
                null,
            };
            List<string> lenient = CognitiveScenarios.ReconcileBatch(batches, 2, false);
            CollectionAssert.AreEqual(new[] { "negative:A", "large:B" }, lenient);
            List<string> strict = CognitiveScenarios.ReconcileBatch(batches, 2, true);
            CollectionAssert.AreEqual(new[] { "negative:A", "negative:ADJ1", "large:B", "short-key:C" }, strict);
        }

        [Test]
        public void LintScenariosStillWork()
        {
            var l = new LintScenarios();
            Assert.AreEqual(3, l.unusedLocals(3, "x"));
            Assert.AreEqual("a;b;", l.BuildReport(new[] { "a", "b" }));
            Assert.AreEqual("AAA", LintScenarios.Tier(11, 11, 11));
        }

        [Test]
        public void DomesticInvoiceIsTested()
        {
            Assert.AreEqual(106.2m, DuplicationScenarios.DomesticInvoice(new List<decimal> { 50m, 50m }, 10m));
        }
    }
}
