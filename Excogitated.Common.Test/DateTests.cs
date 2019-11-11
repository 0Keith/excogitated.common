using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Excogitated.Common.Test
{
    [TestClass]
    public class DateTests
    {
        [TestMethod]
        public void ToIntTest()
        {
            var chars = Enumerable.Range(0, 10).Select(i => new { Digit = i, Char = i.ToString()[0] }).ToList();
            foreach (var c in chars)
            {
                Assert.AreEqual(c.Digit, c.Char.ToInt());
                Console.WriteLine(c);
            }
        }

        [TestMethod]
        public void ParseTest()
        {
            Assert.IsTrue(Date.TryParse(Date.Today, out var date));
            Assert.AreEqual(Date.Today, date);
        }
    }
}
