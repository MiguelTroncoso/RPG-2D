# Changelog técnico

## 2026-07-21 — H10 control del jugador, locomoción y combate táctil — v0.8.1 Alpha

### Incluido

- Causa raíz corregida: las acciones táctiles se muestreaban únicamente con `WasPressedThisFrame`, mientras `OnScreenButton` entrega el estado mediante un dispositivo virtual en el siguiente update; además no existía una frontera común para foco y pausa.
- `H3PlayerInputReader` ahora mantiene un latch de `InputAction.started`, reinicia los dispositivos virtuales `OnScreen` al perder foco y entrega la misma intención a joystick, teclado, mouse/gamepad y touch.
- `MovementIntent`, `PlayerLocomotionModel` y `LocomotionVelocity` separan dead zone, respuesta analógica, aceleración, desaceleración, normalización diagonal y límite de velocidad fuera de `MonoBehaviour`.
- El jugador conserva una dirección de mirada estable y el movimiento sigue usando `Rigidbody2D` continuo, colisiones existentes y respawn H3.
- `PlayerActionStateModel` y `H10PlayerActionStateController` bloquean acciones incompatibles durante pausa, muerte o transición de acción sin mover reglas de combate al cliente de presentación.
- ATK, DEF, AOE, interacción y equipamiento usan los mismos contratos para touch y QA; los botones H10 agregan únicamente feedback visual de pulsación.
- La selección de ataque mantiene continuidad temporal del objetivo durante una cadencia corta para evitar retargeting accidental causado por el movimiento del enemigo, sin crear un lock persistente.
- Se agregaron 4 pruebas EditMode y 24 pruebas PlayMode de locomoción, dirección, colisión, botones, acciones, pausa, safe area, bootstrap y regresiones H3–H9.

### Validación

- EditMode: 27/27 pasados, 0 fallidos, 0 ignorados. XML: `/tmp/h10-editmode-1.xml`.
- PlayMode: 35/35 pasados, 0 fallidos, 0 ignorados. XML: `/tmp/h10-playmode-1.xml`.
- `H10PlayerControlBuilder.Build` y `Validate` ejecutados en ciclos repetidos con código de salida 0; no se duplicaron componentes, controles ni objetos de escena.
- APK generado con ARM64, OpenGLES3, landscape, Development Build, Allow Debugging y ConnectWithProfiler: `Builds/Android/LumbreDeNacar-v0.8.1-H10.apk`, 76,139,085 bytes, SHA-256 `a78cf8d9aaefc9703032860023cc690cea19ddeceb641640125a3e236ff3004f`.
- No se realizó todavía instalación ni validación en teléfono físico; ADB/logcat/Profiler y las métricas de hardware permanecen pendientes.

### Estado H10

**IMPLEMENTADO CON OBSERVACIONES:** la cobertura automática y el APK están verificados, pero H10 no se declara aprobado hasta completar la validación física Android. Se mantiene `v0.8.1 Alpha` y no se inicia H11.

### Límite explícito

H10 no añade enemigos, mapas, NPC, misiones, habilidades, economía, loot, crafting, networking, servidor, multiplayer, nuevas reglas de daño, progresión, inventario, equipamiento, persistencia, guardado ni balance. Mantiene los contratos H3–H9.

## 2026-07-21 — H9.2 corrección de layout Android — v0.8.1 Alpha

### Incluido

- Controles superiores reanclados dentro de la safe area; la matriz de resolución validada cubre 1920×1080, 2400×1080, 2340×1080, 2560×1080 y 1280×720.
- Backdrop ampliado uniformemente y volumen `H9_CameraBounds` ajustado para eliminar el hueco inferior sin estirar la escena ni cambiar gameplay.
- HUD H7 resuelto durante `Awake`/`Start` y refrescado después de cargar o reanudar; se añadió una regresión PlayMode para el snapshot inicial.
- Área táctil del joystick conservada en 228 px, con visual reducido a 168 px y handle de 96 px.
- Paneles H7 compactados manteniendo legibilidad y prompts táctiles adaptados a `HABLAR CON NARA`/`EQUIPAR RECOMPENSA`, mientras teclado conserva sus teclas QA.
- `H9VerticalSlicePolishBuilder.Validate` comprueba duplicados, viewport completo, safe area, obstáculos y spawns dentro del mundo.

### Validación

- EditMode: 23/23 pasados, 0 fallidos, 0 ignorados. XML: `/tmp/lumbre-h9-2-editmode.xml`.
- PlayMode: 11/11 pasados, 0 fallidos, 0 ignorados. XML: `/tmp/lumbre-h9-2-playmode.xml`.
- Build → Validate ejecutado dos veces en batchmode, ambas con código 0 y sin duplicaciones.
- APK Android generado en `Builds/Android/LumbreDeNacar-v0.8.1-H9.2.apk`, 76,097,309 bytes, SHA-256 `a88d21b3d1a924569b59772654d8a46f79812ab7c005b4e6d30b2ca7abaefd6d`; permanece ignorado y no se sube.
- APK instalado manualmente en un teléfono Android y seis capturas físicas incorporadas en `docs/captures/android-h9.2-physical/`.
- La evidencia visual confirma safe area, HUD, cámara y controles táctiles.
- ADB, logcat y Unity Profiler continúan pendientes; no se presentan métricas de rendimiento inferidas.

### Cierre H9

**APROBADO CON OBSERVACIONES:** H9 queda cerrado documentalmente con evidencia física manual. H10 no se inicia y se mantiene `v0.8.1 Alpha`.

### Límite explícito

H9.2 modifica únicamente presentación, safe area, viewport, HUD, controles y validación de spawns. No cambia reglas de combate, IA, misión, progresión, inventario, persistencia, contratos de dominio, versión ni inicia H10.

## 2026-07-21 — H9.1 validación automatizada y cierre condicionado — v0.8.1 Alpha

### Incluido

- Se corrigió la sincronización del offset vertical H9 entre `H9VerticalSlicePolishBuilder`, `H3CinemachineFollowTarget` y `H7CameraPolish`; el runtime ya no restaura la composición antigua `{y:0}`.
- El runner documentado de Unity usa `-runTests` sin `-quit`, permitiendo que el Test Framework genere el XML antes de cerrar.
- Builder H9 ejecutado dos veces y `Validate` ejecutado dos veces sin duplicar Safe Area, confiner, CameraBounds, HUD, botones ni componentes.
- Se añadió el registro de H9.1 con resultados, rutas XML, APK, bloqueo de hardware y procedimiento pendiente.

### Validación

- EditMode: 23/23 pasados, 0 fallidos, 0 ignorados. XML: `/tmp/lumbre-h9-editmode-final.qZqdTZ/results.xml`.
- PlayMode: 9/9 pasados, 0 fallidos, 0 ignorados; incluye regresiones H3, H4, H4B, H5, H6, H7, H8.1, H8 y H9. XML: `/tmp/lumbre-h9-playmode-final.lMz5Vs/results.xml`.
- APK Android generado correctamente: `/tmp/lumbre-h9-android.apk`, ARM64/OpenGLES3, Development/Debug/Profiler, 1,023,031,188 bytes.
- `adb devices -l` no detectó dispositivos; no se ejecutaron instalación, arranques, capturas físicas ni métricas de hardware.

### Estado H9.1 (histórico)

**INICIALMENTE BLOQUEADO:** la evidencia automatizada y el APK estaban verificados, pero faltaba evidencia física. El bloqueo queda resuelto documentalmente por H9.2 con instalación manual y capturas físicas; el perfilado ADB/logcat/Profiler continúa pendiente.

## 2026-07-21 — H9 pulido del vertical slice para Android — v0.8.1 Alpha

### Incluido

- Cámara Cinemachine con composición más cómoda, seguimiento amortiguado, zoom de presentación y límites para evitar zonas negras.
- Safe area dedicada para toda la UI de `PlayerUI`, manteniendo CanvasScaler adaptable en landscape.
- Joystick ampliado y controles móviles reespaciados con transiciones de selección más claras.
- HUD compacto de estado/misión y panel contextual de interacción, con menos espacio desperdiciado.
- Pausa y opciones reacomodadas para resoluciones Android horizontales, conservando la configuración local H8.
- `H9VerticalSlicePolishBuilder` idempotente y `H9VerticalSlicePolishPlayModeTests` para cámara, HUD, controles y safe area.
- Versión del proyecto actualizada a v0.8.1 Alpha y documentación H9 actualizada.

### Validación

- Builder H9 ejecutado en batchmode y validación de escena completada con código de salida 0.
- La primera invocación con `-quit` cerraba Unity antes de terminar el runner; la ejecución correcta se documenta en H9.1 con `-runTests` sin `-quit`.
- Build Android físico y capturas de dispositivo quedan pendientes: `adb devices -l` no detectó hardware conectado.

### Límite explícito

H9 modifica únicamente presentación, UX y preparación Android. No añade enemigos, NPC, mapas, habilidades, misiones, loot, economía, networking ni backend.

## 2026-07-20 — H8.1 diagnóstico y corrección de pantalla negra Android — v0.8.0 Alpha

### Incluido

- Causa raíz confirmada: `Bootstrap` era la escena 0, pero no cargaba `VerticalSlice`.
- `BootstrapSceneLoader` con carga asíncrona, progreso y trazas `[BOOT]` de diagnóstico.
- Validación automática de Build Settings y configuración del loader desde `H8OptimizationBuilder`.
- Fallback de cámara estática si la cámara oficial/Cinemachine no está disponible.
- Sanitización de `PlayerPrefs`, inicialización segura de pausa y `ProfilerRecorder` opcional no bloqueante.
- Prueba PlayMode de arranque Bootstrap → VerticalSlice y APK debug ARM64/OpenGLES3.

### Validación

- EditMode: 23/23 pasados.
- PlayMode: 8/8 pasados.
- APK generado: `/tmp/lumbre-h8-1-black-screen-debug.apk`.
- No se detectaron dispositivos ADB; instalación, logcat y cinco arranques físicos quedan pendientes.

### Límite explícito

H8.1 corrige la ruta de arranque sin añadir gameplay. H9 no puede comenzar hasta confirmar el arranque en un Android físico.

## 2026-07-20 — H8 optimización, UX y preparación del vertical slice completado — v0.8.0 Alpha

### Incluido

- `H8OptimizationBuilder` idempotente para reconstruir la presentación H8 y conservar el scene graph y los contratos de H3–H7.
- Configuración de rendimiento: CanvasScaler a resolución de referencia, Canvas sin canales adicionales, texturas finales sin mipmaps/readable, compresión móvil, Animator con culling y VSync desactivado.
- `H8PauseController` con pausa, continuar, opciones, calidad gráfica, salida, versión visible y controles de música/FX, vibración, FPS y debug.
- `H8LocalSettings` con preferencias de presentación separadas del save de partida H6.
- `H8PerformanceOverlay` con FPS, frame time, memoria, GC y draw calls para QA mediante `ProfilerRecorder`.
- Tooltips contextuales para volumen, gráficos, vibración y diagnósticos; el HUD H7 reduce refrescos de texto a 10 Hz sin perder el pulso visual.
- Builder de APK Android de desarrollo reproducible con `BuildOptions.Development | BuildOptions.ConnectWithProfiler`.

### Validación H8

- Builder H8 ejecutado dos veces en batchmode: código de salida 0 en ambas ejecuciones.
- EditMode: 23 tests pasados, 0 fallidos.
- PlayMode: 7 tests pasados, 0 fallidos; incluye regresiones H3, H4, H4B, H5, H6 y H7, además de pausa, opciones, persistencia local, tooltips y overlay.
- APK Android de desarrollo generado en `/tmp/lumbre-h8-android.apk` sin errores de compilación del proyecto.

### Límite explícito

No había un dispositivo Android conectado (`adb devices` no mostró dispositivos), por lo que FPS sostenidos, temperatura, batería, memoria térmica y tiempos reales de carga quedan pendientes de H9. No se añadieron gameplay, economía, VIP, backend, networking ni arte final.

## 2026-07-20 — H7 arte base, animaciones, audio y pulido completado — v0.7.0 Alpha

### Incluido

- Arte base original con transparencia para Bastión de Brasa, Nara Velaquieta, Mordeluz, Mordeluz Resonante y el recorrido plaza–sendero–cueva.
- Controladores Animator y clips de reposo, movimiento, ataque, daño, muerte, defensa, área, telegraph y conversación/estado de misión.
- `H7PresentationRuntime` como composición de feedback sin mover reglas de gameplay a la presentación.
- Pool de VFX prewarmado con límite de 18 elementos iniciales y 24 activos; efectos de ataque, defensa, área, impacto, muerte, misión, equipamiento y nivel.
- Audio procedural de pasos, ataque, impacto, muerte, misión, equipamiento, nivel, UI y tres ambientes con crossfade por zona.
- HUD provisional de vida, Calor, XP, misión, inventario y reliquia equipada; indicador de misión para Nara.
- Pulido de Cinemachine con shake breve y zoom de subida de nivel; el seguimiento oficial no cambia.
- Builder H7 idempotente y prueba PlayMode de configuración y persistencia visual después de recargar la escena.

### Validación H7

- Builder H7 en batchmode: código de salida 0.
- EditMode: 23 tests pasados, 0 fallidos.
- PlayMode: 6 tests pasados, 0 fallidos; incluye regresiones H3, H4, H4B, H5, H6 y la nueva verificación H7.

### No incluido

No se añadieron nuevas reglas de combate, NPC, misiones, inventario, loot, XP, niveles, economía, VIP, crafting, tienda, networking, servidor, multiplayer ni arte final de producción.

## 2026-07-20 — H6 progresión y persistencia local completado — v0.6.0 Alpha

### Incluido

- `ExperienceModel` con nivel inicial 1, máximo 2, umbral de 100 XP y deduplicación por `sourceId`.
- XP por Mordeluz, Mordeluz Resonante y misión completada: 10 + 30 + 40 XP.
- `ExperienceGainedEvent` y `LevelUpEvent` en dominio puro; HUD provisional de nivel y barra.
- DTOs versionados para experiencia, misión, inventario, equipamiento y posición segura.
- `ISaveRepository` y `JsonFileSaveRepository` local con JSON, archivo temporal y reemplazo seguro.
- Fallback no bloqueante para archivo inexistente, corrupto o esquema incompatible.
- Carga automática, guardado por cambios importantes, pausa/salida y teclas QA `F5`/`F9`.
- Restauración de misión, IDs de derrotas, XP, IDs de fuentes aplicadas, inventario, reliquia equipada y posición.

### Validación H6

- Builder H6 en batchmode: código de salida 0.
- EditMode: 23 tests pasados, 0 fallidos.
- PlayMode: 5 tests pasados, 0 fallidos; incluye regresiones H3, H4, H4B, H5 y recarga real de la escena.

### No incluido

No se añadieron tienda, economía, monedas, crafting, profesiones, atributos avanzados, más niveles, varias partidas, nube, cuentas, login, networking, servidor, multiplayer ni arte/audio final.

## 2026-07-20 — H5 misión y recompensa completado

### Incluido

- Nara Velaquieta con interacción por proximidad y controles `F`/`buttonNorth`.
- Misión de derrotar tres Mordeluz y un Mordeluz Resonante con estados `Available`, `Active`, `ReadyToTurnIn` y `Completed`.
- `DomainEventBus` y `CombatantDefeatedEvent` para progresión desacoplada del combate.
- Deduplicación por `EntityId` para no contar dos veces la misma derrota.
- Recompensa fija `Fragmento de Resonancia`, inventario de seis espacios y ranura de equipamiento `Relic`.
- Entrega idempotente: una segunda interacción no duplica la recompensa.
- HUD provisional de estado, progreso, inventario y equipamiento; botón `G/EQUIP`.

### Validación H5

- Builder H5 en batchmode: código de salida 0.
- EditMode: 17 tests pasados, 0 fallidos.
- PlayMode: 4 tests pasados, 0 fallidos; incluye regresiones H3, H4, H4B y el flujo completo de misión.

### No incluido

No se añadieron XP, niveles, guardado, crafting, tienda, economía, drops aleatorios, networking ni arte final.

## 2026-07-19 — H4B habilidades y variante élite completado

### Incluido

- `HeatResourceModel` con límite, ganancia por ataque básico y gasto de Calor.
- Defensa breve con reducción de daño, duración y cooldown mediante `IDamageModifier`.
- Ataque de área con coste, radio, cooldown y feedback visual provisional.
- `MordeluzResonanteController` reutilizando la IA común de Mordeluz.
- Onda con anticipación visual, telegraph y resolución limitada a la zona de peligro.
- Acciones `Defense`/`AreaAttack` para teclado, gamepad y botones móviles; HUD provisional de Calor.
- `ICombatTimeSource` inyectable para probar reglas temporales fuera de `MonoBehaviour`.

### Validación H4B

- Builder H4B en batchmode: código de salida 0.
- EditMode: 14 tests pasados, 0 fallidos.
- PlayMode: 3 tests pasados, 0 fallidos; regresión H3, H4 y demo H4B completa.

### No incluido

No se añadieron NPC, misión, inventario, equipamiento, loot, XP, niveles, guardado, economía, networking ni arte final.

## 2026-07-19 — H4 Combat Prototype completado

### Incluido

- Interfaces reutilizables `IHealth`, `IDamageable`, `ITargetable` e `IAttacker` en `Game.Domain/Combat`.
- Modelos puros de salud, daño, ataque/cooldown y máquina de estados de Mordeluz.
- Un único enemigo `Mordeluz` con estados `Idle`, `Detect`, `Follow`, `Attack`, `Return` y `Dead`.
- Ataque básico del jugador con teclado `Space`, gamepad `buttonSouth` y botón móvil `ATK`.
- Vida, daño acotado, cooldown, muerte terminal, flash/pulso/desaparición y tonos procedurales.
- Layer `Enemy` 14; el enemigo no bloquea físicamente al jugador, pero conserva detección y alcance de ataque.

### Validación H4

- Builder H4 en batchmode: código de salida 0.
- EditMode: 10 tests pasados, 0 fallidos.
- PlayMode: 2 tests pasados, 0 fallidos; regresión H3 y combate H4.

### No incluido

No se añadieron habilidades adicionales, loot, NPC, inventario, misiones, progresión, élite, economía, guardado, networking ni monetización.

## 2026-07-19 — H3 control del jugador completado

### Incluido

- Layers oficiales: `WorldGround` 8, `WorldObstacle` 9, `Player` 10, `PlayerRespawn` 11, `NavigationDebug` 12 y `PlayerUI` 13.
- Gizmos de Editor para grid isométrico, obstáculos, ruta BFS y punto de respawn.
- `MovementIntent` puro en `Game.Domain`, con deadzone, clamp y conversión isométrica en el cliente.
- Joystick Android con `OnScreenStick`; teclado WASD y tecla `R` para QA; gamepad disponible para pruebas.
- Movimiento con `Rigidbody2D`, colisiones del greybox, respawn local y seguimiento mediante Cinemachine oficial.
- Paquete oficial `com.unity.ugui` para Canvas, EventSystem y módulo de UI del Input System.

### Validación H3

- Builder de escena H3 en batchmode: código de salida 0.
- EditMode: 7 tests pasados, 0 fallidos.
- PlayMode: 1 test pasado, 0 fallidos; el avatar recorrió plaza → cueva → plaza con input virtual.

### No incluido

No se añadieron combate, NPC, inventario, misiones, daño, interacción, arte final, guardado, networking ni economía.

## 2026-07-19 — H1 completado

### Alcance aprobado

- H0 y vertical slice reducido aprobados por el usuario.
- H1 autorizado con Unity 6 LTS estable más reciente, Input System multiplataforma y Cinemachine desde el inicio.

### Comandos relevantes

- Consulta de editores LTS disponibles en Unity Hub headless.
- Instalación de Unity 6000.3.20f1 arm64 con Android Build Support, SDK, NDK, OpenJDK y CMake.
- Creación del proyecto con Unity en `/Users/migueltroncoso/Documents/Juego 2D`.
- Consulta del registro oficial de paquetes para Cinemachine, Input System y Test Framework.
- Generación posterior de escenas H1 mediante `H1SceneBuilder.Build`.
- Verificación batchmode con código de salida 0.
- Ejecución de EditMode: 2 tests, 2 pasados, 0 fallidos.

### Archivos añadidos o actualizados

- `.gitignore` de Unity.
- `Packages/manifest.json` con Cinemachine, Input System y Test Framework; se eliminó Multiplayer Center por no ser necesario en H1.
- `Assets/Game/Domain`, `Application`, `Client`, `Infrastructure/Local` y `Tests`.
- `Assets/Game/Client/Input/PlayerInputActions.inputactions`.
- `Assets/Editor/H1SceneBuilder.cs`.
- `docs/TDD.md`, `docs/ARCHITECTURE.md`, `docs/ANDROID_BUILD.md`, `docs/TESTING.md`.
- `README.md`, `docs/ROADMAP.md`, `docs/DECISIONS.md`.

### Decisiones registradas

- `DEC-0017`: política de Unity 6.3 LTS y revisión efectiva reproducible.
- `DEC-0018`: Input System preparado para Touch, Keyboard&Mouse y Gamepad.
- `DEC-0019`: Cinemachine 3 como sistema oficial de cámara.

### Validación H1

- `Game.Domain` no referencia `UnityEngine`.
- `Bootstrap.unity` y `VerticalSlice.unity` están en Build Settings.
- Ambas escenas contienen `CinemachineBrain` y `CinemachineCamera`.
- Android queda en landscape con orientación por defecto horizontal y portrait deshabilitado.
- El marcador de escena se mantiene en `Game.Client`; no hay componentes editor-only en las escenas runtime.

### No incluido

No se añadieron combate, inventario, misiones, guardado, red, cuentas, economía, VIP, monetización, Addressables ni assets externos.

## 2026-07-19 — H2 greybox completado

### Incluido

- `Assets/Game/Domain/Constants/ProjectConstants.cs` como fuente única de constantes estructurales.
- `Assets/Game/Config/README.md` como carpeta reservada para configuración futura.
- Grid lógico 32×20, tres zonas, obstáculos y BFS de navegación sin `UnityEngine`.
- `VerticalSlice.unity` regenerada con tres meshes agregadas, colisiones 2D, cámara Cinemachine y preview de ruta.
- Tests EditMode para ruta plaza→cueva y zonas aprobadas.
- Compilación batchmode con código 0; 4 tests EditMode pasados, 0 fallidos.

### Fuera de alcance

No se añadieron arte final, avatar, joystick, combate, NPC, inventario, misiones, guardado, networking ni economía.
