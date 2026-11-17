using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OrderKit;

namespace OrderKit.Tests
{
    [TestFixture]
    public class OrderCoverageTests
    {
        private static Order Lines(int count)
        {
            Order o = new Order("OC-1", "gold");
            for (int i = 0; i < count; i++)
            {
                o.AddLine(new OrderLine("SKU-" + i, i + 1, 50L, "general"));
            }
            return o;
        }

        [Test]
        public void ConstructorGuardsAndDefaults()
        {
            Assert.Throws<ArgumentNullException>(() => new Order(null, "gold"));
            Order o = new Order("OC-2", null);
            Assert.AreEqual("standard", o.CustomerTier);
            o.CustomerTier = null;
            Assert.AreEqual("standard", o.CustomerTier);
            Assert.AreEqual(OrderStatus.Draft, o.Status);
        }

        [Test]
        public void AddLineRejectsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Order("OC-3", null).AddLine(null));
        }

        [Test]
        public void EmptyOrderAccessors()
        {
            Order o = Lines(0);
            Assert.AreEqual(0, o.LineCount());
            Assert.IsNull(o.FirstSku());
            Assert.IsNull(o.LastLine());
            Assert.AreEqual(0, o.FirstTwo().Count);
            Assert.AreEqual(0L, o.SubtotalCents());
        }

        [TestCase(1, 1)]
        [TestCase(2, 2)]   // == 2 boundary keeps every line
        [TestCase(3, 2)]
        public void FirstTwoTakesAtMostTwo(int lines, int expected)
        {
            Assert.AreEqual(expected, Lines(lines).FirstTwo().Count);
        }

        [Test]
        public void LineAccessorsOnAPopulatedOrder()
        {
            Order o = Lines(3);
            Assert.AreEqual("SKU-0", o.FirstSku());
            Assert.AreEqual("SKU-2", o.LastLine().Sku);
            Assert.AreEqual(3, o.Lines.Count);
            Assert.AreEqual(3, o.Snapshot().Count);
            Assert.AreEqual(300L, o.SubtotalCents());   // (1 + 2 + 3) x 50
        }

        [Test]
        public void KeysAndDescriptions()
        {
            Order o = Lines(2);
            Assert.AreEqual("order:OC-1", o.CachedKey());
            Assert.AreSame(o.CachedKey(), o.CachedKey());
            Assert.AreEqual("order:OC-1", o.PricingKey());
            Assert.AreEqual(150L, o.BaseCents());
            Assert.AreEqual("OC-1: 2 lines, 150c, draft", o.Describe());
            Assert.AreEqual("Order(OC-1, 2 lines, draft)", o.ToString());
        }

        [Test]
        public void TagsSpliceTheExtras()
        {
            Order o = Lines(1);
            CollectionAssert.AreEqual(new[] { "gold", "draft" }, o.Tags());
            CollectionAssert.AreEqual(new[] { "gold", "draft", "x", "y" }, o.Tags("x", "y"));
        }

        [Test]
        public void RoundedSubtotalRoundsDownToWholeUnits()
        {
            Order o = new Order("OC-4", null);
            o.AddLine(new OrderLine("SKU", 1, 2599L, "general"));
            Assert.AreEqual(2500L, o.RoundedSubtotal());
        }

        [Test]
        public void SuffixedUsesTheDefaultInterfaceMethod()
        {
            IPriceable item = Lines(1);
            Assert.AreEqual("order:OC-1", item.Suffixed(""));
            Assert.AreEqual("order:OC-1", item.Suffixed(null));
            Assert.AreEqual("order:OC-1:eu", item.Suffixed("eu"));
        }
    }

    [TestFixture]
    public class OrderLineTests
    {
        [Test]
        public void ConstructorGuards()
        {
            Assert.Throws<ArgumentNullException>(() => new OrderLine(null, 1, 1L, "general"));
            Assert.Throws<ArgumentOutOfRangeException>(() => new OrderLine("S", -1, 1L, "general"));
            Assert.Throws<ArgumentOutOfRangeException>(() => new OrderLine("S", 1, -1L, "general"));
        }

        [Test]
        public void ZeroIsAValidBoundary()
        {
            OrderLine line = new OrderLine("S", 0, 0L, null);
            Assert.AreEqual(0L, line.ExtendedCents());
            Assert.AreEqual("general", line.Category);
        }

        [Test]
        public void DeconstructAndKeys()
        {
            OrderLine line = new OrderLine("S", 2, 50L, "bulk");
            var (sku, quantity, unitCents) = line;
            Assert.AreEqual("S", sku);
            Assert.AreEqual(2, quantity);
            Assert.AreEqual(50L, unitCents);
            Assert.AreEqual(100L, line.ExtendedCents());
            Assert.AreEqual("S#2", line.IdentityKey());
            Assert.AreEqual("S x2 @ 50", line.ToString());
        }

        [Test]
        public void EmptyLineIsBlank()
        {
            OrderLine line = OrderLine.Empty();
            Assert.AreEqual("", line.Sku);
            Assert.AreEqual(0, line.Quantity);
            Assert.AreEqual("general", line.Category);
        }
    }
}
