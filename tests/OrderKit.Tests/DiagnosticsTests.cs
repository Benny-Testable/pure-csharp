using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OrderKit;

namespace OrderKit.Tests
{
    [TestFixture]
    public class DiagnosticsTests
    {
        [Test]
        public void NoteRecordsTheMember()
        {
            Diagnostics d = new Diagnostics();
            d.Note("m", "Explicit");
            d.Note("n", null);
            CollectionAssert.AreEqual(new[] { "[Explicit] m", "[?] n" }, d.Entries().ToList());
        }

        [Test]
        public void NoteFieldNamesItself()
        {
            Diagnostics d = new Diagnostics();
            d.NoteField(5);
            Assert.AreEqual("Diagnostics.NoteField=5", d.Entries().Single());
        }

        [TestCase(-1)]
        [TestCase(4)]   // == length is out of range
        public void CounterAtRejectsOutOfRange(int index)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Diagnostics().CounterAt(index));
        }

        [TestCase(-1)]
        [TestCase(4)]
        public void CounterValueRejectsOutOfRange(int index)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Diagnostics().CounterValue(index));
        }

        [Test]
        public void BumpsMoveTheRightCounters()
        {
            Diagnostics d = new Diagnostics();
            Assert.AreEqual("0/0", d.Totals());
            d.Bump(3);                 // last valid index
            d.Bump(3);
            d.BumpEither(true);
            d.BumpEither(false);
            Assert.AreEqual(1, d.CounterValue(0));
            Assert.AreEqual(1, d.CounterValue(1));
            Assert.AreEqual(0, d.CounterValue(2));
            Assert.AreEqual(2, d.CounterValue(3));
            Assert.AreEqual("4/3", d.Totals());
            d.BumpAll();
            Assert.AreEqual("8/4", d.Totals());
        }

        [Test]
        public void DumpWritesZeroOneOrManyEntries()
        {
            Diagnostics d = new Diagnostics();
            Assert.AreEqual("", d.Dump());
            d.Note("a", "X");
            Assert.AreEqual("[X] a" + Environment.NewLine, d.Dump());
            d.Note("b", "X");
            d.Note("c", "Y");
            Assert.AreEqual(3, d.Dump().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Length);
        }

        [Test]
        public void TryCountBothOutcomes()
        {
            Diagnostics d = new Diagnostics();
            Assert.IsFalse(d.TryCount("[X]", out int none));
            Assert.AreEqual(0, none);
            d.Note("a", "X");
            d.Note("b", "X");
            d.Note("c", "Y");
            Assert.IsTrue(d.TryCount("[X]", out int two));
            Assert.AreEqual(2, two);
        }
    }
}
