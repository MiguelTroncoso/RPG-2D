# Lumbre de Nácar — H9.2 Corrección de layout Android

## Estado

**APROBADO CON OBSERVACIONES.**

H9.2 corrige únicamente defectos de presentación identificados durante la preparación de la validación física de H9.1. La validación visual física fue completada manualmente con el APK instalado en un teléfono Android. Mantiene `v0.8.1 Alpha`, los contratos H3–H8 y la decisión de no iniciar H10.

## Base y entorno

- Repositorio: `https://github.com/MiguelTroncoso/RPG-2D`
- Base de trabajo: `ef32a235cdf570bddf3b3410b4240f3062e13c84`
- Unity: `6000.3.20f1`
- Plataforma objetivo: Android landscape, ARM64, OpenGLES3
- Build: Development, Allow Debugging y ConnectWithProfiler
- Dispositivo físico: validación manual completada; fabricante, modelo y versión Android no quedaron registrados en el material entregado

## Causas y correcciones

### A. Controles superiores fuera de pantalla

Los botones móviles estaban posicionados como si el anclaje inferior derecho fuese superior derecho, con posiciones positivas que podían quedar fuera del borde superior en relaciones panorámicas. Se reanclaron al extremo superior derecho con offsets negativos y `H9SafeAreaLayout` como raíz común.

### B. Franja negra inferior

La franja no se corrige estirando la escena. El backdrop mantiene escala uniforme y el volumen de cámara `H9_CameraBounds` se amplió verticalmente de forma controlada para incluir el respawn y eliminar el hueco visible sin cambiar colisiones ni reglas.

### C. HUD vacío tras iniciar o cargar

`H7StatusHud` ahora resuelve sus referencias durante `Awake`, reintenta si la composición se completa en un orden posterior y publica el primer snapshot en `Start`. La regresión PlayMode espera el arranque real de la escena y exige texto de estado y misión no vacío.

### D. Joystick

El área táctil conserva 228×228 px. Solo se reduce la representación visual a 168×168 px y el handle a 96×96 px; el movimiento sigue usando el mismo `MovementIntent` y el mismo Input System.

### E. Paneles H7

Los paneles de estado, misión y prompt contextual usan menos altura y una jerarquía más densa, conservando lectura de vida, Calor, XP y objetivo. No se modifican las reglas que alimentan esos datos.

### F. Prompt táctil

En el esquema `Touch`, el prompt muestra `HABLAR CON NARA` y `EQUIPAR RECOMPENSA` sin teclas de teclado. En teclado/QA se mantienen `F / HABLAR CON NARA` y `G / EQUIPAR RECOMPENSA`. No se crea una ruta de input paralela.

### G. Spawns

`Validate` comprueba el respawn del jugador y los cuatro spawns de Mordeluz dentro de `H9_CameraBounds` y fuera de `ProjectLayers.WorldObstacle`. No se modifican IA, combate ni posiciones por lógica de gameplay.

## Matriz automática de layout

La validación del builder cubre:

- 1920×1080
- 2400×1080
- 2340×1080
- 2560×1080
- 1280×720

En cada caso se verifica viewport de cámara completo, referencia Canvas 1920×1080, safe area, controles superiores, joystick inferior izquierdo y paneles de HUD.

## Verificación reproducible

```bash
UNITY="/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity"
PROJECT="/Users/migueltroncoso/Documents/Juego 2D"

"$UNITY" -batchmode -nographics -quit -projectPath "$PROJECT" \
  -executeMethod Lumbre.Game.Editor.H9VerticalSlicePolishBuilder.Build \
  -logFile /tmp/lumbre-h9-2-build.log

"$UNITY" -batchmode -nographics -quit -projectPath "$PROJECT" \
  -executeMethod Lumbre.Game.Editor.H9VerticalSlicePolishBuilder.Validate \
  -logFile /tmp/lumbre-h9-2-validate.log
```

El ciclo Build → Validate se ejecutó dos veces y ambas ejecuciones terminaron con código 0 sin duplicar Safe Area, confiner, CameraBounds, HUD, botones ni componentes.

## Tests

- EditMode: 23/23 pasados, 0 fallidos, 0 ignorados.
- PlayMode: 11/11 pasados, 0 fallidos, 0 ignorados.
- XML EditMode: `/tmp/lumbre-h9-2-editmode.xml`.
- XML PlayMode: `/tmp/lumbre-h9-2-playmode.xml`.
- Se mantienen las regresiones H3, H4, H4B, H5, H6, H7 y H8.

## APK

Salida solicitada:

```text
Builds/Android/LumbreDeNacar-v0.8.1-H9.2.apk
```

Resultado generado: 76,097,309 bytes, fecha `2026-07-21 19:12:51 -0400`, SHA-256 `a88d21b3d1a924569b59772654d8a46f79812ab7c005b4e6d30b2ca7abaefd6d`. La carpeta `Builds/` y el APK están ignorados y no forman parte del commit. El APK solo demuestra empaquetado; no sustituye la instalación y prueba en un teléfono real.

## Evidencia física y observaciones

El APK fue instalado manualmente y se incorporaron seis capturas físicas en [`docs/captures/android-h9.2-physical/`](captures/android-h9.2-physical/). La evidencia confirma visualmente la safe area, el HUD, el encuadre de cámara y los controles táctiles durante el recorrido plaza–sendero–cueva y el combate.

Queda pendiente el perfilado mediante ADB, logcat y Unity Profiler, junto con sus métricas de FPS, frame time, memoria, GC, draw calls, temperatura y batería. Estas observaciones no bloquean el cierre documental visual de H9, pero deben resolverse antes de usar el resultado como benchmark Android.

## Decisión

H9 queda **APROBADO CON OBSERVACIONES**: la evidencia física manual confirma la presentación objetivo y el APK fue instalado manualmente. El perfilado ADB/logcat/Profiler permanece pendiente. No se cambian versiones, reglas de juego, escenas fuera de la composición de presentación, contratos de dominio ni se inicia H10.
