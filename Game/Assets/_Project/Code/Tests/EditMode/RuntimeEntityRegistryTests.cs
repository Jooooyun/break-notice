using System;
using BreakNotice.Core.Identity;
using NUnit.Framework;

namespace BreakNotice.Core.Tests
{
    public sealed class RuntimeEntityRegistryTests
    {
        [Test]
        public void Register_MatchingGeneration_Succeeds()
        {
            var generation = new EncounterGenerationId(1);
            var registry = new RuntimeEntityRegistry(generation);
            var identity = CreateIdentity("employee-a", 1, generation);

            Assert.Throws<ArgumentException>(() => registry.Register(default));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                RuntimeIdentity ignored;
                registry.TryGet(default, out ignored);
            });
            Assert.Throws<ArgumentOutOfRangeException>(() => registry.Remove(default));
            Assert.That(registry.Count, Is.Zero);

            registry.Register(identity);

            Assert.That(registry.GenerationId, Is.EqualTo(generation));
            Assert.That(registry.Count, Is.EqualTo(1));
        }

        [Test]
        public void Register_DuplicateRuntimeEntityId_Throws()
        {
            var generation = new EncounterGenerationId(2);
            var registry = new RuntimeEntityRegistry(generation);
            var identity = CreateIdentity("employee-a", 1, generation);
            registry.Register(identity);

            Assert.Throws<InvalidOperationException>(() => registry.Register(identity));
            Assert.That(registry.Count, Is.EqualTo(1));
        }

        [Test]
        public void Register_MismatchedGeneration_Throws()
        {
            var registry = new RuntimeEntityRegistry(new EncounterGenerationId(3));
            var mismatched = CreateIdentity("employee-a", 1, new EncounterGenerationId(4));

            Assert.Throws<ArgumentException>(() => registry.Register(mismatched));
            Assert.That(registry.Count, Is.Zero);
        }

        [Test]
        public void TryGet_ReturnsExactIdentity()
        {
            var generation = new EncounterGenerationId(5);
            var registry = new RuntimeEntityRegistry(generation);
            var identity = CreateIdentity("employee-a", 1, generation);
            registry.Register(identity);

            var found = registry.TryGet(identity.RuntimeEntityId, out var resolved);

            Assert.That(found, Is.True);
            Assert.That(resolved, Is.EqualTo(identity));
            Assert.That(registry.Count, Is.EqualTo(1));
        }

        [Test]
        public void Remove_RemovesOnlyRequestedIdentity()
        {
            var generation = new EncounterGenerationId(6);
            var registry = new RuntimeEntityRegistry(generation);
            var first = CreateIdentity("employee-a", 1, generation);
            var second = CreateIdentity("employee-b", 2, generation);
            registry.Register(first);
            registry.Register(second);

            var removed = registry.Remove(first.RuntimeEntityId);

            Assert.That(removed, Is.True);
            Assert.That(registry.Count, Is.EqualTo(1));
            Assert.That(registry.TryGet(second.RuntimeEntityId, out var remaining), Is.True);
            Assert.That(remaining, Is.EqualTo(second));
            Assert.That(registry.Remove(first.RuntimeEntityId), Is.False);
            Assert.That(registry.Count, Is.EqualTo(1));
        }

        [Test]
        public void Clear_ReturnsCountToZero()
        {
            var generation = new EncounterGenerationId(7);
            var registry = new RuntimeEntityRegistry(generation);
            registry.Register(CreateIdentity("employee-a", 1, generation));
            registry.Register(CreateIdentity("employee-b", 2, generation));

            registry.Clear();

            Assert.That(registry.Count, Is.Zero);
        }

        private static RuntimeIdentity CreateIdentity(
            string contentId,
            ulong runtimeEntityId,
            EncounterGenerationId generationId)
        {
            return new RuntimeIdentity(
                new ContentId(contentId),
                new RuntimeEntityId(runtimeEntityId),
                generationId);
        }
    }
}
