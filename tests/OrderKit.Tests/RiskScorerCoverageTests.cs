using System;
using System.Collections.Generic;
using NUnit.Framework;
using OrderKit;

namespace OrderKit.Tests
{
    /// <summary>
    /// Walks every branch of RiskScorer.Score (the planted cognitive peak) without
    /// changing it, plus its three wrappers.
    /// </summary>
    [TestFixture]
    public class RiskScorerCoverageTests
    {
        private readonly RiskScorer _scorer = new RiskScorer();

        private static Order One(string category, int quantity)
        {
            Order o = new Order("RC-" + category + quantity, null);
            o.AddLine(new OrderLine("SKU", quantity, 100L, category));
            return o;
        }

        [Test]
        public void NothingScoresZero()
        {
            Assert.AreEqual(0, _scorer.Score(null, null, null));
            Assert.AreEqual(0, _scorer.Score(new Order("RC-empty", null), "domestic", new List<string>()));
        }

        [TestCase(10, "domestic", 5)]     // == threshold is not "over"
        [TestCase(11, null, 25)]
        [TestCase(11, "domestic", 15)]
        [TestCase(11, "sanctioned", 40)]
        [TestCase(11, "embargoed", 40)]
        public void HazmatBranches(int quantity, string region, int expected)
        {
            Assert.AreEqual(expected, _scorer.Score(One("hazmat", quantity), region, null));
        }

        [TestCase(50, 0)]      // == threshold is not "over"
        [TestCase(51, 8)]
        [TestCase(1000, 8)]    // == bulk is not "over"
        [TestCase(1001, 12)]
        public void FragileBranches(int quantity, int expected)
        {
            Assert.AreEqual(expected, _scorer.Score(One("fragile", quantity), "domestic", null));
        }

        [Test]
        public void OtherCategoriesScoreNothing()
        {
            Assert.AreEqual(0, _scorer.Score(One("general", 5000), "sanctioned", null));
        }

        [Test]
        public void FlagsAreWeighedAndNullsSkipped()
        {
            Assert.AreEqual(3, _scorer.Score(null, null, new List<string> { null, "soft-a", "plain" }));
            Assert.AreEqual(23, _scorer.Score(null, null, new List<string> { "hard-a", "soft-b" }));
        }

        [Test]
        public void ScoreOrDefaultReturnsTheScore()
        {
            Order o = One("hazmat", 11);
            Assert.AreEqual(_scorer.Score(o, "sanctioned", null), _scorer.ScoreOrDefault(o, "sanctioned", null));
        }

        [Test]
        public void ScoreDomesticMasksTheSeverityBits()
        {
            Assert.AreEqual(16, _scorer.ScoreDomestic(null, new List<string> { "hard-a" }));   // 20 & ~0x0F
            Assert.AreEqual(0, _scorer.ScoreDomestic(One("hazmat", 11), null));                // 15 & ~0x0F
        }

        [Test]
        public void ScoreAnyAddsOrdersAndRawScores()
        {
            Assert.AreEqual(0, _scorer.ScoreAny(null));
            Assert.AreEqual(32, _scorer.ScoreAny(new object[] { One("hazmat", 11), 7, "ignored" }));
            Assert.AreEqual(100, _scorer.ScoreAny(new object[] { 60, 60 }));
        }
    }
}
