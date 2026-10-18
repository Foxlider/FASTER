using NUnit.Framework;

using System.Collections.Generic;

namespace FASTER.Models.Tests
{
    [TestFixture()]
    public class LocalModIdTests
    {
        [Test()]
        public void IdsAreUniqueAndAddedToTheSet()
        {
            var taken = new HashSet<uint>();
            var ids   = new List<uint>();

            for (var i = 0; i < 1000; i++)
                ids.Add(ModUtilities.NewLocalModId(taken));

            Assert.That(ids, Is.Unique);
            Assert.That(taken, Has.Count.EqualTo(1000));
        }

        [Test()]
        public void TakenIdsAreSkipped()
        {
            // Every ID in the range is taken except one
            var taken = new HashSet<uint>();
            for (uint i = 0; i < ushort.MaxValue / 2; i++)
                taken.Add(uint.MaxValue - i);

            var free = uint.MaxValue - 100u;
            taken.Remove(free);

            Assert.That(ModUtilities.NewLocalModId(taken), Is.EqualTo(free));
        }
    }
}