using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using OrderKit.Analysis;

namespace OrderKit.Tests
{
    /// <summary>
    /// RouteParcels: takes the careful, heavy and standard lanes, and deliberately
    /// NOT the express-fragile or freight ones, so the nested-path measure scores
    /// strictly between 0 and 100.
    /// </summary>
    [TestFixture]
    public class NestedPathsSampleTests
    {
        [Test]
        public void FragileParcelTakesTheCarefulLane()
        {
            CollectionAssert.AreEqual(new[] { ("careful", 1) },
                NestedPathsSample.RouteParcels(new[] { new Parcel(1, 3, "east", Fragile: true) }, new[] { "east" }));
        }

        [Test]
        public void HeavyParcelWithoutOversizeTakesTheHeavyLane()
        {
            CollectionAssert.AreEqual(new[] { ("heavy", 2) },
                NestedPathsSample.RouteParcels(new[] { new Parcel(2, 30, "east") }, new[] { "east" }));
        }

        [Test]
        public void LightParcelTakesTheStandardLane()
        {
            CollectionAssert.AreEqual(new[] { ("standard", 3) },
                NestedPathsSample.RouteParcels(new[] { new Parcel(3, 5, "east") }, new[] { "east" }));
        }

        [Test]
        public void ParcelsWithNoWeightOrOtherRegionAreSkipped()
        {
            Assert.IsEmpty(NestedPathsSample.RouteParcels(
                new[] { new Parcel(4, 0, "east"), new Parcel(5, 3, "west") }, new[] { "east" }));
        }

        [Test]
        public void LaneCountsTotalsEachLane()
        {
            var parcels = new[] { new Parcel(1, 3, "east", Fragile: true), new Parcel(2, 5, "east"), new Parcel(3, 6, "east") };
            var counts = LaneSummary.LaneCounts(parcels, new[] { "east" });
            Assert.AreEqual(1, counts["careful"]);
            Assert.AreEqual(2, counts["standard"]);
        }
    }

    [TestFixture]
    public class ComplexitySampleTests
    {
        // customs value 2000 outside the US adds 25 to every case
        [TestCase("EU", 40, "haz-heavy-eu", 73.0)]
        [TestCase("EU", 10, "haz-mid-eu", 51.0)]
        [TestCase("EU", 5, "haz-light-eu", 39.0)]    // == 5 is not "over"
        [TestCase("JP", 5, "haz-intl", 87.0)]
        public void HazardousBands(string region, double weight, string band, double surcharge)
        {
            Assert.AreEqual((band, surcharge),
                ComplexitySample.ClassifyShipment(region, "std", weight, true, false, 2000, 1, null));
        }

        [TestCase(7, "haz-remote-us", 62.5)]          // zone 7: remote band, +7.5 for zone > 6
        [TestCase(6, "haz-heavy-us", 39.0)]           // zone 6 is not > 6
        public void HazardousUsBands(int zone, string band, double surcharge)
        {
            Assert.AreEqual((band, surcharge), ComplexitySample.ClassifyShipment("US", "std", 25, true, false, 0, zone, null));
        }

        [Test]
        public void HazardousUsLightIsStandard()
        {
            Assert.AreEqual(("haz-std-us", 21.0), ComplexitySample.ClassifyShipment("US", "std", 20, true, false, 0, 1, null));
        }

        [TestCase("gold", 9.0)]
        [TestCase("silver", 13.0)]
        [TestCase("bronze", 18.0)]
        public void ExpressTiers(string tier, double surcharge)
        {
            Assert.AreEqual(("express-us", surcharge), ComplexitySample.ClassifyShipment("US", tier, 1, false, true, 0, 1, null));
        }

        [Test]
        public void ExpressWithoutRegion()
        {
            Assert.AreEqual(("express-xx", 18.0), ComplexitySample.ClassifyShipment(null, "x", 1, false, true, 0, 1, null));
        }

        [TestCase(51, "freight", 34.0)]
        [TestCase(50, "parcel-heavy", 11.0)]          // == 50 is not "over"
        [TestCase(10, "parcel", 4.5)]                 // == 10 is not "over"
        public void GroundBands(double weight, string band, double surcharge)
        {
            Assert.AreEqual((band, surcharge), ComplexitySample.ClassifyShipment("US", "x", weight, false, false, 0, 1, null));
        }

        [Test]
        public void InvalidWeightPromoAndZoneRules()
        {
            Assert.AreEqual(("invalid", 0.0), ComplexitySample.ClassifyShipment("US", "x", 0, false, false, 0, 1, null));
            Assert.AreEqual(("parcel", 0.0), ComplexitySample.ClassifyShipment("US", "x", 1, false, false, 0, 1, "freeship"));
            Assert.AreEqual(("haz-intl", 62.0), ComplexitySample.ClassifyShipment("JP", "x", 1, true, false, 0, 1, "FREESHIP"));
            Assert.AreEqual(("freight", 34.0), ComplexitySample.ClassifyShipment("US", "x", 60, false, false, 0, 9, null));
        }

        [Test]
        public void SummariseBandsHonoursTheMinimum()
        {
            var rows = new[] { ("a", 1.0), ("a", 2.5), ("b", 0.5) };
            var all = ComplexitySample.SummariseBands(rows);
            Assert.AreEqual(3.5, all["a"]);
            Assert.AreEqual(0.5, all["b"]);
            var filtered = ComplexitySample.SummariseBands(rows, minimum: 1.0);
            Assert.AreEqual(3.5, filtered["a"]);
            Assert.IsFalse(filtered.ContainsKey("b"));
        }
    }

    [TestFixture]
    public class CallGraphAndLintFixtureTests
    {
        [Test]
        public void ChainAndCycleCompute()
        {
            Assert.AreEqual(4, CallGraphSample.StageThree(1));
            Assert.AreEqual(3, CallGraphSample.EntryPoint());     // (4 - 3) + 2
            Assert.AreEqual("ping", CallGraphSample.Ping(0));
            Assert.AreEqual("pong", CallGraphSample.Ping(1));
            Assert.AreEqual("ping", CallGraphSample.Pong(1));
        }

        [Test]
        public void LintFixtureStillBehaves()
        {
            Assert.AreEqual(6, LintViolations.BadlyNamedMethod(3, new List<int> { 1, 2 }));
            Assert.IsNull(LintViolations.SwallowEverything(Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".missing")));
            Assert.AreEqual("none", LintViolations.CompareBooleans(null));
            Assert.AreEqual("true", LintViolations.CompareBooleans(true));
            Assert.AreEqual("false", LintViolations.CompareBooleans(false));
            Assert.AreEqual("abc", LintViolations.Concatenate(new[] { "a", "b", "c" }));
        }

        [Test]
        public void SwallowEverythingReadsAnExistingFile()
        {
            string path = Path.GetTempFileName();
            try
            {
                File.WriteAllText(path, "x");
                Assert.AreEqual("x", LintViolations.SwallowEverything(path));
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
