using System;
using NUnit.Framework;
using OrderKit;

namespace OrderKit.Tests
{
    /// <summary>
    /// The bulk-discount feature: Order.UnitsInCategory, Order.CategorySubtotalCents,
    /// BulkRule.IsValid, PricingService.BulkDiscountCents and PriceWithBulkDiscount.
    /// Every new line is executed and both outcomes of every new branch are taken.
    /// </summary>
    [TestFixture]
    public class BulkDiscountTests
    {
        private readonly PricingService _svc = new PricingService();

        // bulk: 60 x 100c + 50 x 200c = 110 units, 16 000c.  general: 5 x 1000c = 5 000c.
        private static Order MixedOrder()
        {
            Order o = new Order("B-1", "standard");
            o.AddLine(new OrderLine("BOLT", 60, 100L, "bulk"));
            o.AddLine(new OrderLine("NUT", 50, 200L, "bulk"));
            o.AddLine(new OrderLine("TOOL", 5, 1000L, "general"));
            return o;
        }

        [Test]
        public void UnitsInCategorySumsOnlyThatCategory()
        {
            Order o = MixedOrder();
            Assert.AreEqual(110, o.UnitsInCategory("bulk"));
            Assert.AreEqual(5, o.UnitsInCategory("general"));
        }

        [Test]
        public void UnitsInCategoryIsZeroWhenNoLineMatches()
        {
            Assert.AreEqual(0, MixedOrder().UnitsInCategory("hazmat"));
            Assert.AreEqual(0, new Order("B-0", null).UnitsInCategory("bulk"));
        }

        [Test]
        public void CategorySubtotalSumsExtendedCentsOfThatCategory()
        {
            Order o = MixedOrder();
            Assert.AreEqual(16_000L, o.CategorySubtotalCents("bulk"));
            Assert.AreEqual(5_000L, o.CategorySubtotalCents("general"));
            Assert.AreEqual(0L, o.CategorySubtotalCents("hazmat"));
        }

        [Test]
        public void CategorySubtotalsPartitionTheOrderSubtotal()
        {
            Order o = MixedOrder();
            Assert.AreEqual(o.SubtotalCents(),
                            o.CategorySubtotalCents("bulk") + o.CategorySubtotalCents("general"));
        }

        [TestCase("bulk", 100, 10, true)]
        [TestCase("bulk", 1, 0, true)]               // lowest valid threshold and percent
        [TestCase("bulk", 100, 30, true)]            // percent cap is inclusive
        [TestCase(null, 100, 10, false)]
        [TestCase("", 100, 10, false)]
        [TestCase("bulk", 0, 10, false)]
        [TestCase("bulk", -5, 10, false)]
        [TestCase("bulk", 100, -1, false)]
        [TestCase("bulk", 100, 31, false)]
        public void BulkRuleValidity(string category, int threshold, int percent, bool valid)
        {
            Assert.AreEqual(valid, new BulkRule(category, threshold, percent).IsValid);
        }

        [Test]
        public void DiscountIsPercentOfCategorySubtotalOnceThresholdIsMet()
        {
            Assert.AreEqual(1_600L, _svc.BulkDiscountCents(MixedOrder(), new BulkRule("bulk", 100, 10)));
            Assert.AreEqual(4_800L, _svc.BulkDiscountCents(MixedOrder(), new BulkRule("bulk", 100, 30)));
            Assert.AreEqual(0L, _svc.BulkDiscountCents(MixedOrder(), new BulkRule("bulk", 100, 0)));
        }

        [Test]
        public void ThresholdBoundaryIsInclusive()
        {
            // 110 units on the order: exactly 110 qualifies, 111 does not.
            Assert.AreEqual(1_600L, _svc.BulkDiscountCents(MixedOrder(), new BulkRule("bulk", 110, 10)));
            Assert.AreEqual(0L, _svc.BulkDiscountCents(MixedOrder(), new BulkRule("bulk", 111, 10)));
        }

        [Test]
        public void NoDiscountForACategoryTheOrderLacks()
        {
            Assert.AreEqual(0L, _svc.BulkDiscountCents(MixedOrder(), new BulkRule("hazmat", 1, 10)));
        }

        [Test]
        public void DiscountRejectsNullOrderNullRuleAndInvalidRule()
        {
            Assert.Throws<ArgumentNullException>(
                () => _svc.BulkDiscountCents(null, new BulkRule("bulk", 100, 10)));
            Assert.Throws<ArgumentNullException>(
                () => _svc.BulkDiscountCents(MixedOrder(), null));
            Assert.Throws<ArgumentException>(
                () => _svc.BulkDiscountCents(MixedOrder(), new BulkRule("bulk", 100, 31)));
        }

        [Test]
        public void DiscountIsTakenOffThePriceWhenTheThresholdIsMet()
        {
            // 21 000 subtotal + 250 domestic = 21 250 (no tier, expedite or loyalty adjustment).
            Assert.AreEqual(21_250L, _svc.PriceCents(MixedOrder(), "domestic", false, 0));
            Assert.AreEqual(19_650L, _svc.PriceWithBulkDiscount(
                MixedOrder(), "domestic", false, 0, new BulkRule("bulk", 100, 10)));
        }

        [Test]
        public void PriceIsUnchangedWhenTheThresholdIsNotMet()
        {
            Assert.AreEqual(21_250L, _svc.PriceWithBulkDiscount(
                MixedOrder(), "domestic", false, 0, new BulkRule("bulk", 500, 10)));
        }

        [Test]
        public void PriceNeverGoesBelowZero()
        {
            // One bulk line of 1 000c, domestic +250, loyalty -1 000 => price 250.
            Order o = new Order("B-2", "standard");
            o.AddLine(new OrderLine("BOLT", 100, 10L, "bulk"));
            Assert.AreEqual(250L, _svc.PriceCents(o, "domestic", false, 10));

            // A 30% discount is 300c, more than the price: floored at zero.
            Assert.AreEqual(0L, _svc.PriceWithBulkDiscount(o, "domestic", false, 10, new BulkRule("bulk", 100, 30)));
            // A 25% discount is exactly the price: still zero, not negative.
            Assert.AreEqual(0L, _svc.PriceWithBulkDiscount(o, "domestic", false, 10, new BulkRule("bulk", 100, 25)));
            // A 20% discount is 200c, less than the price.
            Assert.AreEqual(50L, _svc.PriceWithBulkDiscount(o, "domestic", false, 10, new BulkRule("bulk", 100, 20)));
        }

        [Test]
        public void CancelledAndEmptyOrdersStayAtZero()
        {
            Order cancelled = MixedOrder();
            cancelled.Status = OrderStatus.Cancelled;
            Assert.AreEqual(0L, _svc.PriceWithBulkDiscount(
                cancelled, "domestic", false, 0, new BulkRule("bulk", 100, 10)));
            Assert.AreEqual(0L, _svc.PriceWithBulkDiscount(
                new Order("B-3", null), "domestic", false, 0, new BulkRule("bulk", 100, 10)));
        }
    }
}
