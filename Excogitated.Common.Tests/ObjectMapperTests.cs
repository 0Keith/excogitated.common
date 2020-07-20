using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Excogitated.Common.Tests
{
    [TestClass]
    public class ObjectMapperTests : TestsBase
    {
        [TestMethod]
        public void CopyToTest()
        {
            ObjectMapper.Map<long, int>(i => i.ToInt());
            ObjectMapper.Map<int, long>(i => i);
            var source = new CopyItemSource
            {
                Id = Guid.NewGuid(),
                Version = Rng.GetInt32(),
                Name = Rng.GetDecimal().ToString(),
                Created = DateTimeOffset.Now,
                Count = Rng.GetInt32(),
            };
            var target = new CopyItemTarget();
            source.CopyTo(target);
            Assert.AreEqual(0, target.Id);
            Assert.AreEqual(source.Version, target.Version);
            Assert.AreEqual(source.Name, target.Name);
            Assert.AreEqual(source.Created, target.Created);
            Assert.AreEqual(source.Count, target.Count);
        }
    }

    internal class CopyItemSource
    {
        public Guid Id { get; set; }
        public long Version { get; set; }
        public string Name { get; set; }
        public DateTimeOffset Created { get; set; }
        public int Count { get; set; }
    }

    internal class CopyItemTarget
    {
        public int Id { get; set; }
        public int Version { get; set; }
        public string Name { get; set; }
        public DateTimeOffset Created { get; set; }
        public long Count { get; set; }
    }
}