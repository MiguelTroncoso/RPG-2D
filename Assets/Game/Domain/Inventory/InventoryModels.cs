using System;
using Lumbre.Game.Domain.Persistence;

namespace Lumbre.Game.Domain.Inventory
{
    public enum EquipmentSlot
    {
        Relic = 0
    }

    public readonly struct InventoryItem : IEquatable<InventoryItem>
    {
        public InventoryItem(string itemId, string displayName, EquipmentSlot? equipmentSlot = null)
        {
            ItemId = itemId ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            EquipmentSlot = equipmentSlot;
        }

        public string ItemId { get; }
        public string DisplayName { get; }
        public EquipmentSlot? EquipmentSlot { get; }
        public bool IsValid => !string.IsNullOrWhiteSpace(ItemId);
        public bool IsEquippable => EquipmentSlot.HasValue;

        public bool Equals(InventoryItem other)
        {
            return string.Equals(ItemId, other.ItemId, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is InventoryItem other && Equals(other);
        }

        public override int GetHashCode()
        {
            return ItemId?.GetHashCode() ?? 0;
        }
    }

    public enum InventoryOperationCode
    {
        Added = 0,
        Duplicate = 1,
        Full = 2,
        InvalidItem = 3
    }

    public readonly struct InventoryOperationResult
    {
        private InventoryOperationResult(InventoryOperationCode code, int slotIndex)
        {
            Code = code;
            SlotIndex = slotIndex;
        }

        public InventoryOperationCode Code { get; }
        public int SlotIndex { get; }
        public bool Succeeded => Code == InventoryOperationCode.Added;

        public static InventoryOperationResult Added(int slotIndex)
        {
            return new InventoryOperationResult(InventoryOperationCode.Added, slotIndex);
        }

        public static InventoryOperationResult Failure(InventoryOperationCode code)
        {
            return new InventoryOperationResult(code, -1);
        }
    }

    public sealed class InventoryModel
    {
        private readonly InventoryItem?[] _slots;

        public InventoryModel(int capacity)
        {
            Capacity = Math.Max(1, capacity);
            _slots = new InventoryItem?[Capacity];
        }

        public int Capacity { get; }
        public int Count { get; private set; }

        public InventoryItem? GetAt(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= Capacity)
            {
                return null;
            }

            return _slots[slotIndex];
        }

        public bool Contains(string itemId)
        {
            return FindSlot(itemId) >= 0;
        }

        public InventoryPersistenceData Capture()
        {
            var data = new InventoryPersistenceData
            {
                Capacity = Capacity
            };

            for (var index = 0; index < _slots.Length; index++)
            {
                if (!_slots[index].HasValue)
                {
                    continue;
                }

                var item = _slots[index].Value;
                data.Items.Add(new InventoryItemPersistenceData
                {
                    ItemId = item.ItemId,
                    DisplayName = item.DisplayName,
                    EquipmentSlot = item.EquipmentSlot
                });
            }

            return data;
        }

        public int Restore(InventoryPersistenceData data)
        {
            Clear();
            if (data?.Items == null)
            {
                return 0;
            }

            foreach (var savedItem in data.Items)
            {
                if (savedItem == null)
                {
                    continue;
                }

                TryAdd(new InventoryItem(
                    savedItem.ItemId,
                    savedItem.DisplayName,
                    savedItem.EquipmentSlot));
            }

            return Count;
        }

        public void Clear()
        {
            Array.Clear(_slots, 0, _slots.Length);
            Count = 0;
        }

        public bool TryGet(string itemId, out InventoryItem item)
        {
            var slot = FindSlot(itemId);
            if (slot < 0)
            {
                item = default;
                return false;
            }

            item = _slots[slot].Value;
            return true;
        }

        public InventoryOperationResult TryAdd(InventoryItem item)
        {
            if (!item.IsValid)
            {
                return InventoryOperationResult.Failure(InventoryOperationCode.InvalidItem);
            }

            if (Contains(item.ItemId))
            {
                return InventoryOperationResult.Failure(InventoryOperationCode.Duplicate);
            }

            for (var index = 0; index < _slots.Length; index++)
            {
                if (_slots[index].HasValue)
                {
                    continue;
                }

                _slots[index] = item;
                Count++;
                return InventoryOperationResult.Added(index);
            }

            return InventoryOperationResult.Failure(InventoryOperationCode.Full);
        }

        private int FindSlot(string itemId)
        {
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return -1;
            }

            for (var index = 0; index < _slots.Length; index++)
            {
                if (_slots[index].HasValue
                    && string.Equals(_slots[index].Value.ItemId, itemId, StringComparison.Ordinal))
                {
                    return index;
                }
            }

            return -1;
        }
    }

    public enum EquipmentOperationCode
    {
        Equipped = 0,
        MissingItem = 1,
        NotEquippable = 2,
        WrongSlot = 3,
        AlreadyEquipped = 4
    }

    public readonly struct EquipmentOperationResult
    {
        private EquipmentOperationResult(EquipmentOperationCode code)
        {
            Code = code;
        }

        public EquipmentOperationCode Code { get; }
        public bool Succeeded => Code == EquipmentOperationCode.Equipped;

        public static EquipmentOperationResult Success()
        {
            return new EquipmentOperationResult(EquipmentOperationCode.Equipped);
        }

        public static EquipmentOperationResult Failure(EquipmentOperationCode code)
        {
            return new EquipmentOperationResult(code);
        }
    }

    public sealed class EquipmentModel
    {
        private readonly EquipmentSlot _slot;
        private InventoryItem? _equippedItem;

        public EquipmentModel(EquipmentSlot slot)
        {
            _slot = slot;
        }

        public EquipmentSlot Slot => _slot;
        public InventoryItem? EquippedItem => _equippedItem;

        public string EquippedItemId => _equippedItem.HasValue
            ? _equippedItem.Value.ItemId
            : string.Empty;

        public EquipmentOperationResult TryEquip(InventoryModel inventory, string itemId)
        {
            if (inventory == null || !inventory.TryGet(itemId, out var item))
            {
                return EquipmentOperationResult.Failure(EquipmentOperationCode.MissingItem);
            }

            if (!item.IsEquippable)
            {
                return EquipmentOperationResult.Failure(EquipmentOperationCode.NotEquippable);
            }

            if (item.EquipmentSlot.Value != _slot)
            {
                return EquipmentOperationResult.Failure(EquipmentOperationCode.WrongSlot);
            }

            if (_equippedItem.HasValue && _equippedItem.Value.Equals(item))
            {
                return EquipmentOperationResult.Failure(EquipmentOperationCode.AlreadyEquipped);
            }

            _equippedItem = item;
            return EquipmentOperationResult.Success();
        }

        public EquipmentOperationResult Restore(InventoryModel inventory, string itemId)
        {
            _equippedItem = null;
            if (string.IsNullOrWhiteSpace(itemId))
            {
                return EquipmentOperationResult.Success();
            }

            return TryEquip(inventory, itemId);
        }

        public void Clear()
        {
            _equippedItem = null;
        }
    }
}
