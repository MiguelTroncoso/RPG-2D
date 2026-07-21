using Lumbre.Game.Domain.Events;
using Lumbre.Game.Domain.Inventory;
using Lumbre.Game.Domain.Missions;
using NUnit.Framework;

namespace Lumbre.Game.Tests
{
    public sealed class H5MissionInventoryTests
    {
        [Test]
        public void MissionProgressesFromEventsAndDeliversOneEquippableReward()
        {
            var eventBus = new DomainEventBus();
            var inventory = new InventoryModel(6);
            var equipment = new EquipmentModel(EquipmentSlot.Relic);
            var mission = new H5MissionModel(eventBus, inventory);

            Assert.AreEqual(MissionState.Available, mission.State);
            Assert.IsTrue(mission.TryAccept().Succeeded);
            Assert.AreEqual(MissionState.Active, mission.State);

            eventBus.Publish(new CombatantDefeatedEvent(CombatantKind.Mordeluz, "mordeluz-1"));
            eventBus.Publish(new CombatantDefeatedEvent(CombatantKind.Mordeluz, "mordeluz-1"));
            eventBus.Publish(new CombatantDefeatedEvent(CombatantKind.Mordeluz, "mordeluz-2"));
            eventBus.Publish(new CombatantDefeatedEvent(CombatantKind.Mordeluz, "mordeluz-3"));
            eventBus.Publish(new CombatantDefeatedEvent(
                CombatantKind.MordeluzResonante, "mordeluz-resonante"));

            Assert.AreEqual(3, mission.Snapshot.MordeluzDefeated);
            Assert.AreEqual(1, mission.Snapshot.ResonantDefeated);
            Assert.AreEqual(MissionState.ReadyToTurnIn, mission.State);

            var delivery = mission.TryTurnIn();
            Assert.IsTrue(delivery.Succeeded);
            Assert.AreEqual(MissionOperationCode.RewardDelivered, delivery.Code);
            Assert.AreEqual(MissionState.Completed, mission.State);
            Assert.AreEqual(1, inventory.Count);
            Assert.IsTrue(inventory.Contains(H5MissionModel.RewardItemId));
            Assert.IsTrue(equipment.TryEquip(inventory, H5MissionModel.RewardItemId).Succeeded);
            Assert.IsTrue(equipment.EquippedItem.HasValue);

            var secondDelivery = mission.TryTurnIn();
            Assert.AreEqual(MissionOperationCode.Completed, secondDelivery.Code);
            Assert.AreEqual(1, inventory.Count, "The fixed reward must not be duplicated.");
            Assert.AreEqual(EquipmentOperationCode.AlreadyEquipped,
                equipment.TryEquip(inventory, H5MissionModel.RewardItemId).Code);
        }

        [Test]
        public void InventoryHasSixSlotsAndRejectsDuplicatesAfterItIsFull()
        {
            var inventory = new InventoryModel(6);

            for (var index = 0; index < inventory.Capacity; index++)
            {
                var result = inventory.TryAdd(new InventoryItem($"item-{index}", $"Item {index}"));
                Assert.IsTrue(result.Succeeded);
                Assert.AreEqual(index, result.SlotIndex);
            }

            Assert.AreEqual(6, inventory.Count);
            Assert.AreEqual(InventoryOperationCode.Duplicate,
                inventory.TryAdd(new InventoryItem("item-0", "Duplicate")).Code);
            Assert.AreEqual(InventoryOperationCode.Full,
                inventory.TryAdd(new InventoryItem("item-6", "Seventh item")).Code);
            Assert.AreEqual(6, inventory.Count);
        }

        [Test]
        public void MissionIgnoresDefeatsBeforeAcceptanceAndAfterCompletion()
        {
            var eventBus = new DomainEventBus();
            var inventory = new InventoryModel(6);
            var mission = new H5MissionModel(eventBus, inventory);

            eventBus.Publish(new CombatantDefeatedEvent(CombatantKind.Mordeluz, "before-accept"));
            Assert.AreEqual(0, mission.Snapshot.MordeluzDefeated);

            mission.TryAccept();
            eventBus.Publish(new CombatantDefeatedEvent(CombatantKind.Mordeluz, "one"));
            eventBus.Publish(new CombatantDefeatedEvent(CombatantKind.Mordeluz, "two"));
            eventBus.Publish(new CombatantDefeatedEvent(CombatantKind.Mordeluz, "three"));
            eventBus.Publish(new CombatantDefeatedEvent(CombatantKind.MordeluzResonante, "resonant"));
            Assert.IsTrue(mission.TryTurnIn().Succeeded);

            eventBus.Publish(new CombatantDefeatedEvent(CombatantKind.Mordeluz, "after-completion"));
            Assert.AreEqual(3, mission.Snapshot.MordeluzDefeated);
            Assert.AreEqual(MissionState.Completed, mission.State);
        }
    }
}
