using System;
using System.Linq;
using System.Reflection;
using BreakNotice.Core.Identity;
using NUnit.Framework;

namespace BreakNotice.Core.Tests
{
    public sealed class RuntimeIdentityTests
    {
        [Test]
        public void ContentId_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ContentId(null));
        }

        [Test]
        public void ContentId_Empty_Throws()
        {
            Assert.Throws<ArgumentException>(() => new ContentId(string.Empty));
        }

        [Test]
        public void ContentId_WhitespaceOnly_Throws()
        {
            Assert.Throws<ArgumentException>(() => new ContentId(" \t\r\n"));
        }

        [Test]
        public void ContentId_Equality_IsOrdinal()
        {
            var exact = new ContentId("  Employee-A  ");
            var same = new ContentId("  Employee-A  ");
            var differentCase = new ContentId("  employee-a  ");

            Assert.That(exact, Is.EqualTo(same));
            Assert.That(exact == same, Is.True);
            Assert.That(exact != differentCase, Is.True);
            Assert.That(exact.Value, Is.EqualTo("  Employee-A  "));
            Assert.That(exact.GetHashCode(), Is.EqualTo(same.GetHashCode()));
            Assert.That(default(ContentId).IsValid, Is.False);
            Assert.Throws<InvalidOperationException>(() =>
            {
                var ignored = default(ContentId).Value;
            });
        }

        [Test]
        public void RuntimeEntityId_Zero_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new RuntimeEntityId(0));
            Assert.That(default(RuntimeEntityId).IsValid, Is.False);
            Assert.That(new RuntimeEntityId(42).Value, Is.EqualTo(42UL));
        }

        [Test]
        public void EncounterGenerationId_Zero_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new EncounterGenerationId(0));
            Assert.That(default(EncounterGenerationId).IsValid, Is.False);
            Assert.That(new EncounterGenerationId(7).Value, Is.EqualTo(7U));
        }

        [Test]
        public void Allocator_FirstId_IsOne()
        {
            var generation = new EncounterGenerationId(3);
            var allocator = new RuntimeIdentityAllocator(generation);
            var content = new ContentId("employee-a");
            var runtimeId = new RuntimeEntityId(1);

            Assert.Throws<ArgumentException>(() => new RuntimeIdentityAllocator(default));
            Assert.Throws<ArgumentException>(() => new RuntimeIdentity(default, runtimeId, generation));
            Assert.Throws<ArgumentException>(() => new RuntimeIdentity(content, default, generation));
            Assert.Throws<ArgumentException>(() => new RuntimeIdentity(content, runtimeId, default));
            Assert.That(allocator.Allocate().Value, Is.EqualTo(1UL));
        }

        [Test]
        public void Allocator_Ids_AreMonotonicAndUnique()
        {
            var allocator = new RuntimeIdentityAllocator(new EncounterGenerationId(4));
            var first = allocator.Allocate();
            var second = allocator.Allocate();
            var third = allocator.Allocate();

            Assert.That(second.Value, Is.EqualTo(first.Value + 1));
            Assert.That(third.Value, Is.EqualTo(second.Value + 1));
            Assert.That(new[] { first, second, third }.Distinct().Count(), Is.EqualTo(3));
        }

        [Test]
        public void Allocator_Generation_RemainsFixed()
        {
            var generation = new EncounterGenerationId(5);
            var allocator = new RuntimeIdentityAllocator(generation);

            allocator.Allocate();
            allocator.Allocate();

            Assert.That(allocator.GenerationId, Is.EqualTo(generation));
            Assert.That(typeof(RuntimeIdentityAllocator).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic), Is.Empty);
        }

        [Test]
        public void RuntimeAssembly_HasNoMutableStaticFields()
        {
            var assembly = typeof(ContentId).Assembly;
            var expectedTypes = new[]
            {
                "BreakNotice.Core.Identity.ContentId",
                "BreakNotice.Core.Identity.EncounterGenerationId",
                "BreakNotice.Core.Identity.RuntimeEntityId",
                "BreakNotice.Core.Identity.RuntimeEntityRegistry",
                "BreakNotice.Core.Identity.RuntimeIdentity",
                "BreakNotice.Core.Identity.RuntimeIdentityAllocator"
            };
            var exportedTypes = assembly.GetExportedTypes()
                .Select(type => type.FullName)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
            var mutableStaticFields = assembly.GetTypes()
                .SelectMany(type => type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                .Where(field => !field.IsInitOnly && !field.IsLiteral)
                .ToArray();
            var implicitConversions = typeof(ContentId).GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(method => method.Name == "op_Implicit")
                .ToArray();

            Assert.That(exportedTypes, Is.EqualTo(expectedTypes));
            Assert.That(mutableStaticFields, Is.Empty);
            Assert.That(implicitConversions, Is.Empty);
        }

        [Test]
        public void RuntimeAssembly_HasNoUnityReferences()
        {
            var references = typeof(ContentId).Assembly.GetReferencedAssemblies();

            Assert.That(references.Any(reference => reference.Name.StartsWith("UnityEngine", StringComparison.Ordinal)), Is.False);
            Assert.That(references.Any(reference => reference.Name.StartsWith("UnityEditor", StringComparison.Ordinal)), Is.False);
        }
    }
}
