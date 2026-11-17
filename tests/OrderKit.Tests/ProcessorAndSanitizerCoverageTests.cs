using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OrderKit;

namespace OrderKit.Tests
{
    /// <summary>
    /// The duplicate pair is driven through one set of checks, so the tests do not
    /// themselves become a second planted clone.
    /// </summary>
    [TestFixture]
    public class DuplicateProcessorCoverageTests
    {
        private sealed class Processor
        {
            public Action<IEnumerable<Order>, long> Process;
            public Func<int> Accepted;
            public Func<int> Rejected;
            public Func<IEnumerable<string>> AcceptedIds;
            public Action Reset;
        }

        private static IEnumerable<Processor> Both()
        {
            DuplicateProcessorA a = new DuplicateProcessorA();
            yield return new Processor { Process = a.Process, Accepted = a.AcceptedCount, Rejected = a.RejectedCount, AcceptedIds = a.AcceptedIds, Reset = a.Reset };
            DuplicateProcessorB b = new DuplicateProcessorB();
            yield return new Processor { Process = b.Process, Accepted = b.AcceptedCount, Rejected = b.RejectedCount, AcceptedIds = b.AcceptedIds, Reset = b.Reset };
        }

        [Test]
        public void ResetClearsBothLists()
        {
            foreach (Processor p in Both())
            {
                p.Process(new[] { Priced("R", 1L), new Order("E", null) }, 500L);
                p.Reset();
                Assert.AreEqual(0, p.Accepted());
                Assert.AreEqual(0, p.Rejected());
            }
        }

        private static Order Priced(string id, long cents, OrderStatus status = OrderStatus.Draft)
        {
            Order o = new Order(id, null) { Status = status };
            o.AddLine(new OrderLine("S", 1, cents, "general"));
            return o;
        }

        [Test]
        public void NullSequenceThrows()
        {
            foreach (Processor p in Both())
            {
                Assert.Throws<ArgumentNullException>(() => p.Process(null, 0L));
            }
        }

        [Test]
        public void NegativeCeilingThrowsButZeroIsAllowed()
        {
            foreach (Processor p in Both())
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => p.Process(new List<Order>(), -1L));
                p.Process(new[] { Priced("FREE", 0L) }, 0L);   // == 0 boundary is a valid ceiling
                Assert.AreEqual(1, p.Accepted());
            }
        }

        [Test]
        public void EveryRoutingOutcome()
        {
            foreach (Processor p in Both())
            {
                p.Process(new List<Order>
                {
                    null,                                          // skipped
                    new Order("E", null),                          // empty -> rejected
                    Priced("Z", 100L),                             // accepted
                    Priced("A", 500L),                             // == ceiling -> accepted
                    Priced("OVER", 501L),                          // > ceiling -> rejected
                    Priced("DONE", 10L, OrderStatus.Fulfilled)     // terminal -> rejected
                }, 500L);
                Assert.AreEqual(2, p.Accepted());
                Assert.AreEqual(3, p.Rejected());
                CollectionAssert.AreEqual(new[] { "A", "Z" }, p.AcceptedIds().ToList());
            }
        }
    }

    [TestFixture]
    public class InputSanitizerCoverageTests
    {
        [Test]
        public void SanitizeHandlesNullAndTrims()
        {
            Assert.AreEqual("", InputSanitizer.Sanitize(null));
            Assert.AreEqual("a b", InputSanitizer.Sanitize("  a;| b  "));
        }

        [Test]
        public void LooksSuspiciousOnEmptyInput()
        {
            Assert.IsFalse(InputSanitizer.LooksSuspicious(null));
            Assert.IsFalse(InputSanitizer.LooksSuspicious(""));
        }

        [Test]
        public void FlowsBuildTheExpectedStrings()
        {
            Assert.AreEqual("tenant//sku/", InputSanitizer.BuildLookupKey(null, null));
            Assert.AreEqual("SELECT * FROM t WHERE p", InputSanitizer.BuildQuery("t", "p"));
            Assert.AreEqual("/tmp/r.csv", InputSanitizer.BuildReportPath("/tmp/", "r.csv"));
            Assert.Throws<ArgumentNullException>(() => InputSanitizer.BuildReportPath(null, "r.csv"));
        }

        [Test]
        public void BuildCommandWithZeroOneOrManyArgs()
        {
            Assert.AreEqual("ls", InputSanitizer.BuildCommand("ls", null));
            Assert.AreEqual("ls", InputSanitizer.BuildCommand("ls", new string[0]));
            Assert.AreEqual("ls -l", InputSanitizer.BuildCommand("ls", new[] { "-l" }));
            Assert.AreEqual("ls -l -a", InputSanitizer.BuildCommand("ls", new[] { "-l", "-a" }));
        }

        [Test]
        public void SafeLengthOfNullIsZero()
        {
            Assert.AreEqual(0, InputSanitizer.SafeLength(null));
            Assert.AreEqual(3, InputSanitizer.SafeLength("abc"));
        }
    }
}
