using System;
using System.Collections.Generic;
using System.IO;
using Lumbre.Game.Application.Persistence;
using Lumbre.Game.Domain.Constants;
using Lumbre.Game.Domain.Events;
using Lumbre.Game.Domain.Inventory;
using Lumbre.Game.Domain.Missions;
using Lumbre.Game.Domain.Persistence;
using Lumbre.Game.Domain.Progression;
using Lumbre.Game.Infrastructure.Local;
using NUnit.Framework;

namespace Lumbre.Game.Tests
{
    public sealed class H6ProgressionPersistenceTests
    {
        private string _savePath;

        [SetUp]
        public void SetUp()
        {
            _savePath = Path.Combine(
                Path.GetTempPath(),
                "lumbre-h6-test-" + Guid.NewGuid().ToString("N") + ".json");
        }

        [TearDown]
        public void TearDown()
        {
            new JsonFileSaveRepository(_savePath).Reset();
        }

        [Test]
        public void ExperienceReachesLevelTwoExactlyOnceAndRejectsDuplicateSources()
        {
            var eventBus = new DomainEventBus();
            var experience = new ExperienceModel(
                ProjectConstants.H6MaximumLevel,
                ProjectConstants.H6ExperienceToLevelTwo);
            var progression = new H6ProgressionModel(eventBus, experience);
            var gainedEvents = new List<ExperienceGainedEvent>();
            var levelUpEvents = new List<LevelUpEvent>();
            eventBus.Published += domainEvent =>
            {
                if (domainEvent is ExperienceGainedEvent gained)
                {
                    gainedEvents.Add(gained);
                }

                if (domainEvent is LevelUpEvent levelUp)
                {
                    levelUpEvents.Add(levelUp);
                }
            };

            eventBus.Publish(new CombatantDefeatedEvent(CombatantKind.Mordeluz, "common-1"));
            eventBus.Publish(new CombatantDefeatedEvent(CombatantKind.Mordeluz, "common-2"));
            eventBus.Publish(new CombatantDefeatedEvent(CombatantKind.Mordeluz, "common-3"));
            eventBus.Publish(new CombatantDefeatedEvent(
                CombatantKind.MordeluzResonante, "resonant-1"));

            Assert.AreEqual(60, progression.Snapshot.TotalExperience);
            Assert.AreEqual(ProjectConstants.H6StartingLevel, progression.Snapshot.Level);

            eventBus.Publish(new MissionRewardGrantedEvent(
                new InventoryItem(H5MissionModel.RewardItemId, H5MissionModel.RewardDisplayName,
                    EquipmentSlot.Relic)));

            Assert.AreEqual(100, progression.Snapshot.TotalExperience);
            Assert.AreEqual(ProjectConstants.H6MaximumLevel, progression.Snapshot.Level);
            Assert.AreEqual(5, gainedEvents.Count,
                "The five normal rewards must produce five XP events.");
            Assert.AreEqual(1, levelUpEvents.Count);

            eventBus.Publish(new CombatantDefeatedEvent(CombatantKind.Mordeluz, "common-1"));
            eventBus.Publish(new MissionRewardGrantedEvent(
                new InventoryItem(H5MissionModel.RewardItemId, H5MissionModel.RewardDisplayName,
                    EquipmentSlot.Relic)));
            Assert.AreEqual(100, progression.Snapshot.TotalExperience);
            Assert.AreEqual(5, gainedEvents.Count);

            progression.Dispose();
        }

        [Test]
        public void MissionInventoryEquipmentAndProgressionRoundTripThroughDtos()
        {
            var eventBus = new DomainEventBus();
            var mission = new H5MissionModel(eventBus, new InventoryModel(6));
            var inventory = new InventoryModel(6);
            var equipment = new EquipmentModel(EquipmentSlot.Relic);
            var experience = new ExperienceModel(2, 100);

            Assert.IsTrue(mission.TryAccept().Succeeded);
            eventBus.Publish(new CombatantDefeatedEvent(CombatantKind.Mordeluz, "common-1"));
            eventBus.Publish(new CombatantDefeatedEvent(CombatantKind.Mordeluz, "common-2"));
            eventBus.Publish(new CombatantDefeatedEvent(CombatantKind.Mordeluz, "common-3"));
            eventBus.Publish(new CombatantDefeatedEvent(
                CombatantKind.MordeluzResonante, "resonant-1"));
            Assert.IsTrue(mission.TryTurnIn().Succeeded);

            Assert.IsTrue(inventory.TryAdd(mission.RewardItem).Succeeded);
            Assert.IsTrue(equipment.TryEquip(inventory, H5MissionModel.RewardItemId).Succeeded);
            Assert.IsTrue(experience.TryAddExperience(100, "test-mission").Succeeded);

            var save = SaveGameData.CreateNew();
            save.Experience = experience.Capture();
            save.Mission = mission.Capture();
            save.Inventory = inventory.Capture();
            save.EquippedItemId = equipment.EquippedItemId;
            save.SafePosition = new SafePositionData(4.5f, 2.25f, -0.3f);

            var restoredBus = new DomainEventBus();
            var restoredInventory = new InventoryModel(6);
            var restoredMission = new H5MissionModel(restoredBus, restoredInventory);
            var restoredEquipment = new EquipmentModel(EquipmentSlot.Relic);
            var restoredExperience = new ExperienceModel(2, 100);
            restoredMission.Restore(save.Mission);
            restoredInventory.Restore(save.Inventory);
            restoredEquipment.Restore(restoredInventory, save.EquippedItemId);
            restoredExperience.Restore(save.Experience);

            Assert.AreEqual(MissionState.Completed, restoredMission.State);
            Assert.AreEqual(3, restoredMission.Snapshot.MordeluzDefeated);
            Assert.AreEqual(1, restoredMission.Snapshot.ResonantDefeated);
            Assert.AreEqual(1, restoredInventory.Count);
            Assert.AreEqual(H5MissionModel.RewardItemId, restoredEquipment.EquippedItemId);
            Assert.AreEqual(2, restoredExperience.Level);
            Assert.AreEqual(100, restoredExperience.TotalExperience);
            Assert.AreEqual(4.5f, save.SafePosition.X);
            Assert.AreEqual(MissionOperationCode.Completed,
                restoredMission.TryTurnIn().Code,
                "A loaded completed mission must not deliver a second reward.");
            Assert.AreEqual(1, restoredInventory.Count);
        }

        [Test]
        public void MissingSaveCreatesNewGameAndRoundTripUsesVersionedJson()
        {
            var repository = new JsonFileSaveRepository(_savePath);
            var missing = repository.Load();
            Assert.AreEqual(SaveLoadStatus.NewGame, missing.Status);
            Assert.AreEqual(SaveSchema.CurrentVersion, missing.Data.SchemaVersion);

            var data = SaveGameData.CreateNew();
            data.SafePosition = new SafePositionData(1f, 2f, -0.3f);
            data.Experience.TotalExperience = 60;
            data.Experience.Level = 1;
            Assert.IsTrue(repository.Save(data).Succeeded);
            Assert.IsTrue(File.Exists(_savePath));
            Assert.IsFalse(File.Exists(repository.TemporaryFilePath));

            var loaded = repository.Load();
            Assert.AreEqual(SaveLoadStatus.Loaded, loaded.Status);
            Assert.AreEqual(60, loaded.Data.Experience.TotalExperience);
            Assert.AreEqual(1f, loaded.Data.SafePosition.X);
            Assert.AreEqual(2f, loaded.Data.SafePosition.Y);
        }

        [Test]
        public void CorruptSaveFallsBackWithoutBlockingAndReportsTheError()
        {
            File.WriteAllText(_savePath, "{ definitely not valid json");
            var result = new JsonFileSaveRepository(_savePath).Load();

            Assert.AreEqual(SaveLoadStatus.Corrupt, result.Status);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.Error));
            Assert.AreEqual(MissionState.Available, result.Data.Mission.State);
            Assert.AreEqual(1, result.Data.Experience.Level);
        }

        [Test]
        public void IncompatibleSchemaFallsBackToNewGame()
        {
            File.WriteAllText(_savePath, "{\"SchemaVersion\":999}");
            var result = new JsonFileSaveRepository(_savePath).Load();

            Assert.AreEqual(SaveLoadStatus.Incompatible, result.Status);
            Assert.IsFalse(string.IsNullOrWhiteSpace(result.Error));
            Assert.AreEqual(SaveSchema.CurrentVersion, result.Data.SchemaVersion);
            Assert.AreEqual(0, result.Data.Experience.TotalExperience);
        }

        [Test]
        public void ExperienceRestorePreservesSourceIdsAndPreventsReplay()
        {
            var original = new ExperienceModel(2, 100);
            Assert.IsTrue(original.TryAddExperience(10, "xp:mordeluz:one").Succeeded);
            Assert.IsTrue(original.TryAddExperience(30, "xp:mordeluz-resonante:one").Succeeded);

            var restored = new ExperienceModel(2, 100);
            restored.Restore(original.Capture());

            Assert.AreEqual(40, restored.TotalExperience);
            Assert.AreEqual(ExperienceOperationCode.Duplicate,
                restored.TryAddExperience(10, "xp:mordeluz:one").Code);
            Assert.AreEqual(40, restored.TotalExperience);
        }
    }
}
