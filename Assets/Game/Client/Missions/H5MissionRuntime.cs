using Lumbre.Game.Domain.Events;
using Lumbre.Game.Domain.Inventory;
using Lumbre.Game.Domain.Missions;
using UnityEngine;

namespace Lumbre.Game.Client.Missions
{
    [DefaultExecutionOrder(-100)]
    public sealed class H5MissionRuntime : MonoBehaviour
    {
        [SerializeField, Min(1)] private int inventoryCapacity = 6;

        private DomainEventBus _eventBus;
        private H5MissionModel _mission;
        private InventoryModel _inventory;
        private EquipmentModel _equipment;

        public DomainEventBus EventBus => _eventBus;
        public H5MissionModel Mission => _mission;
        public InventoryModel Inventory => _inventory;
        public EquipmentModel Equipment => _equipment;

        private void Awake()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            _mission?.Dispose();
        }

        public void ConfigureInventoryCapacity(int capacity)
        {
            inventoryCapacity = Mathf.Max(1, capacity);
            if (UnityEngine.Application.isPlaying)
            {
                Initialize();
            }
        }

        public void Publish(IDomainEvent domainEvent)
        {
            Initialize();
            _eventBus.Publish(domainEvent);
        }

        public EquipmentOperationResult TryEquipReward()
        {
            Initialize();
            return _equipment.TryEquip(_inventory, H5MissionModel.RewardItemId);
        }

        private void Initialize()
        {
            if (_mission != null)
            {
                return;
            }

            _eventBus = new DomainEventBus();
            _inventory = new InventoryModel(inventoryCapacity);
            _equipment = new EquipmentModel(EquipmentSlot.Relic);
            _mission = new H5MissionModel(_eventBus, _inventory);
        }
    }
}
