# H9 — Vertical Slice Polish

## Alcance

H9 mejora la presentación de la demo v0.8.1 Alpha sin añadir mecánicas ni actores. Se mantienen intactos combate, misión, progresión, inventario, guardado local y contratos de capas.

## Cambios

- Cámara Cinemachine oficial con offset de composición, damping reducido, zoom de lectura y `CinemachineConfiner2D` sobre `H9_CameraBounds`.
- Raíz `H9_SafeAreaRoot` para la UI completa de `PlayerUI`, con `CanvasScaler` 1920×1080 y adaptación a safe area.
- Joystick de 228 px, handle de 120 px y botones móviles de 148 px con separación uniforme y color tint de selección.
- HUD de estado/misión compacto; `H5MissionHud` queda reservado para prompts contextuales de hablar/equipar.
- Pausa y opciones reacomodadas para reducir desplazamiento vertical en resoluciones landscape.
- `H9VerticalSlicePolishBuilder.Validate` comprueba cámara, safe area y escena.

## Validación reproducible

```bash
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics -quit \
  -projectPath "/Users/migueltroncoso/Documents/Juego 2D" \
  -executeMethod Lumbre.Game.Editor.H9VerticalSlicePolishBuilder.Build \
  -logFile /tmp/lumbre-h9-builder.log
```

La validación estructural de escena pasa en batchmode. El test dedicado es `H9VerticalSlicePolishPlayModeTests`; debe ejecutarse junto con las regresiones H3–H8 cuando el runner de Unity tenga disponible el .NET SDK requerido por `build-server`.

## Android

El build de desarrollo se genera en `/tmp/lumbre-h9-android.apk`. La captura [h9-android-layout-preview.png](captures/h9-android-layout-preview.png) es un render de referencia 1920×1080, no una captura física. Durante esta ejecución no había dispositivo ADB conectado; las capturas y métricas de hardware quedan pendientes de la revisión técnica con un teléfono.
