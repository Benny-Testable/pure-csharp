using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OrderKit;
using OrderKit.Platform;

namespace OrderKit.Tests
{
    [TestFixture]
    public class PriceableTests
    {
        private static Order Sample()
        {
            Order o = new Order("PR-1", null);
            o.AddLine(new OrderLine("SKU-1", 2, 1250L, "general"));
            return o;
        }

        [Test]
        public void CentsGuardsAndAdds()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Cents(-1L));
            Cents zero = new Cents(0L);
            Cents five = new Cents(5L);
            Assert.AreEqual(5L, zero.Plus(five).Value);
            Assert.AreEqual("5c", five.ToString());
            Assert.AreEqual(12L, PriceableHelper.TotalOf(new Cents(7L), five));
        }

        [Test]
        public void PriceKeyIsAValueRecord()
        {
            PriceKey key = new PriceKey("t", "s");
            Assert.AreEqual("t/s", key.Combined);
            Assert.AreEqual(new PriceKey("t", "s"), key);
        }

        [Test]
        public void PriceQuoteDefaultsAndRebases()
        {
            PriceQuote quote = new PriceQuote("k", 100L) { Currency = "EUR" };
            Assert.AreEqual("domestic", quote.Region);
            PriceQuote rebased = quote.Rebased(250L);
            Assert.AreEqual(250L, rebased.Cents);
            Assert.AreEqual("EUR", rebased.Currency);
            Assert.AreEqual(100L, quote.Cents);
        }

        [Test]
        public void QuoteScopeUsesItsPrimaryConstructor()
        {
            QuoteScope scope = new QuoteScope("acme", "GBP");
            Assert.AreEqual("acme", scope.Tenant);
            Assert.AreEqual("GBP", scope.Currency);
            var key = scope.KeyFor("SKU-9");
            Assert.AreEqual("acme", key.Tenant);
            Assert.AreEqual("SKU-9", key.Sku);
            PriceQuote quote = scope.Quote(Sample());
            Assert.AreEqual("order:PR-1", quote.Key);
            Assert.AreEqual(2500L, quote.Cents);
            Assert.AreEqual("GBP", quote.Currency);
        }

        [Test]
        public void HelperConstantsAndRegions()
        {
            Assert.AreEqual(8, PriceableHelper.MagicLength);
            StringAssert.Contains("\"region\": \"domestic\"", PriceableHelper.SchemaText());
            CollectionAssert.AreEqual(new[] { "domestic", "eu", "apac" }, PriceableHelper.KnownRegions());
            CollectionAssert.AreEqual(new[] { "domestic", "eu", "apac", "latam" }, PriceableHelper.KnownRegions("latam"));
        }

        [Test]
        public void HelperKeysAndDescriptions()
        {
            Order o = Sample();
            Assert.Throws<ArgumentNullException>(() => PriceableHelper.Suffixed(null, "x"));
            Assert.AreEqual("order:PR-1:x", PriceableHelper.Suffixed(o, "x"));
            Assert.AreEqual("<none>", PriceableHelper.Describe(null));
            Assert.AreEqual("order:PR-1=2500", PriceableHelper.Describe(o));
            Assert.AreEqual("orderkit://quote/order:PR-1", PriceableHelper.QualifiedKey(o));
            Assert.AreEqual("item", Assert.Throws<ArgumentNullException>(() => PriceableHelper.QualifiedKey(null)).ParamName);
            Assert.AreEqual("Priced", PriceableHelper.NameOfStatus(OrderStatus.Priced));
        }

        [Test]
        public void WeightOfEachCandidateKind()
        {
            Order o = Sample();
            Assert.AreEqual(2500L, PriceableHelper.WeightOf(o));
            Assert.AreEqual(2500L, PriceableHelper.WeightOf(o.Lines[0]));
            Assert.AreEqual(0L, PriceableHelper.WeightOf("neither"));
        }

        [Test]
        public void FirstOfTypeScansZeroOneOrMany()
        {
            OrderLine line = new OrderLine("L", 1, 1L, null);
            Assert.IsNull(PriceableHelper.FirstOfType<OrderLine>(null));
            Assert.IsNull(PriceableHelper.FirstOfType<OrderLine>(new object[] { "a", 1 }));
            Assert.AreSame(line, PriceableHelper.FirstOfType<OrderLine>(new object[] { "a", line, new OrderLine("M", 1, 1L, null) }));
        }

        [Test]
        public void SameShapeComparesSkuAndQuantity()
        {
            OrderLine a = new OrderLine("S", 2, 10L, null);
            Assert.IsTrue(PriceableHelper.SameShape(null, null));
            Assert.IsFalse(PriceableHelper.SameShape(a, null));
            Assert.IsTrue(PriceableHelper.SameShape(a, new OrderLine("S", 2, 99L, "bulk")));
            Assert.IsFalse(PriceableHelper.SameShape(a, new OrderLine("S", 3, 10L, null)));
        }

        [Test]
        public void QuoteForIsEuInUsd()
        {
            PriceQuote quote = PriceableHelper.QuoteFor(Sample());
            Assert.AreEqual("eu", quote.Region);
            Assert.AreEqual("USD", quote.Currency);
            Assert.AreEqual(2500L, quote.Cents);
        }

        [Test]
        public void IntegrationsDeclareEveryPlantedPackage()
        {
            CollectionAssert.AreEquivalent(
                new[] { "Newtonsoft.Json", "System.Net.Http", "Microsoft.AspNet.Mvc", "System.Text.RegularExpressions", "BouncyCastle" },
                Integrations.DeclaredPackages().ToList());
        }
    }
}
