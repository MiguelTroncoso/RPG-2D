using System.IO;
using Lumbre.Game.Application.Persistence;
using Lumbre.Game.Client.Missions;
using Lumbre.Game.Client.Player;
using Lumbre.Game.Domain.Constants;
using Lumbre.Game.Domain.Events;
using Lumbre.Game.Domain.Missions;
using Lumbre.Game.Domain.Persistence;
using Lumbre.Game.Domain.Progression;
using Lumbre.Game.Infrastructure.Local;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Lumbre.Game.Client.Progression
{
    [DefaultExecutionOrder(-90)]
    public sealed class H6ProgressionRuntime : MonoBehaviour
    {
        public const string DefaultSaveFileName = "lumbre-h6-save.json";

        [SerializeField] private string saveFileName = DefaultSaveFileName;
        [SerializeField] private bool autoSave = true;
        [SerializeField] private H5MissionRuntime missionRuntime;
        [SerializeField] private H3PlayerController playerController;

        private H6ProgressionModel _progression;
        private ISaveRepository _saveRepository;
        private bool _saveRequested;
        private bool _initialized;
        private bool _hasPendingPosition;
        private Vector3 _pendingPosition;

        public H5MissionRuntime MissionRuntime => missionRuntime;
        public H6ProgressionModel Progression => _progression;
        public ISaveRepository SaveRepository => _saveRepository;
        public SaveLoadStatus LastLoadStatus { get; private set; }
        public SaveOperationResult LastSaveResult { get; private set; }
        public string SaveFilePath => _saveRepository is JsonFileSaveRepository jsonRepository
            ? jsonRepository.FilePath
            : string.Empty;

        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            if (_hasPendingPosition && playerController != null)
            {
                playerController.RestoreSafePosition(_pendingPosition);
            }

            if (LastLoadStatus != SaveLoadStatus.Loaded)
            {
                SaveNow();
            }
        }

        private void Update()
        {
            if (Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current.f5Key.wasPressedThisFrame)
            {
                SaveNow();
            }

            if (Keyboard.current.f9Key.wasPressedThisFrame)
            {
                ResetProgressForQa();
            }
        }

        private void LateUpdate()
        {
            if (_saveRequested)
            {
                _saveRequested = false;
                SaveNow();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveNow();
            }
        }

        private void OnApplicationQuit()
        {
            SaveNow();
        }

        private void OnDestroy()
        {
            if (missionRuntime?.EventBus != null)
            {
                missionRuntime.EventBus.Published -= HandleDomainEvent;
            }

            _progression?.Dispose();
        }

        public void ConfigureSaveFileName(string fileName)
        {
            if (_initialized)
            {
                return;
            }

            saveFileName = string.IsNullOrWhiteSpace(fileName)
                ? DefaultSaveFileName
                : fileName;
        }

        public void SaveNow()
        {
            Initialize();
            if (_saveRepository == null || missionRuntime == null || _progression == null)
            {
                return;
            }

            var data = SaveGameData.CreateNew();
            data.Experience = _progression.Capture();
            data.Mission = missionRuntime.Mission.Capture();
            data.Inventory = missionRuntime.Inventory.Capture();
            data.EquippedItemId = missionRuntime.Equipment.EquippedItemId;
            if (playerController != null)
            {
                var position = playerController.transform.position;
                data.SafePosition = new SafePositionData(position.x, position.y, position.z);
            }

            LastSaveResult = _saveRepository.Save(data);
            if (!LastSaveResult.Succeeded)
            {
                Debug.LogError($"H6 save failed: {LastSaveResult.Error}");
            }
        }

        public void ResetProgressForQa()
        {
            Initialize();
            _saveRepository?.Reset();
            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid())
            {
                SceneManager.LoadScene(activeScene.name, LoadSceneMode.Single);
            }
        }

        private void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            missionRuntime ??= FindFirstObjectByType<H5MissionRuntime>();
            playerController ??= FindFirstObjectByType<H3PlayerController>();
            if (missionRuntime == null)
            {
                return;
            }

            var experience = new ExperienceModel(
                ProjectConstants.H6MaximumLevel,
                ProjectConstants.H6ExperienceToLevelTwo);
            _progression = new H6ProgressionModel(missionRuntime.EventBus, experience);
            _saveRepository = new JsonFileSaveRepository(Path.Combine(
                UnityEngine.Application.persistentDataPath,
                string.IsNullOrWhiteSpace(saveFileName) ? DefaultSaveFileName : saveFileName));
            missionRuntime.EventBus.Published += HandleDomainEvent;

            var loadResult = _saveRepository.Load();
            LastLoadStatus = loadResult.Status;
            if (!string.IsNullOrWhiteSpace(loadResult.Error))
            {
                Debug.LogWarning($"H6 save load fallback: {loadResult.Error}");
            }

            Restore(loadResult.Data);
            _initialized = true;
        }

        private void Restore(SaveGameData data)
        {
            data ??= SaveGameData.CreateNew();
            _progression.Restore(data.Experience);
            missionRuntime.Mission.Restore(data.Mission);
            missionRuntime.Inventory.Restore(data.Inventory);
            missionRuntime.Equipment.Restore(
                missionRuntime.Inventory,
                data.EquippedItemId);

            if (LastLoadStatus == SaveLoadStatus.Loaded
                && data.SafePosition != null
                && IsFinite(data.SafePosition.X)
                && IsFinite(data.SafePosition.Y)
                && IsFinite(data.SafePosition.Z))
            {
                _pendingPosition = new Vector3(
                    data.SafePosition.X,
                    data.SafePosition.Y,
                    data.SafePosition.Z);
                _hasPendingPosition = true;
            }
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private void HandleDomainEvent(IDomainEvent domainEvent)
        {
            if (!autoSave)
            {
                return;
            }

            if (domainEvent is CombatantDefeatedEvent
                || domainEvent is MissionProgressChangedEvent
                || domainEvent is MissionStateChangedEvent
                || domainEvent is MissionRewardGrantedEvent
                || domainEvent is ExperienceGainedEvent
                || domainEvent is LevelUpEvent)
            {
                _saveRequested = true;
            }
        }
    }
}
