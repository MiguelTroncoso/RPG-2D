using Lumbre.Game.Client.Presentation;
using Lumbre.Game.Domain.Constants;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lumbre.Game.Client.Bootstrap
{
    public sealed class BootstrapSceneLoader : MonoBehaviour
    {
        public const string DiagnosticVersion = "v0.8.0 Alpha-debug";

        private bool _loadStarted;

        private void Awake()
        {
            Time.timeScale = 1f;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"[BOOT] {DiagnosticVersion}");
            Debug.Log("[BOOT] Bootstrap Awake");
        }

        private void Start()
        {
            if (_loadStarted)
            {
                return;
            }

            _loadStarted = true;
            Debug.Log("[BOOT] Bootstrap Start");
            StartCoroutine(LoadVerticalSlice());
        }

        private System.Collections.IEnumerator LoadVerticalSlice()
        {
            Debug.Log("[BOOT] Loading VerticalSlice");
            var operation = SceneManager.LoadSceneAsync(ProjectConstants.VerticalSliceSceneName,
                LoadSceneMode.Single);
            if (operation == null)
            {
                Debug.LogError("[BOOT] VerticalSlice load operation was null.");
                yield break;
            }

            operation.allowSceneActivation = true;
            var lastProgress = -1f;
            while (!operation.isDone)
            {
                if (Mathf.Abs(operation.progress - lastProgress) >= 0.1f)
                {
                    lastProgress = operation.progress;
                    Debug.Log($"[BOOT] VerticalSlice progress {operation.progress:0.00}");
                }

                yield return null;
            }

            yield return null;
            var verticalSlice = SceneManager.GetSceneByName(ProjectConstants.VerticalSliceSceneName);
            if (verticalSlice.IsValid() && verticalSlice.isLoaded)
            {
                SceneManager.SetActiveScene(verticalSlice);
            }

            Debug.Log($"[BOOT] VerticalSlice activated (active={SceneManager.GetActiveScene().name})");
            yield return null;
            ValidateVerticalSlice();
            Destroy(gameObject);
        }

        private static void ValidateVerticalSlice()
        {
            var player = GameObject.FindWithTag("Player");
            var hud = GameObject.Find("H7_StatusPanel")?.GetComponent<H7StatusHud>();
            var canvas = Object.FindFirstObjectByType<Canvas>();
            var mainCamera = UnityEngine.Camera.main;
            var hasCinemachineBrain = FindComponentByTypeName("Unity.Cinemachine.CinemachineBrain") != null;
            var officialCamera = GameObject.Find("CM_OfficialCamera");

            if (mainCamera == null || !mainCamera.enabled || !hasCinemachineBrain
                || officialCamera == null || !officialCamera.activeInHierarchy)
            {
                mainCamera = CreateOrConfigureFallbackCamera(player, mainCamera);
            }

            Debug.Log($"[BOOT] Player initialized: {player != null}");
            Debug.Log($"[BOOT] Camera initialized: {mainCamera != null && mainCamera.enabled}");
            Debug.Log($"[BOOT] HUD initialized: {hud != null && hud.IsConfigured}");

            if (canvas == null || !canvas.gameObject.activeInHierarchy)
            {
                Debug.LogWarning("[BOOT] Active Canvas was not found after VerticalSlice activation.");
            }
        }

        private static UnityEngine.Camera CreateOrConfigureFallbackCamera(GameObject player,
            UnityEngine.Camera current)
        {
            var camera = current ?? Object.FindFirstObjectByType<UnityEngine.Camera>(
                FindObjectsInactive.Include);
            if (camera == null)
            {
                var cameraObject = new GameObject("H8_1_CameraFallback");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<UnityEngine.Camera>();
            }

            camera.gameObject.SetActive(true);
            camera.enabled = true;
            camera.orthographic = true;
            camera.orthographicSize = 9f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.055f, 0.09f, 1f);
            var targetPosition = player == null ? Vector3.zero : player.transform.position;
            camera.transform.position = new Vector3(targetPosition.x, targetPosition.y, -20f);
            Debug.LogWarning("[BOOT] Cinemachine unavailable; static fallback camera enabled.");
            return camera;
        }

        private static Component FindComponentByTypeName(string fullTypeName)
        {
            var type = System.AppDomain.CurrentDomain.GetAssemblies();
            for (var assemblyIndex = 0; assemblyIndex < type.Length; assemblyIndex++)
            {
                var componentType = type[assemblyIndex].GetType(fullTypeName, false);
                if (componentType == null)
                {
                    continue;
                }

                return Object.FindFirstObjectByType(componentType, FindObjectsInactive.Include) as Component;
            }

            return null;
        }
    }
}
