using System.Collections.Generic;
using System.Runtime.Serialization;
using Lumbre.Game.Domain.Inventory;
using Lumbre.Game.Domain.Missions;

namespace Lumbre.Game.Domain.Persistence
{
    public static class SaveSchema
    {
        public const int CurrentVersion = 1;
    }

    [DataContract]
    public sealed class SaveGameData
    {
        public SaveGameData()
        {
            SchemaVersion = SaveSchema.CurrentVersion;
            Experience = new ExperiencePersistenceData();
            Mission = new MissionPersistenceData();
            Inventory = new InventoryPersistenceData();
            SafePosition = new SafePositionData();
        }

        [DataMember(Order = 1)]
        public int SchemaVersion { get; set; }

        [DataMember(Order = 2)]
        public ExperiencePersistenceData Experience { get; set; }

        [DataMember(Order = 3)]
        public MissionPersistenceData Mission { get; set; }

        [DataMember(Order = 4)]
        public InventoryPersistenceData Inventory { get; set; }

        [DataMember(Order = 5)]
        public string EquippedItemId { get; set; }

        [DataMember(Order = 6)]
        public SafePositionData SafePosition { get; set; }

        public static SaveGameData CreateNew()
        {
            return new SaveGameData
            {
                SchemaVersion = SaveSchema.CurrentVersion,
                Experience = ExperiencePersistenceData.CreateNew(),
                Mission = MissionPersistenceData.CreateNew(),
                Inventory = InventoryPersistenceData.CreateNew(),
                EquippedItemId = string.Empty,
                SafePosition = new SafePositionData()
            };
        }
    }

    [DataContract]
    public sealed class ExperiencePersistenceData
    {
        public ExperiencePersistenceData()
        {
            AppliedSourceIds = new List<string>();
        }

        [DataMember(Order = 1)]
        public int Level { get; set; }

        [DataMember(Order = 2)]
        public int TotalExperience { get; set; }

        [DataMember(Order = 3)]
        public int MaxLevel { get; set; }

        [DataMember(Order = 4)]
        public int ExperienceToNextLevel { get; set; }

        [DataMember(Order = 5)]
        public List<string> AppliedSourceIds { get; set; }

        public static ExperiencePersistenceData CreateNew()
        {
            return new ExperiencePersistenceData
            {
                Level = 1,
                TotalExperience = 0,
                MaxLevel = 2,
                ExperienceToNextLevel = 100
            };
        }
    }

    [DataContract]
    public sealed class MissionPersistenceData
    {
        public MissionPersistenceData()
        {
            ProcessedDefeatIds = new List<string>();
        }

        [DataMember(Order = 1)]
        public string MissionId { get; set; }

        [DataMember(Order = 2)]
        public MissionState State { get; set; }

        [DataMember(Order = 3)]
        public int MordeluzDefeated { get; set; }

        [DataMember(Order = 4)]
        public int ResonantDefeated { get; set; }

        [DataMember(Order = 5)]
        public bool RewardDelivered { get; set; }

        [DataMember(Order = 6)]
        public List<string> ProcessedDefeatIds { get; set; }

        public static MissionPersistenceData CreateNew()
        {
            return new MissionPersistenceData
            {
                MissionId = H5MissionModel.MissionId,
                State = MissionState.Available,
                MordeluzDefeated = 0,
                ResonantDefeated = 0,
                RewardDelivered = false
            };
        }
    }

    [DataContract]
    public sealed class InventoryPersistenceData
    {
        public InventoryPersistenceData()
        {
            Items = new List<InventoryItemPersistenceData>();
        }

        [DataMember(Order = 1)]
        public int Capacity { get; set; }

        [DataMember(Order = 2)]
        public List<InventoryItemPersistenceData> Items { get; set; }

        public static InventoryPersistenceData CreateNew()
        {
            return new InventoryPersistenceData
            {
                Capacity = 6
            };
        }
    }

    [DataContract]
    public sealed class InventoryItemPersistenceData
    {
        [DataMember(Order = 1)]
        public string ItemId { get; set; }

        [DataMember(Order = 2)]
        public string DisplayName { get; set; }

        [DataMember(Order = 3)]
        public EquipmentSlot? EquipmentSlot { get; set; }
    }

    [DataContract]
    public sealed class SafePositionData
    {
        public SafePositionData()
        {
        }

        public SafePositionData(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        [DataMember(Order = 1)]
        public float X { get; set; }

        [DataMember(Order = 2)]
        public float Y { get; set; }

        [DataMember(Order = 3)]
        public float Z { get; set; }
    }
}
