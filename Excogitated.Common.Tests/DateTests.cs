using Excogitated.Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Excogitated.Common.Tests
{
    [TestClass]
    public class DateTests : TestsBase
    {
        [TestMethod]
        public void ToInt()
        {
            var chars = Enumerable.Range(0, 10).Select(i => new { Digit = i, Char = i.ToString()[0] }).ToList();
            foreach (var c in chars)
            {
                Assert.AreEqual(c.Digit, c.Char.ToInt());
                Console.WriteLine(c);
            }
        }

        [TestMethod]
        public void Parse()
        {
            Assert.IsTrue(Date.TryParse(Date.Today, out var date));
            Assert.AreEqual(Date.Today, date);
        }

        [TestMethod]
        public void ToChar()
        {
            Assert.AreEqual('0', 0.ToChar());
            Assert.AreEqual('1', 1.ToChar());
            Assert.AreEqual('2', 2.ToChar());
            Assert.AreEqual('3', 3.ToChar());
            Assert.AreEqual('4', 4.ToChar());
            Assert.AreEqual('5', 5.ToChar());
            Assert.AreEqual('6', 6.ToChar());
            Assert.AreEqual('7', 7.ToChar());
            Assert.AreEqual('8', 8.ToChar());
            Assert.AreEqual('9', 9.ToChar());
        }

        [TestMethod]
        public void ShortYear()
        {
            var expected = DateTime.Today.AddYears(-2000).ToString("yyyy-MM-dd");
            var actual = Date.Today.AddYears(-2000).ToString();
            Console.WriteLine(new { expected, actual }.ToString());
            Assert.AreEqual(expected, actual);

            expected = DateTime.Today.AddYears(-2000).ToString("MM-dd-yyyy");
            actual = new MonthDayYear(Date.Today.AddYears(-2000)).ToString();
            Console.WriteLine(new { expected, actual }.ToString());
            Assert.AreEqual(expected, actual);
        }
    }
}
