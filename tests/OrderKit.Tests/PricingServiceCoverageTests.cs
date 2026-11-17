using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OrderKit;

namespace OrderKit.Tests
{
    /// <summary>
    /// Exercises every decision in PricingService.PriceCents (the planted cyclomatic
    /// peak) without changing it: each tier, each region, the expedited line-count
    /// bands, the loyalty bands at their boundaries, and the category floors.
    /// </summary>
    [TestFixture]
    public class PricingServiceCoverageTests
    {
        private readonly PricingService _svc = new PricingService();

        private static Order WithLines(string tier, int count, long unitCents, string category)
        {
            Order o = new Order("PC-" + count, tier);
            for (int i = 0; i < count; i++)
            {
                o.AddLine(new OrderLine("SKU-" + i, 1, unitCents, category));
            }
            return o;
        }

        private static Order Order10k(string tier = "standard")
        {
            Order o = new Order("PC-10k", tier);
            o.AddLine(new OrderLine("SKU-1", 10, 1000L, "general"));
            return o;
        }

        [Test]
        public void NullOrderThrows()
        {
            Assert.Throws<ArgumentNullException>(() => _svc.PriceCents(null, "domestic", false, 0));
        }

        [TestCase("platinum", 9250L)]
        [TestCase("gold", 9750L)]
        [TestCase("silver", 10050L)]
        [TestCase("standard", 10250L)]
        public void TierDiscounts(string tier, long expected)
        {
            Assert.AreEqual(expected, _svc.PriceCents(Order10k(tier), "domestic", false, 0));
        }

        [TestCase(null, 10500L)]
        [TestCase("", 10500L)]
        [TestCase("domestic", 10250L)]
        [TestCase("eu", 11200L)]
        [TestCase("apac", 11800L)]
        [TestCase("moon", 12400L)]
        public void RegionSurcharges(string region, long expected)
        {
            Assert.AreEqual(expected, _svc.PriceCents(Order10k(), region, false, 0));
        }

        [TestCase("eu", 200700L)]
        [TestCase("apac", 201100L)]
        public void LargeOrdersGetARegionalRebate(string region, long expected)
        {
            Order big = new Order("PC-big", null);
            big.AddLine(new OrderLine("SKU-1", 200, 1000L, "general"));
            Assert.AreEqual(expected, _svc.PriceCents(big, region, false, 0));
        }

        [TestCase(1, 1850L)]    // <= 10 lines: +1500
        [TestCase(10, 2750L)]   // == 10 is still the lowest band
        [TestCase(11, 3850L)]   // > 10: +2500
        [TestCase(20, 4750L)]   // == 20 is still the middle band
        [TestCase(21, 6350L)]   // > 20: +4000
        public void ExpeditedBandsByLineCount(int lines, long expected)
        {
            Assert.AreEqual(expected, _svc.PriceCents(WithLines(null, lines, 100L, "general"), "domestic", true, 0));
        }

        [TestCase(0, 10250L)]
        [TestCase(1, 10250L)]
        [TestCase(2, 10050L)]   // >= 2 boundary
        [TestCase(4, 10050L)]
        [TestCase(5, 9750L)]    // >= 5 boundary
        [TestCase(9, 9750L)]
        [TestCase(10, 9250L)]   // >= 10 boundary
        public void LoyaltyBands(int years, long expected)
        {
            Assert.AreEqual(expected, _svc.PriceCents(Order10k(), "domestic", false, years));
        }

        [Test]
        public void HazmatBelowFloorIsToppedUp()
        {
            Assert.AreEqual(2750L, _svc.PriceCents(WithLines(null, 1, 100L, "hazmat"), "domestic", false, 0));
        }

        [Test]
        public void UnknownCategoryPaysAFlatShortfall()
        {
            Assert.AreEqual(400L, _svc.PriceCents(WithLines(null, 1, 100L, "toys"), "domestic", false, 0));
        }

        [Test]
        public void FragileExactlyAtFloorPaysNoShortfall()
        {
            Assert.AreEqual(1150L, _svc.PriceCents(WithLines(null, 1, 900L, "fragile"), "domestic", false, 0));
        }

        [Test]
        public void FragileOneCentBelowFloorPaysOneCent()
        {
            Assert.AreEqual(1150L, _svc.PriceCents(WithLines(null, 1, 899L, "fragile"), "domestic", false, 0));
        }

        [Test]
        public void NegativeTotalIsClampedToZero()
        {
            Order o = new Order("PC-neg", "platinum");
            o.AddLine(new OrderLine("SKU-1", 1, 1L, "general"));
            Assert.AreEqual(0L, _svc.PriceCents(o, "domestic", false, 20));
        }

        [Test]
        public void HelpersComputeTheirValues()
        {
            Assert.AreEqual(5L, PricingService.Combine(2L, 3L));
            Assert.AreEqual(0x34, _svc.AuditCode(0x1234));
            Assert.AreEqual(0L, _svc.FloorFor(null));
            Assert.AreEqual(100L, _svc.FloorFor("bulk"));
        }
    }

    [TestFixture]
    public class AsyncPricingGatewayTests
    {
        private readonly PricingService _svc = new PricingService();

        private static Order Line(string id, long unitCents)
        {
            Order o = new Order(id, null);
            o.AddLine(new OrderLine("SKU", 1, unitCents, "general"));
            return o;
        }

        [Test]
        public void NullPricingServiceThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new AsyncPricingGateway(null));
        }

        [Test]
        public void NullBatchThrowsArgumentNull()
        {
            AsyncPricingGateway gw = new AsyncPricingGateway(_svc);
            ArgumentNullException ex = Assert.ThrowsAsync<ArgumentNullException>(() => gw.PriceBatchAsync(null, "domestic"));
            Assert.AreEqual("orders", ex.ParamName);
        }

        [Test]
        public void BatchOfZeroOrdersIsZero()
        {
            AsyncPricingGateway gw = new AsyncPricingGateway(_svc);
            Assert.AreEqual(0L, gw.PriceBatchAsync(new List<Order>(), "domestic").Result);
        }

        [Test]
        public void BatchOfOneOrderMatchesTheSyncPrice()
        {
            AsyncPricingGateway gw = new AsyncPricingGateway(_svc);
            Order o = Line("G-1", 1000L);
            Assert.AreEqual(_svc.PriceCents(o, "eu", false, 0), gw.PriceBatchAsync(new[] { o }, "eu").Result);
        }

        [Test]
        public void BatchOfManyOrdersIsTheSum()
        {
            AsyncPricingGateway gw = new AsyncPricingGateway(_svc);
            Order[] orders = { Line("G-1", 1000L), Line("G-2", 2000L), Line("G-3", 3000L) };
            long expected = orders.Sum(o => _svc.PriceCents(o, "domestic", false, 0));
            Assert.AreEqual(expected, gw.PriceBatchAsync(orders, "domestic").Result);
        }
    }
}
