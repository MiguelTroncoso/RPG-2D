using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using Lumbre.Game.Application.Persistence;
using Lumbre.Game.Domain.Persistence;

namespace Lumbre.Game.Infrastructure.Local
{
    public sealed class JsonFileSaveRepository : ISaveRepository
    {
        private readonly string _filePath;
        private readonly string _temporaryFilePath;

        public JsonFileSaveRepository(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("A save file path is required.", nameof(filePath));
            }

            _filePath = Path.GetFullPath(filePath);
            _temporaryFilePath = _filePath + ".tmp";
        }

        public string FilePath => _filePath;
        public string TemporaryFilePath => _temporaryFilePath;

        public SaveLoadResult Load()
        {
            if (!File.Exists(_filePath))
            {
                return new SaveLoadResult(
                    SaveLoadStatus.NewGame,
                    SaveGameData.CreateNew(),
                    string.Empty);
            }

            try
            {
                var json = File.ReadAllText(_filePath, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return Corrupt("Save file is empty.");
                }

                var data = Deserialize(json);
                if (data == null)
                {
                    return Corrupt("Save file did not contain a save object.");
                }

                if (data.SchemaVersion != SaveSchema.CurrentVersion)
                {
                    return new SaveLoadResult(
                        SaveLoadStatus.Incompatible,
                        SaveGameData.CreateNew(),
                        $"Unsupported save schema version {data.SchemaVersion}; "
                        + $"expected {SaveSchema.CurrentVersion}.");
                }

                return new SaveLoadResult(SaveLoadStatus.Loaded, data, string.Empty);
            }
            catch (Exception exception) when (exception is IOException
                || exception is UnauthorizedAccessException
                || exception is SerializationException
                || exception is FormatException
                || exception is InvalidOperationException)
            {
                return Corrupt($"Could not read save file: {exception.Message}");
            }
        }

        public SaveOperationResult Save(SaveGameData data)
        {
            if (data == null || data.SchemaVersion != SaveSchema.CurrentVersion)
            {
                return new SaveOperationResult(
                    SaveOperationCode.InvalidData,
                    "Save data is null or uses an incompatible schema version.");
            }

            try
            {
                var directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var json = Serialize(data);
                File.WriteAllText(_temporaryFilePath, json, new UTF8Encoding(false));
                ReplaceTemporaryFile();
                return new SaveOperationResult(SaveOperationCode.Saved, string.Empty);
            }
            catch (Exception exception) when (exception is IOException
                || exception is UnauthorizedAccessException
                || exception is SerializationException
                || exception is InvalidOperationException)
            {
                TryDeleteTemporaryFile();
                return new SaveOperationResult(SaveOperationCode.Failed, exception.Message);
            }
        }

        public void Reset()
        {
            TryDelete(_filePath);
            TryDelete(_temporaryFilePath);
        }

        private static string Serialize(SaveGameData data)
        {
            var serializer = new DataContractJsonSerializer(typeof(SaveGameData));
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, data);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        private static SaveGameData Deserialize(string json)
        {
            var serializer = new DataContractJsonSerializer(typeof(SaveGameData));
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return serializer.ReadObject(stream) as SaveGameData;
            }
        }

        private void ReplaceTemporaryFile()
        {
            if (!File.Exists(_filePath))
            {
                File.Move(_temporaryFilePath, _filePath);
                return;
            }

            try
            {
                File.Replace(_temporaryFilePath, _filePath, null);
            }
            catch (PlatformNotSupportedException)
            {
                ReplaceByMoveAfterDelete();
            }
            catch (IOException)
            {
                ReplaceByMoveAfterDelete();
            }
        }

        private void ReplaceByMoveAfterDelete()
        {
            TryDelete(_filePath);
            File.Move(_temporaryFilePath, _filePath);
        }

        private SaveLoadResult Corrupt(string error)
        {
            return new SaveLoadResult(
                SaveLoadStatus.Corrupt,
                SaveGameData.CreateNew(),
                error);
        }

        private void TryDeleteTemporaryFile()
        {
            TryDelete(_temporaryFilePath);
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}
