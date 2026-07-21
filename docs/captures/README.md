# Capturas H9/H9.1/H9.2

`h9-android-layout-preview.png` es un render de referencia 1920×1080 de la escena `VerticalSlice` con la presentación H9. Sirve para revisar composición, safe area, cámara y escala de controles; no es una captura de un teléfono.

`android-h9/` conserva el espacio histórico reservado para H9/H9.1. La evidencia física de cierre está en `android-h9.2-physical/`.

## H9.2 — evidencia física Android

Estas seis imágenes provienen de una validación física manual. El APK fue instalado manualmente en un teléfono Android y las capturas se tomaron durante el recorrido y el combate. No contienen notificaciones, correos, teléfonos ni identificadores personales.

| Orden | Evidencia |
| --- | --- |
| 01 | [Gameplay en cueva](android-h9.2-physical/01-gameplay-cave.jpeg) |
| 02 | [Gameplay en plaza](android-h9.2-physical/02-gameplay-plaza.jpeg) |
| 03 | [Combate y área de peligro](android-h9.2-physical/03-combat-area.jpeg) |
| 04 | [Movimiento y composición de cámara](android-h9.2-physical/04-camera-movement.jpeg) |
| 05 | [Controles táctiles y joystick](android-h9.2-physical/05-touch-controls.jpeg) |
| 06 | [HUD y safe area](android-h9.2-physical/06-hud-safe-area.jpeg) |

La evidencia visual confirma, dentro de las escenas capturadas, la safe area, la lectura del HUD, el encuadre de cámara y la visibilidad de los controles táctiles. El perfilado mediante ADB, logcat y Unity Profiler sigue pendiente y no se infiere rendimiento a partir de estas imágenes.

La política de privacidad de capturas y el cierre con observaciones están documentados en `docs/H9_1_PHYSICAL_VALIDATION.md`.
