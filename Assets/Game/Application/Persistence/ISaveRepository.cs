using Lumbre.Game.Domain.Persistence;

namespace Lumbre.Game.Application.Persistence
{
    public enum SaveLoadStatus
    {
        Loaded = 0,
        NewGame = 1,
        Corrupt = 2,
        Incompatible = 3
    }

    public readonly struct SaveLoadResult
    {
        public SaveLoadResult(SaveLoadStatus status, SaveGameData data, string error)
        {
            Status = status;
            Data = data ?? SaveGameData.CreateNew();
            Error = error ?? string.Empty;
        }

        public SaveLoadStatus Status { get; }
        public SaveGameData Data { get; }
        public string Error { get; }
        public bool Succeeded => Status == SaveLoadStatus.Loaded
            || Status == SaveLoadStatus.NewGame;
    }

    public enum SaveOperationCode
    {
        Saved = 0,
        InvalidData = 1,
        Failed = 2
    }

    public readonly struct SaveOperationResult
    {
        public SaveOperationResult(SaveOperationCode code, string error)
        {
            Code = code;
            Error = error ?? string.Empty;
        }

        public SaveOperationCode Code { get; }
        public string Error { get; }
        public bool Succeeded => Code == SaveOperationCode.Saved;
    }

    public interface ISaveRepository
    {
        SaveLoadResult Load();
        SaveOperationResult Save(SaveGameData data);
        void Reset();
    }
}
