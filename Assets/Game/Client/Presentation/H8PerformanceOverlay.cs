using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace Lumbre.Game.Client.Presentation
{
    public sealed class H8PerformanceOverlay : MonoBehaviour
    {
        [SerializeField] private Text output;

        private ProfilerRecorder _drawCalls;
        private ProfilerRecorder _gcReserved;
        private ProfilerRecorder _systemMemory;
        private ProfilerRecorder _mainThreadFrameTime;
        private ProfilerRecorder _gpuFrameTime;
        private float _elapsed;
        private int _frames;
        private bool _showFps;
        private bool _showDebug;

        public bool IsConfigured => output != null;
        public bool IsVisible => _showFps || _showDebug;
        public string LastReport { get; private set; } = string.Empty;

        public void Configure(Text label)
        {
            output = label;
            output.raycastTarget = false;
        }

        public void SetVisibility(bool showFps, bool showDebug)
        {
            _showFps = showFps;
            _showDebug = showDebug;
            if (output != null)
            {
                output.enabled = IsVisible;
            }
        }

        private void Awake()
        {
            output ??= GetComponent<Text>();
        }

        private void OnEnable()
        {
            if (!UnityEngine.Application.isPlaying)
            {
                return;
            }

            _drawCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
            _gcReserved = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
            _systemMemory = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
            _mainThreadFrameTime = ProfilerRecorder.StartNew(ProfilerCategory.Internal,
                "Main Thread Frame Time");
            _gpuFrameTime = ProfilerRecorder.StartNew(ProfilerCategory.Render, "GPU Frame Time");
        }

        private void Update()
        {
            if (!IsVisible)
            {
                return;
            }

            _frames++;
            _elapsed += Time.unscaledDeltaTime;
            if (_elapsed < 0.5f)
            {
                return;
            }

            var fps = _frames / Mathf.Max(0.001f, _elapsed);
            var frameMs = _elapsed / Mathf.Max(1, _frames) * 1000f;
            var memoryMb = Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f);
            var gcMb = _gcReserved.Valid
                ? _gcReserved.LastValue / (1024f * 1024f)
                : Profiler.GetMonoUsedSizeLong() / (1024f * 1024f);
            var systemMb = _systemMemory.Valid
                ? _systemMemory.LastValue / (1024f * 1024f)
                : memoryMb;
            var drawCalls = _drawCalls.Valid ? _drawCalls.LastValue : 0L;
            var cpuMs = FormatMilliseconds(_mainThreadFrameTime);
            var gpuMs = FormatMilliseconds(_gpuFrameTime);

            LastReport = $"FPS {fps:0}  FRAME {frameMs:0.0}ms\n"
                + $"MEM {memoryMb:0}MB  GC {gcMb:0}MB  SYS {systemMb:0}MB\n"
                + $"CPU {cpuMs}ms  GPU {gpuMs}ms  DRAWCALLS {drawCalls}";
            if (_showDebug)
            {
                var qualityName = QualitySettings.names.Length == 0
                    ? "AUTO"
                    : QualitySettings.names[Mathf.Clamp(QualitySettings.GetQualityLevel(),
                        0, QualitySettings.names.Length - 1)];
                LastReport += "\nQUALITY " + qualityName;
            }

            if (output != null)
            {
                output.text = LastReport;
            }

            _elapsed = 0f;
            _frames = 0;
        }

        private void OnDisable()
        {
            if (_drawCalls.Valid)
            {
                _drawCalls.Dispose();
            }

            if (_gcReserved.Valid)
            {
                _gcReserved.Dispose();
            }

            if (_systemMemory.Valid)
            {
                _systemMemory.Dispose();
            }

            if (_mainThreadFrameTime.Valid)
            {
                _mainThreadFrameTime.Dispose();
            }

            if (_gpuFrameTime.Valid)
            {
                _gpuFrameTime.Dispose();
            }
        }

        private static string FormatMilliseconds(ProfilerRecorder recorder)
        {
            if (!recorder.Valid || recorder.LastValue <= 0)
            {
                return "--";
            }

            return (recorder.LastValue / 1000000f).ToString("0.0");
        }
    }
}
