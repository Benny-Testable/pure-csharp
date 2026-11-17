using System;
using System.Collections.Generic;
using NUnit.Framework;
using OrderKit;

namespace OrderKit.Tests
{
    [TestFixture]
    public class OrderStatusTests
    {
        [TestCase(OrderStatus.Draft, false)]
        [TestCase(OrderStatus.Submitted, false)]
        [TestCase(OrderStatus.Priced, false)]
        [TestCase(OrderStatus.Fulfilled, true)]
        [TestCase(OrderStatus.Cancelled, true)]
        [TestCase(OrderStatus.Rejected, true)]
        public void TerminalMask(OrderStatus status, bool terminal)
        {
            Assert.AreEqual(terminal, status.IsTerminal());
        }

        [TestCase(OrderStatus.Draft, "draft")]
        [TestCase(OrderStatus.Submitted, "submitted")]
        [TestCase(OrderStatus.Priced, "priced")]
        [TestCase(OrderStatus.Fulfilled, "fulfilled")]
        [TestCase(OrderStatus.Cancelled, "cancelled")]
        [TestCase(OrderStatus.Rejected, "rejected")]
        [TestCase((OrderStatus)99, "unknown")]
        public void Labels(OrderStatus status, string label)
        {
            Assert.AreEqual(label, status.Label());
        }

        [Test]
        public void ClassifyEveryShape()
        {
            Assert.AreEqual("none", OrderStatusExtensions.Classify(null));
            Assert.AreEqual("terminal:cancelled", OrderStatusExtensions.Classify(OrderStatus.Cancelled));
            Assert.AreEqual("open:draft", OrderStatusExtensions.Classify(OrderStatus.Draft));
            Assert.AreEqual("empty", OrderStatusExtensions.Classify(""));
            Assert.AreEqual("text:x", OrderStatusExtensions.Classify("x"));
            Assert.AreEqual("other", OrderStatusExtensions.Classify(5));
        }

        [Test]
        public void ShapeEveryPattern()
        {
            Assert.AreEqual("none", OrderStatusExtensions.Shape(null));
            Assert.AreEqual("dead", OrderStatusExtensions.Shape(new Order("S-1", "gold") { Status = OrderStatus.Cancelled }));
            Assert.AreEqual("anonymous-draft", OrderStatusExtensions.Shape(new Order("S-2", "")));
            Assert.AreEqual("long-tier", OrderStatusExtensions.Shape(new Order("S-3", "thirteen-char")));   // 13 > 12
            Assert.AreEqual("ordinary", OrderStatusExtensions.Shape(new Order("S-4", "twelve-chars")));     // 12 is not > 12
            Assert.AreEqual("priority", OrderStatusExtensions.Shape(new Order("S-5", "platinum")));
            Assert.AreEqual("ordinary", OrderStatusExtensions.Shape(new Order("S-6", "") { Status = OrderStatus.Submitted }));
        }

        [Test]
        public void RhythmByListShape()
        {
            Assert.AreEqual("empty", OrderStatusExtensions.Rhythm(new long[0]));
            Assert.AreEqual("single:5", OrderStatusExtensions.Rhythm(new long[] { 5 }));
            Assert.AreEqual("pair:3", OrderStatusExtensions.Rhythm(new long[] { 1, 2 }));
            Assert.AreEqual("span:1..9", OrderStatusExtensions.Rhythm(new long[] { 1, 5, 9 }));
        }

        [TestCase(-1L, "invalid")]
        [TestCase(0L, "free")]
        [TestCase(1L, "small")]
        [TestCase(999L, "small")]
        [TestCase(1_000L, "standard")]   // == lower bound of standard
        [TestCase(99_999L, "standard")]
        [TestCase(100_000L, "large")]    // == upper bound is exclusive
        public void BandBoundaries(long cents, string band)
        {
            Assert.AreEqual(band, OrderStatusExtensions.Band(cents));
        }

        [Test]
        public void OpenishExcludesCancelledAndRejected()
        {
            Assert.IsTrue(OrderStatusExtensions.IsOpenish(OrderStatus.Draft));
            Assert.IsFalse(OrderStatusExtensions.IsOpenish(OrderStatus.Cancelled));
            Assert.IsFalse(OrderStatusExtensions.IsOpenish(OrderStatus.Rejected));
        }

        [Test]
        public void TryParseLabelBothOutcomes()
        {
            Assert.IsTrue(OrderStatusExtensions.TryParseLabel("priced", out OrderStatus parsed));
            Assert.AreEqual(OrderStatus.Priced, parsed);
            Assert.IsFalse(OrderStatusExtensions.TryParseLabel("bogus", out OrderStatus fallback));
            Assert.AreEqual(OrderStatus.Draft, fallback);
        }
    }
}
