using System;
using System.Collections.Generic;
using Lumbre.Game.Domain.Constants;
using Lumbre.Game.Domain.Events;
using Lumbre.Game.Domain.Missions;
using Lumbre.Game.Domain.Persistence;

namespace Lumbre.Game.Domain.Progression
{
    public enum ExperienceSource
    {
        Mordeluz = 0,
        MordeluzResonante = 1,
        MissionCompleted = 2
    }

    public enum ExperienceOperationCode
    {
        Added = 0,
        Duplicate = 1,
        MaxLevel = 2,
        InvalidAmount = 3,
        InvalidSource = 4
    }

    public readonly struct ExperienceOperationResult
    {
        private ExperienceOperationResult(ExperienceOperationCode code, int amountAdded,
            int level, int totalExperience, bool leveledUp)
        {
            Code = code;
            AmountAdded = amountAdded;
            Level = level;
            TotalExperience = totalExperience;
            LeveledUp = leveledUp;
        }

        public ExperienceOperationCode Code { get; }
        public int AmountAdded { get; }
        public int Level { get; }
        public int TotalExperience { get; }
        public bool LeveledUp { get; }
        public bool Succeeded => Code == ExperienceOperationCode.Added;

        public static ExperienceOperationResult Added(int amount, int level,
            int totalExperience, bool leveledUp)
        {
            return new ExperienceOperationResult(
                ExperienceOperationCode.Added, amount, level, totalExperience, leveledUp);
        }

        public static ExperienceOperationResult Failure(ExperienceOperationCode code,
            int level, int totalExperience)
        {
            return new ExperienceOperationResult(code, 0, level, totalExperience, false);
        }
    }

    public readonly struct ExperienceSnapshot
    {
        public ExperienceSnapshot(int level, int maxLevel, int totalExperience,
            int experienceToNextLevel)
        {
            Level = level;
            MaxLevel = maxLevel;
            TotalExperience = totalExperience;
            ExperienceToNextLevel = experienceToNextLevel;
        }

        public int Level { get; }
        public int MaxLevel { get; }
        public int TotalExperience { get; }
        public int ExperienceToNextLevel { get; }
        public int CurrentLevelExperience => TotalExperience;
        public float NormalizedProgress => ExperienceToNextLevel <= 0
            ? 1f
            : Math.Min(1f, TotalExperience / (float)ExperienceToNextLevel);
    }

    public readonly struct ExperienceGainedEvent : IDomainEvent
    {
        public ExperienceGainedEvent(ExperienceSource source, string sourceId, int amount,
            ExperienceSnapshot snapshot)
        {
            Source = source;
            SourceId = sourceId ?? string.Empty;
            Amount = amount;
            Snapshot = snapshot;
        }

        public ExperienceSource Source { get; }
        public string SourceId { get; }
        public int Amount { get; }
        public ExperienceSnapshot Snapshot { get; }
    }

    public readonly struct LevelUpEvent : IDomainEvent
    {
        public LevelUpEvent(int previousLevel, int currentLevel, ExperienceSnapshot snapshot)
        {
            PreviousLevel = previousLevel;
            CurrentLevel = currentLevel;
            Snapshot = snapshot;
        }

        public int PreviousLevel { get; }
        public int CurrentLevel { get; }
        public ExperienceSnapshot Snapshot { get; }
    }

    public sealed class ExperienceModel
    {
        private readonly HashSet<string> _appliedSourceIds =
            new HashSet<string>(StringComparer.Ordinal);
        private int _level;
        private int _totalExperience;

        public ExperienceModel(int maxLevel, int experienceToNextLevel)
        {
            MaxLevel = Math.Max(1, maxLevel);
            ExperienceToNextLevel = Math.Max(1, experienceToNextLevel);
            _level = 1;
        }

        public int Level => _level;
        public int MaxLevel { get; }
        public int TotalExperience => _totalExperience;
        public int ExperienceToNextLevel { get; }
        public ExperienceSnapshot Snapshot => new ExperienceSnapshot(
            _level, MaxLevel, _totalExperience, ExperienceToNextLevel);

        public ExperienceOperationResult TryAddExperience(int amount, string sourceId)
        {
            if (amount <= 0)
            {
                return ExperienceOperationResult.Failure(
                    ExperienceOperationCode.InvalidAmount, _level, _totalExperience);
            }

            if (string.IsNullOrWhiteSpace(sourceId))
            {
                return ExperienceOperationResult.Failure(
                    ExperienceOperationCode.InvalidSource, _level, _totalExperience);
            }

            if (!_appliedSourceIds.Add(sourceId))
            {
                return ExperienceOperationResult.Failure(
                    ExperienceOperationCode.Duplicate, _level, _totalExperience);
            }

            if (_level >= MaxLevel)
            {
                return ExperienceOperationResult.Failure(
                    ExperienceOperationCode.MaxLevel, _level, _totalExperience);
            }

            var previousLevel = _level;
            _totalExperience = Math.Min(
                ExperienceToNextLevel, _totalExperience + amount);
            if (_totalExperience >= ExperienceToNextLevel)
            {
                _level = MaxLevel;
            }

            return ExperienceOperationResult.Added(
                amount, _level, _totalExperience, previousLevel != _level);
        }

        public ExperiencePersistenceData Capture()
        {
            return new ExperiencePersistenceData
            {
                Level = _level,
                TotalExperience = _totalExperience,
                MaxLevel = MaxLevel,
                ExperienceToNextLevel = ExperienceToNextLevel,
                AppliedSourceIds = new List<string>(_appliedSourceIds)
            };
        }

        public void Restore(ExperiencePersistenceData data)
        {
            _appliedSourceIds.Clear();
            if (data?.AppliedSourceIds != null)
            {
                foreach (var sourceId in data.AppliedSourceIds)
                {
                    if (!string.IsNullOrWhiteSpace(sourceId))
                    {
                        _appliedSourceIds.Add(sourceId);
                    }
                }
            }

            _level = data == null ? 1 : Math.Max(1, Math.Min(MaxLevel, data.Level));
            _totalExperience = data == null
                ? 0
                : Math.Max(0, Math.Min(ExperienceToNextLevel, data.TotalExperience));
            if (_level >= MaxLevel)
            {
                _level = MaxLevel;
                _totalExperience = ExperienceToNextLevel;
            }
        }

        public void Reset()
        {
            _appliedSourceIds.Clear();
            _level = 1;
            _totalExperience = 0;
        }
    }

    public sealed class H6ProgressionModel : IDisposable
    {
        private readonly DomainEventBus _eventBus;
        private readonly ExperienceModel _experience;

        public H6ProgressionModel(DomainEventBus eventBus, ExperienceModel experience)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _experience = experience ?? throw new ArgumentNullException(nameof(experience));
            _eventBus.Published += HandleDomainEvent;
        }

        public ExperienceModel Experience => _experience;
        public ExperienceSnapshot Snapshot => _experience.Snapshot;

        public void Dispose()
        {
            _eventBus.Published -= HandleDomainEvent;
        }

        public void Restore(ExperiencePersistenceData data)
        {
            _experience.Restore(data);
        }

        public ExperiencePersistenceData Capture()
        {
            return _experience.Capture();
        }

        public void Reset()
        {
            _experience.Reset();
        }

        private void HandleDomainEvent(IDomainEvent domainEvent)
        {
            if (domainEvent is CombatantDefeatedEvent defeatedEvent)
            {
                TryAwardCombatExperience(defeatedEvent);
                return;
            }

            if (domainEvent is MissionRewardGrantedEvent)
            {
                TryAwardExperience(
                    ExperienceSource.MissionCompleted,
                    "xp:mission:" + H5MissionModel.MissionId,
                    ProjectConstants.PlayerMissionExperience);
            }
        }

        private void TryAwardCombatExperience(CombatantDefeatedEvent defeatedEvent)
        {
            if (string.IsNullOrWhiteSpace(defeatedEvent.EntityId))
            {
                return;
            }

            switch (defeatedEvent.Kind)
            {
                case CombatantKind.Mordeluz:
                    TryAwardExperience(
                        ExperienceSource.Mordeluz,
                        "xp:mordeluz:" + defeatedEvent.EntityId,
                        ProjectConstants.MordeluzExperience);
                    break;
                case CombatantKind.MordeluzResonante:
                    TryAwardExperience(
                        ExperienceSource.MordeluzResonante,
                        "xp:mordeluz-resonante:" + defeatedEvent.EntityId,
                        ProjectConstants.MordeluzResonanteExperience);
                    break;
            }
        }

        private void TryAwardExperience(ExperienceSource source, string sourceId, int amount)
        {
            var previousLevel = _experience.Level;
            var result = _experience.TryAddExperience(amount, sourceId);
            if (!result.Succeeded)
            {
                return;
            }

            _eventBus.Publish(new ExperienceGainedEvent(source, sourceId, result.AmountAdded,
                _experience.Snapshot));
            if (result.LeveledUp)
            {
                _eventBus.Publish(new LevelUpEvent(previousLevel, _experience.Level,
                    _experience.Snapshot));
            }
        }
    }
}
