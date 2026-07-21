using System;
using System.Linq;
using Lumbre.Game.Domain;
using Lumbre.Game.Infrastructure.Local;
using NUnit.Framework;

namespace Lumbre.Game.Tests
{
    public sealed class ArchitectureSmokeTests
    {
        [Test]
        public void DomainAssemblyDoesNotReferenceUnityEngine()
        {
            var references = typeof(SessionState).Assembly.GetReferencedAssemblies();

            Assert.IsFalse(references.Any(reference => reference.Name == "UnityEngine"));
            Assert.IsFalse(references.Any(reference => reference.Name.StartsWith("UnityEngine.", StringComparison.Ordinal)));
        }

        [Test]
        public void LocalSessionStartsAndPublishesAnEvent()
        {
            var session = new LocalGameSession();
            var eventCount = 0;
            session.EventPublished += _ => eventCount++;

            session.Start();

            Assert.AreEqual(SessionStatus.Running, session.Status);
            Assert.AreEqual(1, eventCount);
            Assert.AreEqual(1, session.CurrentState.Revision);
        }
    }
}
