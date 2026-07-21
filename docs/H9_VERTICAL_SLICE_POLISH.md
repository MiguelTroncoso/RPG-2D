# H9 — Vertical Slice Polish

## Alcance

H9 mejora la presentación de la demo v0.8.1 Alpha sin añadir mecánicas ni actores. Se mantienen intactos combate, misión, progresión, inventario, guardado local y contratos de capas.

## Cambios

- Cámara Cinemachine oficial con offset de composición, damping reducido, zoom de lectura y `CinemachineConfiner2D` sobre `H9_CameraBounds`.
- Raíz `H9_SafeAreaRoot` para la UI completa de `PlayerUI`, con `CanvasScaler` 1920×1080 y adaptación a safe area.
- Joystick con área táctil de 228 px, visual de 168 px, handle de 96 px y botones móviles de 148 px con separación uniforme y color tint de selección.
- HUD de estado/misión compacto; `H5MissionHud` queda reservado para prompts contextuales de hablar/equipar.
- Pausa y opciones reacomodadas para reducir desplazamiento vertical en resoluciones landscape.
- `H9VerticalSlicePolishBuilder.Validate` comprueba cámara, safe area y escena.
- El builder sincroniza el offset de composición en el binding H3 y en Cinemachine para que `Awake` no restaure la composición anterior.

## Validación reproducible

```bash
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics -quit \
  -projectPath "/Users/migueltroncoso/Documents/Juego 2D" \
  -executeMethod Lumbre.Game.Editor.H9VerticalSlicePolishBuilder.Build \
  -logFile /tmp/lumbre-h9-builder.log
```

La validación estructural de escena pasa en batchmode. El test dedicado es `H9VerticalSlicePolishPlayModeTests`. Para obtener XML no se usa `-quit` junto con `-runTests`; Unity termina al finalizar el Test Framework.

H9.1 verificó dos ciclos Build → Validate con código 0, EditMode 23/23 y PlayMode 9/9. La regresión H9 detectó y corrigió el offset de cámara que `H3CinemachineFollowTarget` sobrescribía en runtime.

## H9.2 — Corrección de layout Android

H9.2 conserva el alcance de H9 y corrige únicamente defectos de presentación detectados al preparar la validación física: controles superiores dentro de la safe area, viewport completo sin franja negra inferior, snapshot inicial del HUD, joystick menos obstructivo, paneles H7 más densos y prompts táctiles sin teclas de teclado. La superficie táctil del joystick no se reduce.

El builder valida la escena en 1920×1080, 2400×1080, 2340×1080, 2560×1080 y 1280×720, además de comprobar que la cámara ocupa el viewport completo y que el respawn y los spawns de combate quedan dentro de `H9_CameraBounds` y fuera de obstáculos. La validación física en Android sigue siendo obligatoria y pendiente.

## Android

El build de desarrollo se genera en `/tmp/lumbre-h9-android.apk` con ARM64, OpenGLES3, Development Build, Allow Debugging y ConnectWithProfiler. La captura [h9-android-layout-preview.png](captures/h9-android-layout-preview.png) es un render de referencia 1920×1080, no una captura física. Durante H9.1 `adb devices -l` no detectó un dispositivo; las capturas, arranques y métricas de hardware quedan pendientes. El detalle está en [H9.1 Physical Validation](H9_1_PHYSICAL_VALIDATION.md).
