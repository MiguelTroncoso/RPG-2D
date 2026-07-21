using System;
using System.Collections.Generic;
using Lumbre.Game.Domain.Events;
using Lumbre.Game.Domain.Inventory;
using Lumbre.Game.Domain.Persistence;

namespace Lumbre.Game.Domain.Missions
{
    public enum MissionState
    {
        Available = 0,
        Active = 1,
        ReadyToTurnIn = 2,
        Completed = 3
    }

    public readonly struct H5MissionSnapshot
    {
        public H5MissionSnapshot(MissionState state, int mordeluzDefeated,
            int mordeluzRequired, int resonantDefeated, int resonantRequired,
            bool rewardDelivered)
        {
            State = state;
            MordeluzDefeated = mordeluzDefeated;
            MordeluzRequired = mordeluzRequired;
            ResonantDefeated = resonantDefeated;
            ResonantRequired = resonantRequired;
            RewardDelivered = rewardDelivered;
        }

        public MissionState State { get; }
        public int MordeluzDefeated { get; }
        public int MordeluzRequired { get; }
        public int ResonantDefeated { get; }
        public int ResonantRequired { get; }
        public bool RewardDelivered { get; }
        public bool IsReady => MordeluzDefeated >= MordeluzRequired
            && ResonantDefeated >= ResonantRequired;
    }

    public enum MissionOperationCode
    {
        Accepted = 0,
        AlreadyActive = 1,
        NotAvailable = 2,
        NotReadyToTurnIn = 3,
        Completed = 4,
        InventoryFull = 5,
        RewardDelivered = 6
    }

    public readonly struct MissionOperationResult
    {
        private MissionOperationResult(MissionOperationCode code)
        {
            Code = code;
        }

        public MissionOperationCode Code { get; }
        public bool Succeeded => Code == MissionOperationCode.Accepted
            || Code == MissionOperationCode.RewardDelivered;

        public static MissionOperationResult Success(MissionOperationCode code)
        {
            return new MissionOperationResult(code);
        }

        public static MissionOperationResult Failure(MissionOperationCode code)
        {
            return new MissionOperationResult(code);
        }
    }

    public readonly struct MissionStateChangedEvent : IDomainEvent
    {
        public MissionStateChangedEvent(MissionState previous, MissionState current)
        {
            Previous = previous;
            Current = current;
        }

        public MissionState Previous { get; }
        public MissionState Current { get; }
    }

    public readonly struct MissionProgressChangedEvent : IDomainEvent
    {
        public MissionProgressChangedEvent(H5MissionSnapshot snapshot)
        {
            Snapshot = snapshot;
        }

        public H5MissionSnapshot Snapshot { get; }
    }

    public readonly struct MissionRewardGrantedEvent : IDomainEvent
    {
        public MissionRewardGrantedEvent(InventoryItem item)
        {
            Item = item;
        }

        public InventoryItem Item { get; }
    }

    public sealed class H5MissionModel : IDisposable
    {
        public const int RequiredMordeluz = 3;
        public const int RequiredResonant = 1;
        public const string MissionId = "nara-mordeluz-resonance";
        public const string RewardItemId = "nara-fragmento-resonancia";
        public const string RewardDisplayName = "Fragmento de Resonancia";

        private readonly DomainEventBus _eventBus;
        private readonly InventoryModel _inventory;
        private readonly HashSet<string> _processedDefeatIds = new HashSet<string>(StringComparer.Ordinal);
        private MissionState _state = MissionState.Available;
        private int _mordeluzDefeated;
        private int _resonantDefeated;
        private bool _rewardDelivered;

        public H5MissionModel(DomainEventBus eventBus, InventoryModel inventory)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _eventBus.Published += HandleDomainEvent;
        }

        public MissionState State => _state;
        public H5MissionSnapshot Snapshot => new H5MissionSnapshot(
            _state,
            _mordeluzDefeated,
            RequiredMordeluz,
            _resonantDefeated,
            RequiredResonant,
            _rewardDelivered);

        public InventoryItem RewardItem => new InventoryItem(
            RewardItemId,
            RewardDisplayName,
            EquipmentSlot.Relic);

        public MissionPersistenceData Capture()
        {
            return new MissionPersistenceData
            {
                MissionId = MissionId,
                State = _state,
                MordeluzDefeated = _mordeluzDefeated,
                ResonantDefeated = _resonantDefeated,
                RewardDelivered = _rewardDelivered,
                ProcessedDefeatIds = new List<string>(_processedDefeatIds)
            };
        }

        public void Restore(MissionPersistenceData data)
        {
            if (data != null
                && !string.IsNullOrWhiteSpace(data.MissionId)
                && !string.Equals(data.MissionId, MissionId, StringComparison.Ordinal))
            {
                Reset();
                return;
            }

            _processedDefeatIds.Clear();
            if (data?.ProcessedDefeatIds != null)
            {
                foreach (var defeatId in data.ProcessedDefeatIds)
                {
                    if (!string.IsNullOrWhiteSpace(defeatId))
                    {
                        _processedDefeatIds.Add(defeatId);
                    }
                }
            }

            _mordeluzDefeated = data == null
                ? 0
                : Math.Max(0, Math.Min(RequiredMordeluz, data.MordeluzDefeated));
            _resonantDefeated = data == null
                ? 0
                : Math.Max(0, Math.Min(RequiredResonant, data.ResonantDefeated));

            var restoredState = data == null || !Enum.IsDefined(typeof(MissionState), data.State)
                ? MissionState.Available
                : data.State;
            _rewardDelivered = data != null
                && (data.RewardDelivered || restoredState == MissionState.Completed);
            if (_rewardDelivered)
            {
                _state = MissionState.Completed;
            }
            else if (restoredState == MissionState.Active && Snapshot.IsReady)
            {
                _state = MissionState.ReadyToTurnIn;
            }
            else
            {
                _state = restoredState;
            }
        }

        public void Reset()
        {
            _processedDefeatIds.Clear();
            _state = MissionState.Available;
            _mordeluzDefeated = 0;
            _resonantDefeated = 0;
            _rewardDelivered = false;
        }

        public MissionOperationResult TryAccept()
        {
            if (_state == MissionState.Active)
            {
                return MissionOperationResult.Failure(MissionOperationCode.AlreadyActive);
            }

            if (_state != MissionState.Available)
            {
                return MissionOperationResult.Failure(MissionOperationCode.NotAvailable);
            }

            ChangeState(MissionState.Active);
            return MissionOperationResult.Success(MissionOperationCode.Accepted);
        }

        public MissionOperationResult TryTurnIn()
        {
            if (_state == MissionState.Completed || _rewardDelivered)
            {
                return MissionOperationResult.Failure(MissionOperationCode.Completed);
            }

            if (_state != MissionState.ReadyToTurnIn)
            {
                return MissionOperationResult.Failure(MissionOperationCode.NotReadyToTurnIn);
            }

            if (_inventory.Contains(RewardItemId))
            {
                DeliverReward();
                return MissionOperationResult.Success(MissionOperationCode.RewardDelivered);
            }

            var addResult = _inventory.TryAdd(RewardItem);
            if (!addResult.Succeeded)
            {
                return MissionOperationResult.Failure(MissionOperationCode.InventoryFull);
            }

            DeliverReward();
            return MissionOperationResult.Success(MissionOperationCode.RewardDelivered);
        }

        public void Dispose()
        {
            _eventBus.Published -= HandleDomainEvent;
        }

        private void HandleDomainEvent(IDomainEvent domainEvent)
        {
            if (_state != MissionState.Active
                || !(domainEvent is CombatantDefeatedEvent defeatedEvent))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(defeatedEvent.EntityId)
                && !_processedDefeatIds.Add(defeatedEvent.EntityId))
            {
                return;
            }

            switch (defeatedEvent.Kind)
            {
                case CombatantKind.Mordeluz:
                    _mordeluzDefeated = Math.Min(RequiredMordeluz, _mordeluzDefeated + 1);
                    break;
                case CombatantKind.MordeluzResonante:
                    _resonantDefeated = Math.Min(RequiredResonant, _resonantDefeated + 1);
                    break;
                default:
                    return;
            }

            _eventBus.Publish(new MissionProgressChangedEvent(Snapshot));
            if (Snapshot.IsReady)
            {
                ChangeState(MissionState.ReadyToTurnIn);
            }
        }

        private void DeliverReward()
        {
            if (_rewardDelivered)
            {
                return;
            }

            _rewardDelivered = true;
            _eventBus.Publish(new MissionRewardGrantedEvent(RewardItem));
            ChangeState(MissionState.Completed);
        }

        private void ChangeState(MissionState nextState)
        {
            if (_state == nextState)
            {
                return;
            }

            var previous = _state;
            _state = nextState;
            _eventBus.Publish(new MissionStateChangedEvent(previous, nextState));
        }
    }
}
