# Lumbre de Nácar — H9.1 Validación física, regresiones y cierre de H9

## Estado de decisión

**APROBADO CON OBSERVACIONES**

La validación automatizada, el builder H9, el APK Android y la evidencia visual de una validación física manual están verificados. El APK fue instalado manualmente y las capturas confirman safe area, HUD, cámara y controles táctiles. El perfilado mediante ADB, logcat y Unity Profiler sigue pendiente.

## Identificación

- Proyecto: Lumbre de Nácar
- Repositorio: `https://github.com/MiguelTroncoso/RPG-2D`
- Commit base: `b1746f02fd7291ada73ec123b53bd420ba7843fa`
- Versión: `v0.8.1 Alpha`
- Hito: H9.1 — Validación física, regresiones y cierre de H9
- Commit de esta validación: se asigna al publicar H9.1

## Entorno de pruebas

- Unity: `6000.3.20f1`
- Plataforma del runner: macOS
- SDK Android/NDK/OpenJDK: instalados dentro de Unity
- ADB: `/Applications/Unity/Hub/Editor/6000.3.20f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb`
- Fecha de ejecución: `2026-07-21`
- Dispositivo: teléfono Android validado manualmente; modelo no registrado en el material entregado
- Fabricante/modelo: no disponible
- Versión Android: no disponible
- Resolución y aspect ratio del dispositivo: no disponible; las capturas entregadas son JPEG de 1356×610
- RAM: no disponible
- Orientación física: landscape

## Pruebas automáticas

El problema del entorno quedó resuelto identificando que `-quit` cerraba Unity antes de que el Test Framework terminara. Las ejecuciones H9.1 usan `-runTests` sin `-quit`; Unity termina cuando finaliza la suite.

### EditMode

- Total: 23
- Pasados: 23
- Fallidos: 0
- Ignorados/inconclusos: 0
- Código de salida Unity: 0
- XML: `/tmp/lumbre-h9-editmode-final.qZqdTZ/results.xml`

### PlayMode

- Total: 9
- Pasados: 9
- Fallidos: 0
- Ignorados/inconclusos: 0
- Código de salida Unity: 0
- XML: `/tmp/lumbre-h9-playmode-final.lMz5Vs/results.xml`
- Incluye regresiones H3, H4, H4B, H5, H6, H7, H8.1, H8 y la prueba H9.

## Builder H9

Se ejecutó la secuencia completa:

1. `H9VerticalSlicePolishBuilder.Build` — código 0.
2. `H9VerticalSlicePolishBuilder.Validate` — código 0.
3. `H9VerticalSlicePolishBuilder.Build` — código 0.
4. `H9VerticalSlicePolishBuilder.Validate` — código 0.

La escena mantiene una única `H9_SafeAreaRoot`, un `H9_CameraBounds`, un `CinemachineConfiner2D`, un componente `H9SafeAreaLayout` y los controles/HUD existentes. No se detectaron duplicaciones.

## APK Android

- Ruta: `/tmp/lumbre-h9-android.apk`
- Resultado: generado correctamente
- Arquitectura: ARM64
- API gráfica: OpenGLES3
- Development Build: sí
- Allow Debugging: sí
- ConnectWithProfiler: sí
- Tamaño: 1,023,031,188 bytes, aproximadamente 73 MB
- APK: no versionado en GitHub

## Validación física manual

Se completó una validación manual con el APK instalado directamente en un teléfono Android. Las capturas muestran el recorrido plaza–sendero–cueva, HUD, cámara, joystick, controles ATK/DEF/AOE y una situación de combate.

La evidencia visual confirma:

- safe area y controles dentro del área visible;
- HUD de vida, Calor, inventario, misión y experiencia sin recortes visibles;
- encuadre de cámara durante el recorrido;
- joystick y botones táctiles visibles y utilizables en las escenas capturadas.

No se afirma evidencia de rendimiento: la captura de logcat, el perfilado por ADB y Unity Profiler, FPS, frame time, memoria, GC, draw calls, temperatura y batería siguen pendientes.

## Capturas

- Física Android: [`android-h9.2-physical/`](captures/android-h9.2-physical/), seis capturas manuales.
- Índice de capturas: [`docs/captures/README.md`](captures/README.md).
- Render de referencia: [`h9-android-layout-preview.png`](captures/h9-android-layout-preview.png). No representa hardware físico.
- Las capturas físicas no contienen notificaciones, correos, números de teléfono ni identificadores personales.

## Bug encontrado y corrección

La prueba PlayMode H9 detectó que `H7CameraPolish.IsPolished` era falso durante el runtime: `H3CinemachineFollowTarget.Awake` restauraba `followOffset` a `{x:0,y:0,z:-20}`, sobrescribiendo la composición H9 `{x:0,y:0.65,z:-20}`.

Corrección permitida y aplicada:

- `H9VerticalSlicePolishBuilder` sincroniza el offset en `H3CinemachineFollowTarget` además de `CinemachineFollow`.
- La escena conserva el offset H9 serializado.
- No se modificaron reglas de combate, misión, XP, inventario, equipamiento, persistencia ni contratos de dominio.

## Métricas preliminares

No disponibles. El perfilado mediante ADB, logcat y Unity Profiler queda como observación pendiente. No se extrapolan métricas del Editor ni de las capturas como benchmark Android.

## Riesgos pendientes

- Registro del fabricante, modelo, versión Android y RAM del dispositivo.
- Perfilado por ADB, logcat y Unity Profiler.
- FPS, frame time, memoria, GC, draw calls, temperatura y batería.
- Validación instrumentada de arranques, minimización, menú de opciones y FPS/debug.

## Decisión final

**APROBADO CON OBSERVACIONES.** La evidencia física manual y las capturas confirman la presentación objetivo de H9. El perfilado por ADB, logcat y Unity Profiler queda pendiente. H10 no se inicia y se mantiene `v0.8.1 Alpha`.

## Seguimiento H9.2 — corrección previa a la validación física

H9.2 corrigió únicamente defectos de presentación identificados antes de volver al teléfono: safe area de controles superiores, franja inferior del viewport, inicialización del HUD, tamaño visual del joystick, densidad de paneles y prompts táctiles. También añadió validación automática de cámara, resoluciones, obstáculos y puntos de spawn.

La corrección quedó verificada con EditMode 23/23, PlayMode 11/11 y seis capturas físicas manuales con APK instalado. La evidencia visual cierra H9 como **APROBADO CON OBSERVACIONES**; ADB, logcat y Profiler siguen pendientes y H10 no se inicia.
