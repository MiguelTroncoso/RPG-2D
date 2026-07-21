# Lumbre de Nácar — H9.1 Validación física, regresiones y cierre de H9

## Estado de decisión

**BLOQUEADO**

La validación automatizada, el builder H9 y el APK Android están verificados. La aceptación física no puede cerrarse en esta estación porque no hay un teléfono Android conectado a ADB.

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
- Dispositivo: no disponible
- Fabricante/modelo: no disponible
- Versión Android: no disponible
- Resolución y aspect ratio: no disponible
- RAM: no disponible
- Orientación física: no disponible

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

## Validación física

No se pudo ejecutar:

- desinstalación e instalación limpia;
- primer y segundo arranque;
- arranque con save;
- minimizar/restaurar;
- force close y reapertura;
- pausa, opciones, sliders y toggles;
- joystick, ATK, DEF y AOE;
- interacción con Nara y equipamiento;
- recorrido plaza–sendero–cueva;
- combate en hardware;
- captura de logcat;
- capturas reales;
- medición de FPS, frame time, memoria, GC, draw calls, temperatura y batería.

`adb devices -l` devuelve una lista vacía. No se presentan renders del Editor como evidencia física.

## Capturas

- Física Android: ninguna; bloqueada por falta de dispositivo.
- Render de referencia: [`h9-android-layout-preview.png`](captures/h9-android-layout-preview.png). No representa hardware físico.
- Carpeta reservada para evidencia real: [`docs/captures/android-h9/`](captures/android-h9/).

## Bug encontrado y corrección

La prueba PlayMode H9 detectó que `H7CameraPolish.IsPolished` era falso durante el runtime: `H3CinemachineFollowTarget.Awake` restauraba `followOffset` a `{x:0,y:0,z:-20}`, sobrescribiendo la composición H9 `{x:0,y:0.65,z:-20}`.

Corrección permitida y aplicada:

- `H9VerticalSlicePolishBuilder` sincroniza el offset en `H3CinemachineFollowTarget` además de `CinemachineFollow`.
- La escena conserva el offset H9 serializado.
- No se modificaron reglas de combate, misión, XP, inventario, equipamiento, persistencia ni contratos de dominio.

## Métricas preliminares

No disponibles. No se ejecutó la medición de diez minutos porque no hay hardware conectado. No se extrapolan métricas del Editor ni del APK como benchmark Android.

## Riesgos pendientes

- Arranque real Bootstrap → VerticalSlice en teléfono.
- Safe Area en la relación de aspecto del dispositivo objetivo.
- Zonas negras y encuadre bajo notch o barra de navegación.
- Ergonomía y visibilidad de joystick, ATK, DEF, AOE, HABLAR y EQUIP.
- Menú de opciones, sliders, toggles y FPS/debug en hardware.
- Estabilidad durante cinco arranques y minimización.
- FPS, frame time, memoria, GC, draw calls, temperatura y batería.
- Logcat de errores Android.

## Decisión final

**BLOQUEADO.** H9.1 no se declara aprobado porque la prueba física obligatoria no pudo ejecutarse. El repositorio queda preparado para revisión técnica, sin iniciar H10 y manteniendo `v0.8.1 Alpha`.

## Seguimiento H9.2 — corrección previa a la validación física

H9.2 corrigió únicamente defectos de presentación identificados antes de volver al teléfono: safe area de controles superiores, franja inferior del viewport, inicialización del HUD, tamaño visual del joystick, densidad de paneles y prompts táctiles. También añadió validación automática de cámara, resoluciones, obstáculos y puntos de spawn.

La corrección quedó verificada con EditMode 23/23 y PlayMode 11/11. La prueba física, las capturas reales y las métricas de hardware siguen pendientes; por tanto, este documento no cambia el estado de decisión de H9.1 ni autoriza el inicio de H10.
