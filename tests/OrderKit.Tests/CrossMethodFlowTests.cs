using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OrderKit;
using OrderKit.Analysis;
using OrderKit.RetiredMetrics;

namespace OrderKit.Tests
{
    /// <summary>
    /// Cross-method data flow and multi-method call chains: a value defined or
    /// returned in one method and read in another. These use only inputs that the
    /// fixtures document as covered; the deliberately untested paths (RiskBand "ban",
    /// CreditCheck "over", the express-fragile and freight lanes) stay untested.
    /// </summary>
    [TestFixture]
    public class CrossMethodFlowTests
    {
        // ---- order -> pricing -> risk -> reporting ---------------------------------

        private static Order HazmatOrder()
        {
            Order o = new Order("F-1", "gold");
            o.AddLine(new OrderLine("HZ-1", 20, 1_000L, "hazmat"));
            return o;
        }

        // gold: 20 000 - 5% = 19 000; + region; bulk rule hazmat >= 10 units, 10% = 2 000 off.
        [TestCase("eu", 18_200L, 38)]             // 19 000 + 1 200 - 2 000;  hazmat 15 + flags 23
        [TestCase("sanctioned", 19_400L, 63)]     // 19 000 + 2 400 - 2 000;  hazmat 40 + flags 23
        public void HighValueOrderFlowsThroughPricingRiskAndReporting(
            string region, long expectedPrice, int expectedRisk)
        {
            Order order = HazmatOrder();                                  // constructor -> properties
            var flags = new List<string> { "hard-fraud", null, "soft-late" };

            long price = new PricingService().PriceWithBulkDiscount(
                order, region, false, 0, new BulkRule("hazmat", 10, 10));
            int risk = new RiskScorer().Score(order, region, flags);

            string key = InputSanitizer.BuildLookupKey(order.CustomerTier, order.FirstSku());
            string path = InputSanitizer.BuildReportPath("/out/", InputSanitizer.Sanitize(key));

            Assert.AreEqual(expectedPrice, price);
            Assert.AreEqual(expectedRisk, risk);
            Assert.AreEqual("tenant/gold/sku/HZ-1", key);
            Assert.AreEqual("/out/tenant/gold/sku/HZ-1", path);
            Assert.IsFalse(InputSanitizer.LooksSuspicious(path));
        }

        [Test]
        public void LowValueOrderFlowsThroughWithNoDiscountAndNoRisk()
        {
            Order order = new Order("F-2", null);                         // tier defaults to "standard"
            order.AddLine(new OrderLine("BOLT", 10, 500L, "general"));

            long price = new PricingService().PriceWithBulkDiscount(
                order, "domestic", false, 0, new BulkRule("general", 100, 10));   // 10 < 100 units
            int risk = new RiskScorer().Score(order, "domestic", null);
            string key = InputSanitizer.BuildLookupKey(order.CustomerTier, order.FirstSku());

            Assert.AreEqual(5_250L, price);                               // 5 000 + 250, no discount
            Assert.AreEqual(0, risk);
            Assert.AreEqual("tenant/standard/sku/BOLT", key);
        }

        [Test]
        public void QuoteScopeCarriesTheOrderIntoAPricedQuote()
        {
            Order order = HazmatOrder();
            var scope = new QuoteScope("acme", "USD");                    // constructor -> property

            PriceQuote quote = scope.Quote(order);                        // order.PricingKey/BaseCents
            long priced = new PricingService().PriceWithBulkDiscount(
                order, "eu", false, 0, new BulkRule("hazmat", 10, 10));
            PriceQuote rebased = quote.Rebased(priced);

            Assert.AreEqual("order:F-1", quote.Key);
            Assert.AreEqual(20_000L, quote.Cents);
            Assert.AreEqual(18_200L, rebased.Cents);
            Assert.AreEqual("USD", rebased.Currency);
            Assert.AreEqual(("acme", "HZ-1"), scope.KeyFor(order.FirstSku()));
        }

        // ---- the five-deep call chain ----------------------------------------------

        [TestCase(0, 0)]
        [TestCase(1, 3)]
        [TestCase(2, 6)]
        [TestCase(5, 15)]
        public void EntryPointDrivesAllFourStages(int value, int expected)
        {
            // StageOne = StageTwo + StageFour; StageThree = 2 * StageFour; StageTwo = StageThree - 3.
            Assert.AreEqual(expected, CallGraphSample.EntryPoint(value));
            Assert.AreEqual(CallGraphSample.StageOne(value), CallGraphSample.EntryPoint(value));
            Assert.AreEqual(CallGraphSample.StageTwo(value) + CallGraphSample.StageFour(value),
                            CallGraphSample.StageOne(value));
            Assert.AreEqual(CallGraphSample.StageThree(value) - 3, CallGraphSample.StageTwo(value));
            Assert.AreEqual(CallGraphSample.StageFour(value) * 2, CallGraphSample.StageThree(value));
        }

        [TestCase(0, "ping", "pong")]
        [TestCase(1, "pong", "ping")]
        [TestCase(2, "ping", "pong")]
        [TestCase(5, "pong", "ping")]
        public void MutualRecursionTakesBothOutcomesAtEveryDepth(int depth, string ping, string pong)
        {
            Assert.AreEqual(ping, CallGraphSample.Ping(depth));
            Assert.AreEqual(pong, CallGraphSample.Pong(depth));
        }

        // ---- classify -> summarise --------------------------------------------------

        private static List<(string Band, double Surcharge)> Shipments() => new()
        {
            ComplexitySample.ClassifyShipment("EU", "gold", 40, true, false, 0, 0, null),
            ComplexitySample.ClassifyShipment("EU", "gold", 10.5, false, false, 2000, 0, null),
            ComplexitySample.ClassifyShipment("EU", "gold", 12, false, false, 0, 0, null),
            ComplexitySample.ClassifyShipment("US", "silver", 5, false, true, 0, 0, null),
            ComplexitySample.ClassifyShipment("US", "plain", 3, false, false, 0, 8, "freeship"),
            ComplexitySample.ClassifyShipment("US", "plain", 60, false, false, 0, 0, null),
        };

        [Test]
        public void ClassifiedShipmentsAreSummarisedByBand()
        {
            var rows = Shipments();
            Assert.AreEqual("haz-heavy-eu", rows[0].Band);

            var totals = ComplexitySample.SummariseBands(rows);

            Assert.AreEqual(5, totals.Count);
            Assert.AreEqual(48.0, totals["haz-heavy-eu"]);
            Assert.AreEqual(47.0, totals["parcel-heavy"]);      // 11 + 25 customs, then 11
            Assert.AreEqual(13.0, totals["express-us"]);
            Assert.AreEqual(7.5, totals["parcel"]);             // promo zeroes it, zone adds 7.5
            Assert.AreEqual(34.0, totals["freight"]);
        }

        [Test]
        public void MinimumDropsSmallRowsBeforeTheyAreSummed()
        {
            var totals = ComplexitySample.SummariseBands(Shipments(), minimum: 10.0);

            Assert.IsFalse(totals.ContainsKey("parcel"));       // 7.5 < 10
            Assert.AreEqual(4, totals.Count);
            Assert.AreEqual(47.0, totals["parcel-heavy"]);
            Assert.AreEqual(Shipments().Sum(r => r.Surcharge) - 7.5, totals.Values.Sum());
        }

        // ---- parcels -> lanes -> counts ---------------------------------------------

        [Test]
        public void ParcelsAreRoutedThenCountedPerLane()
        {
            var parcels = new List<Parcel>
            {
                new Parcel(1, 5.0, "EU", Fragile: true),
                new Parcel(2, 25.0, "EU"),
                new Parcel(3, 5.0, "US"),
                new Parcel(4, 3.0, "EU"),
                new Parcel(5, 5.0, "MARS"),                      // region not served: skipped
            };
            var regions = new[] { "EU", "US" };

            var routed = NestedPathsSample.RouteParcels(parcels, regions);
            var counts = LaneSummary.LaneCounts(parcels, regions);

            Assert.AreEqual(4, routed.Count);
            CollectionAssert.AreEqual(
                new[] { ("careful", 1), ("heavy", 2), ("standard", 3), ("standard", 4) }, routed);
            Assert.AreEqual(1, counts["careful"]);
            Assert.AreEqual(1, counts["heavy"]);
            Assert.AreEqual(2, counts["standard"]);
            Assert.AreEqual(routed.Count, counts.Values.Sum());
        }

        // ---- checkout: tax -> shipping ----------------------------------------------

        [TestCase("IN", 100, 168)]    // 118 taxed, below 500 => +50
        [TestCase("IN", 500, 590)]    // 590 taxed, at/above 500 => no shipping
        [TestCase("US", 100, 122)]    // 107 taxed, below 500 => +15
        [TestCase("US", 600, 642)]    // 642 taxed => no shipping
        [TestCase("FR", 100, 115)]    // untaxed, below 500 => +15
        [TestCase("FR", 500, 500)]    // untaxed, exactly 500 => no shipping
        public void CheckoutTakesEveryTaxAndShippingCombination(string country, int subtotal, int expected)
        {
            Assert.AreEqual((decimal)expected, PathScenarios.Checkout(subtotal, country));
        }

        // ---- invoice -> status -> credit -> tally -----------------------------------

        [Test]
        public void InvoiceTotalsFeedStatusCreditAndTally()
        {
            decimal small = DataFlowScenarios.InvoiceTotal(500m, 0.1m);       // fee 50, uncapped
            decimal large = DataFlowScenarios.InvoiceTotal(2000m, 0.1m);      // fee 200, capped at 100
            Assert.AreEqual(550m, small);
            Assert.AreEqual(2100m, large);

            var statuses = new List<string>
            {
                DataFlowScenarios.Status((int)large, (int)large),             // settled
                DataFlowScenarios.Status((int)small, (int)large),             // partial
                DataFlowScenarios.Status(0, (int)large),                      // open
                DataFlowScenarios.Status((int)large, (int)small),             // settled
            };
            var tally = DataFlowScenarios.Tally(statuses);

            Assert.AreEqual(2, tally["SETTLED"]);
            Assert.AreEqual(1, tally["PARTIAL"]);
            Assert.AreEqual(1, tally["OPEN"]);
            Assert.AreEqual("ok", DataFlowScenarios.CreditCheck(small, 1000m));
            Assert.AreEqual("near", DataFlowScenarios.CreditCheck(950m, 1000m));
        }
    }
}
