# H8.1 — Diagnóstico y corrección de pantalla negra en Android

> Estado: corrección aplicada y verificada en Editor/APK; validación física pendiente por falta de dispositivo ADB. La versión del proyecto permanece en `v0.8.0 Alpha`.

## Incidente

En el dispositivo Android de prueba se observó el splash/logo de Unity y luego una pantalla completamente negra. No aparecían `Bootstrap`, `VerticalSlice`, jugador, HUD ni controles táctiles.

La primera regla de H8.1 fue no cambiar materiales, shaders, cámara o stripping a ciegas. Se intentó capturar logcat antes de modificar código:

- `/tmp/lumbre-h8-1-logcat-clear.txt`
- `/tmp/lumbre-black-screen-logcat.txt`
- `/tmp/lumbre-black-screen-logcat-filtered.txt`

El ADB incluido con Unity no detectó ningún dispositivo conectado, por lo que no existe logcat físico disponible todavía.

## Causa raíz confirmada

La causa no era inicialmente un shader ni Cinemachine: `Bootstrap.unity` era la escena 0 de Build Settings, pero solo contenía un marcador y cámaras. No tenía ningún componente que cargara `VerticalSlice` y no había una llamada runtime a `SceneManager.LoadScene` desde Bootstrap.

Por eso el APK podía compilar correctamente y mostrar el splash, pero permanecía en una escena sin mundo, jugador ni Canvas. Los tests anteriores cargaban `VerticalSlice` directamente, ocultando el fallo de la ruta real de arranque.

## Corrección mínima

- `BootstrapSceneLoader` carga `VerticalSlice` mediante `LoadSceneAsync`, registra progreso y fuerza `Time.timeScale = 1` al iniciar.
- El loader valida jugador, cámara, HUD y Canvas tras la activación.
- Si Cinemachine o la cámara principal no están disponibles, se habilita una cámara ortográfica estática de fallback sin modificar las reglas de gameplay.
- `H8OptimizationBuilder` configura el loader en Bootstrap y valida que Bootstrap sea el índice 0 y que VerticalSlice esté habilitada.
- `H8LocalSettings` sanitiza valores no finitos, calidad inválida y fallos de `PlayerPrefs` con defaults seguros.
- `H8PerformanceOverlay` trata `ProfilerRecorder` como diagnóstico opcional y no bloqueante.
- `H8PauseController` inicia explícitamente con pausa cerrada y `Time.timeScale = 1`.

El APK de diagnóstico generado es:

`/tmp/lumbre-h8-1-black-screen-debug.apk`

Configuración: Android ARM64, Development, Script Debugging, Autoconnect Profiler, símbolos de desarrollo y OpenGLES3 como API inicial. Vulkan queda para una comparación posterior si el dispositivo lo requiere.

## Verificación automatizada

- Builder H8.1: completado en batchmode, código 0.
- Build Settings: Bootstrap índice 0 y VerticalSlice incluido, validado por el builder y PlayMode.
- EditMode: 23/23 pasados.
- PlayMode: 8/8 pasados.
- El test H8.1 confirma la transición Bootstrap → VerticalSlice, jugador, cámara Cinemachine, Canvas, Input System UI, un AudioListener, HUD configurado, paneles de pausa/opciones cerrados y fallback seguro para preferencias inválidas.
- El log de PlayMode contiene la secuencia `[BOOT]` completa hasta jugador, cámara y HUD.

## Validación física pendiente

La lista ADB permanece vacía. El ejecutable usado fue:

`/Applications/Unity/Hub/Editor/6000.3.20f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb`

Quedan pendientes, sin declarar H8.1 finalizado:

1. Instalar el APK en un Android físico.
2. Capturar logcat limpio y filtrado.
3. Confirmar cinco arranques: instalación limpia, segundo arranque, con save, después de minimizar y después de forzar cierre.
4. Confirmar táctil, recorrido plaza → cueva → plaza, HUD y retorno sin pantalla negra.

H9 queda bloqueado hasta completar esta validación de arranque en hardware. No se añadieron NPC, misiones, inventario, economía, networking, guardado nuevo ni contenido de gameplay.
