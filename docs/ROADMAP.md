# Lumbre de Nácar — Roadmap por hitos verificables

> Versión: 1.8 · Estado: H9 — APROBADO CON OBSERVACIONES; evidencia física manual incorporada · Fecha: 2026-07-21

Este roadmap separa la primera demo de la producción online. Cada hito tiene un resultado pequeño y un criterio de salida. H0 y el vertical slice reducido están aprobados; H1 está autorizado con Unity 6.3 LTS, Input System multiplataforma y Cinemachine.

## Regla de trabajo

Un hito no se considera terminado porque exista una escena o una clase. Debe poder demostrarse con una acción reproducible, una prueba o una medición. Si un hito falla, se corrige antes de añadir contenido.

## Primera demo offline

| Hito | Resultado | Complejidad | Dependencia |
| --- | --- | --- | --- |
| H0 — Aprobación de alcance | Vertical slice, dispositivo y criterios aceptados | Baja | Ninguna |
| H1 — Arranque técnico | Proyecto Unity fijado, estructura modular y escena vacía | Baja | H0 |
| H2 — Greybox isométrico | Plaza, sendero, cueva, grid, cámara y colisiones | Media | H1 |
| H3 — Control del jugador | Joystick Android, movimiento lógico y respawn | Media | H2 |
| H4 — Combat Prototype | Ataque básico, vida, daño, cooldown, IA y muerte | Alta | H3 |
| H4B — Habilidades y élite | Calor, defensa, área, telegraph y Mordeluz Resonante | Media | H4 |
| H5 — Misión y recompensa | NPC, misión, loot fijo, inventario y equipamiento | Media | H4B |
| H6 — Progresión y guardado | Nivel 1→2, save versionado y restauración | Media | H5 |
| H7 — Arte base y pulido | Arte original base, animaciones, VFX, audio, HUD y cámara | Media | H6 |
| H8 — Optimización, UX y preparación | Pausa, opciones, configuración local, HUD refinado y perfilado QA | Media | H7 |
| H8.1 — Arranque Android | Diagnóstico y corrección de pantalla negra; APK debug y checklist físico | Alta | H8 |
| H9 — Vertical Slice Polish | Cámara, UX móvil, HUD, menús y preparación Android | Media | H8.1 |
| H9.1 — Validación física y regresiones | Tests completos, APK, dispositivo Android, capturas y métricas | Alta | H9 |
| H9.2 — Corrección de layout Android | Safe area, viewport, HUD, joystick, prompts y spawns | Media | H9.1 |
| H10 — Demo candidata | Corrección de bugs y prueba con persona externa | Media | H9.2 |

### H0 — Aprobación de alcance

**Entregables:** `GDD_DRAFT.md`, `CRITICAL_REVIEW.md`, `DECISIONS.md`, dispositivo Android de referencia y definición de “demo aprobada”.

**Criterio de salida:** aprobación explícita del vertical slice reducido. Hasta entonces no se crea Unity ni código.

### H1 — Arranque técnico

**Entregables:** proyecto Unity 6.3 LTS, escenas mínimas, assemblies o módulos, configuración Android horizontal, Input System preparado para touch/teclado/mouse/gamepad, Cinemachine, carpeta de tests y contrato inicial de `IGameSession`.

**Criterio de salida:** el proyecto abre en la LTS resuelta, importa Input System/Cinemachine sin errores, las escenas Bootstrap y VerticalSlice cargan, el proyecto compila en batchmode, existe al menos un EditMode test y la configuración Android queda documentada. No hay sistemas H2.

**Resultado:** completado. Unity 6000.3.20f1 abre el proyecto; Bootstrap y VerticalSlice contienen la integración oficial de Cinemachine; Input System 1.19.0 y Test Framework 1.6.0 se resolvieron; la compilación batchmode terminó con código 0 y los 2 EditMode tests pasaron. El proyecto queda detenido antes de H2 a la espera de una nueva aprobación.

### H2 — Greybox isométrico

**Entregables:** una escena única con tres zonas, grid lógico, cámara horizontal, celdas transitables/bloqueadas y un punto de respawn.

**Criterio de salida:** la escena contiene plaza, sendero y cueva en geometría greybox agregada; el grid lógico conecta plaza y cueva evitando obstáculos; existen colisiones 2D; Cinemachine mantiene la cámara; no hay arte final ni sistemas de H3+.

**Resultado:** completado. `GreyboxLayout`, `WalkabilityGrid` y `GridPathfinder` viven en `Game.Domain`; `VerticalSlice.unity` usa tres meshes de zona, cuatro límites, seis colliders de celdas bloqueadas y una línea de preview de navegación. La compilación batchmode terminó con código 0 y los 4 EditMode tests pasaron. La medición de FPS/temperatura real queda para H9.

### H3 — Control del jugador

**Entregables:** Layers oficiales, Gizmos de depuración, joystick virtual Android, teclado para QA, movimiento basado en intención, colisiones 2D, seguimiento con Cinemachine y respawn local.

**Criterio de salida:** el avatar puede recorrer la ruta plaza → sendero → cueva y regresar a la plaza sin atravesar obstáculos ni quedar atrapado; el teclado se mantiene como entrada de QA y el joystick usa el mismo contrato de intención.

**Resultado:** completado. `MovementIntent` permanece en `Game.Domain`; el cliente adapta joystick/teclado a esa intención y `H3PlayerController` aplica el desplazamiento mediante `Rigidbody2D`. La escena contiene las Layers `WorldGround`, `WorldObstacle`, `Player`, `PlayerRespawn`, `NavigationDebug` y `PlayerUI`, además de Gizmos para grid, obstáculos, ruta BFS y respawn. La prueba PlayMode inyecta un gamepad virtual, recorre plaza → cueva → plaza y pasó; los 7 tests EditMode también pasaron.

### H4 — Combat Prototype

**Entregables:** Mordeluz, interfaces `IDamageable`, `IAttacker`, `ITargetable` e `IHealth`, ataque básico, vida, daño, cooldown, IA `Idle → Detect → Follow → Attack → Return`, muerte y feedback visual/sonoro provisional.

**Criterio de salida:** Mordeluz detecta al jugador, lo sigue dentro de su leash, ataca con cooldown, recibe daño, muere y deja de atacar; el jugador puede derrotarlo con ataques básicos y las pruebas automáticas permanecen en verde.

**Resultado:** completado. Los modelos de salud, daño, ataque y máquina de estados viven en `Game.Domain/Combat`; los adaptadores Unity viven en `Game.Client/Combat`. `VerticalSlice.unity` contiene un único Mordeluz y un botón móvil `ATK`. EditMode pasó 10/10; PlayMode pasó 2/2, incluyendo regresión del recorrido H3 y combate completo. No se añadieron habilidades adicionales ni sistemas de progresión.

### H4B — Habilidades y variante élite

**Entregables:** recurso Calor, defensa breve con reducción de daño, golpe de área con coste/radio/cooldown, `MordeluzResonanteController` reutilizando la base común, onda con anticipación visual y zona de peligro, feedback provisional y pruebas EditMode/PlayMode.

**Criterio de salida:** el jugador puede activar defensa y área respetando recurso, duración y cooldown; puede derrotar tres Mordeluz comunes y vencer al Resonante después de leer y sobrevivir a su telegraph; las reglas permanecen fuera de `MonoBehaviour` y todos los tests pasan.

**Resultado:** completado. `HeatResourceModel`, `DefenseAbilityModel`, `AreaAttackAbilityModel` y `ResonantWaveAttackModel` reciben `ICombatTimeSource`; `MordeluzResonanteController` reutiliza la máquina de estados, movimiento, colisiones y muerte de `MordeluzController`. EditMode pasó 14/14 y PlayMode 3/3. No se añadieron NPC, misión, inventario, loot, progresión, guardado, economía, networking ni arte final.

### H5 — Misión y recompensa

**Entregables:** Nara Velaquieta con interacción por proximidad; misión de derrotar tres Mordeluz y un Mordeluz Resonante; estados `Available → Active → ReadyToTurnIn → Completed`; progreso por eventos de dominio; recompensa fija; inventario de seis espacios; una ranura de equipamiento; HUD provisional y pruebas EditMode/PlayMode.

**Criterio de salida:** el jugador acepta la misión con Nara, derrota los tres Mordeluz y el Resonante, vuelve dentro del rango, recibe exactamente una recompensa, la ve en el inventario de seis espacios y la equipa en la ranura de reliquia. Una segunda interacción no duplica el objeto.

**Resultado:** completado. `DomainEventBus`, `H5MissionModel`, `InventoryModel` y `EquipmentModel` mantienen las reglas fuera de `MonoBehaviour`; los puentes de combate publican derrotas y la UI solo presenta el estado. La escena contiene Nara, botones `HABLAR`/`EQUIP`, HUD provisional y recompensa fija `Fragmento de Resonancia`. EditMode pasó 17/17 y PlayMode 4/4, incluyendo las regresiones H3, H4 y H4B. No se añadieron XP, niveles, guardado, crafting, tienda, economía, drops aleatorios, networking ni arte final.

### H6 — Progresión y guardado

**Entregables:** XP en dominio puro; nivel inicial 1 y máximo 2; XP por tres fuentes; eventos de XP y subida de nivel; HUD provisional; `ISaveRepository`; repositorio JSON versionado en `Game.Infrastructure.Local`; escritura temporal con reemplazo; carga automática, fallback para archivo inexistente/corrupto/incompatible, reset QA y restauración de misión, progreso, XP, nivel, inventario, equipamiento y posición segura.

**Criterio de salida:** completar el recorrido normal concede exactamente 100 XP y alcanza nivel 2; guardar, recargar la escena y restaurar recupera misión, progreso, XP, nivel, inventario, equipamiento y posición; enemigos reaparecidos no generan XP/progreso/recompensa duplicados.

**Resultado:** completado. `ExperienceModel` y `H6ProgressionModel` permanecen en `Game.Domain`; los DTOs no contienen referencias Unity; `ISaveRepository` vive en `Game.Application` y `JsonFileSaveRepository` en `Game.Infrastructure.Local`. La escena incorpora runtime y HUD provisional de nivel/XP. EditMode pasó 23/23 y PlayMode 5/5, incluyendo H3, H4, H4B, H5 y recarga real de H6. No se añadieron tienda, economía, monedas, crafting, profesiones, más niveles, nube, cuentas, networking, multiplayer ni arte final.

### H7 — Arte base, animaciones, audio y pulido

**Entregables:** arte base original del avatar, Nara, Mordeluz, Mordeluz Resonante y mundo; controladores Animator; animaciones de reposo, movimiento, ataque, daño, muerte, defensa, área y telegraph; VFX con pool; feedback de audio procedural, ambientes por zona, HUD provisional de vida/Calor/XP/misión/inventario/equipamiento; indicadores de Nara; cámara Cinemachine con shake y zoom de subida de nivel.

**Criterio de salida:** el recorrido completo usa el arte base sin Quads de greybox visibles; cada entidad tiene una silueta y estados animados legibles; ataque, daño, muerte, defensa, área, recompensa, equipamiento, nivel y telegraph producen feedback visual/sonoro; el pool respeta un presupuesto fijo; el HUD se lee en horizontal; el recorrido no cambia sus reglas de H3–H6.

**Resultado:** completado. `H7PresentationBuilder` reconstruye la presentación de forma idempotente, importa assets originales con transparencia y compresión móvil, configura seis controladores Animator, sustituye los visuales greybox, oculta solo la geometría de presentación anterior, mantiene colliders/grid/nav y agrega `H7PresentationRuntime`, `H7VfxPool`, audio por zonas, HUD y pulido de cámara. EditMode pasó 23/23 y PlayMode 6/6, incluyendo la regresión H3–H6 y la recarga de persistencia con la capa H7 presente. No se añadieron sistemas de gameplay, economía, VIP, networking ni arte final de producción.

### H8 — Optimización, UX y preparación del vertical slice

**Entregables:** optimización de sprites, Canvas, Animator, VFX y audio; HUD de estado legible; pausa y opciones; configuración local separada del save de partida; tooltips; overlay de FPS/frame time/memoria/GC/draw calls/CPU-GPU disponible para QA; builder idempotente y build Android de desarrollo reproducible.

**Criterio de salida:** el vertical slice conserva las reglas H3–H7, el jugador puede pausar, continuar, cambiar opciones, volver al juego y salir; música, FX, vibración, FPS, debug y calidad se guardan como configuración local; el HUD y los tooltips están configurados; el builder y las suites EditMode/PlayMode terminan en verde; no quedan errores críticos del proyecto. El build Android se genera correctamente. Las métricas de FPS sostenidos, temperatura y batería en un teléfono físico quedan explícitamente para H9 porque no había dispositivo conectado durante H8.

**Resultado:** completado. `H8OptimizationBuilder` aplica configuración de Canvas, texturas, Animator, VSync y presentación; `H8PauseController` ofrece pausa, opciones, versión visible, calidad y salida; `H8LocalSettings` persiste preferencias con `PlayerPrefs` sin tocar el save de H6; `H8PerformanceOverlay` expone métricas de QA mediante `ProfilerRecorder`; `H8Tooltip` añade ayuda contextual. El builder se ejecutó dos veces de forma idempotente, EditMode pasó 23/23, PlayMode pasó 7/7 y el APK de desarrollo Android se generó en `/tmp/lumbre-h8-android.apk`. No se añadieron sistemas de gameplay ni backend.

### H8.1 — Diagnóstico y corrección de pantalla negra Android

**Entregables:** causa raíz reproducible, loader Bootstrap → VerticalSlice, validación de Build Settings, trazas de arranque, fallback de cámara, settings/overlay no bloqueantes, APK debug ARM64/OpenGLES3 y checklist de hardware.

**Criterio de salida:** el APK debe mostrar VerticalSlice, jugador, cámara, HUD y controles después del splash en cinco arranques físicos: instalación limpia, segundo arranque, con save, después de minimizar y después de forzar cierre. La corrección no debe introducir gameplay nuevo.

**Resultado parcial:** la causa raíz quedó confirmada y corregida: Bootstrap era la escena 0 sin ningún cargador runtime. Builder, EditMode 23/23, PlayMode 8/8 y el APK `/tmp/lumbre-h8-1-black-screen-debug.apk` están verificados. No hay dispositivo ADB conectado, por lo que la instalación, logcat y los cinco arranques físicos siguen pendientes como puerta de aceptación Android de H9.

### H9 — Vertical Slice Polish

**Entregables:** cámara Cinemachine pulida con límites, safe area de `PlayerUI`, controles móviles reespaciados, HUD compacto, menús adaptables, feedback de selección y builder idempotente. La prueba física debe aportar las capturas Android y las métricas de FPS/memoria del dispositivo de referencia.

**Criterio de salida:** el slice conserva las reglas H3–H8, el avatar completa el recorrido sin zonas negras, la UI respeta safe area en relaciones horizontales objetivo, cámara/controles/HUD/menús pasan sus pruebas y el APK Android compila. La aceptación física de FPS, memoria, temperatura y batería requiere hardware conectado; no se declara cerrada sin esa evidencia.

**Resultado:** pulido de presentación implementado en v0.8.1 Alpha. El builder y `Validate` pasaron dos ejecuciones cada uno; EditMode pasó 23/23 y PlayMode 9/9. El APK ARM64/OpenGLES3 se generó correctamente y posteriormente fue instalado manualmente para la validación física documental.

### H9.1 — Validación física, regresiones y cierre condicionado

**Entregables:** XML verificables de EditMode/PlayMode, dos ciclos Build/Validate, APK ARM64/OpenGLES3, instalación limpia, arranques, recorrido, capturas Android y métricas preliminares.

**Criterio de salida:** todas las suites pasan, el APK instala y arranca en un teléfono físico, la UI respeta safe area, no hay zonas negras graves, los controles son utilizables y se registran capturas/métricas reales.

**Resultado histórico:** inicialmente BLOQUEADO por falta de dispositivo ADB. El estado se actualiza con la evidencia manual incorporada en H9.2; el perfilado ADB, logcat y Unity Profiler permanece pendiente.

### H9.2 — Corrección de layout Android

**Entregables:** controles superiores dentro de safe area, viewport completo sin franja inferior, HUD inicializado de forma determinista, joystick visual reducido sin perder área táctil, paneles compactos, prompts táctiles y validación de spawns.

**Criterio de salida:** Build/Validate es idempotente; la matriz 1920×1080, 2400×1080, 2340×1080, 2560×1080 y 1280×720 conserva cámara y UI dentro de los límites; EditMode y PlayMode pasan; el APK ARM64/OpenGLES3 compila; la validación visual física queda documentada.

**Resultado:** correcciones implementadas y verificadas automáticamente con dos ciclos Build → Validate, EditMode 23/23, PlayMode 11/11 y APK ARM64/OpenGLES3 generado en `Builds/Android/LumbreDeNacar-v0.8.1-H9.2.apk`. El APK fue instalado manualmente y seis capturas físicas confirman safe area, HUD, cámara y controles. H9 queda **APROBADO CON OBSERVACIONES**; ADB, logcat y Unity Profiler siguen pendientes. No se cambia gameplay, versión ni se inicia H10.

### H10 — Demo candidata

**Entregables:** build reproducible, checklist de aceptación, registro de bugs, instrucciones de prueba y changelog.

**Criterio de salida:** se cumplen todos los criterios de `CRITICAL_REVIEW.md`, ningún bug bloquea el recorrido y el responsable del producto aprueba pasar a la siguiente decisión.

## Puerta posterior a la demo

Solo después de H10 se decide si vale la pena invertir en online. La primera prueba online deberá ser un spike separado y pequeño:

1. Contratos de comando/evento y `RemoteSession`.
2. Servidor autoritativo con una escena y pocos jugadores.
3. Movimiento validado y snapshots.
4. Combate contra un enemigo.
5. Persistencia de una recompensa fija.
6. Reconexión, rate limits y auditoría mínima.

No se construyen clanes, mercado, monetización, temporadas ni web antes de que este spike pruebe que la autoridad y la persistencia funcionan.

## Producción posterior, aún no aprobada

- **Slice online:** cuentas de prueba, movimiento, combate y persistencia con pocos jugadores.
- **MVP online:** una ciudad, una zona PvE, una mazmorra corta, cinco criaturas y cinco misiones.
- **Beta Android:** seguridad, compatibilidad, soporte, telemetría consentida, actualización y pruebas de carga.
- **Portal oficial:** estado, noticias, guías, biblioteca/wiki y mapa interactivo.
- **Lanzamiento gradual:** AAB firmado fuera del repositorio, pruebas cerradas/abiertas, operación y rollback.

Cada etapa posterior requiere una nueva revisión de alcance. No es un compromiso de construir todos los sistemas de la visión inicial.

## Prioridad de recorte

1. Control y cámara.
2. Combate y feedback.
3. Misión, recompensa y guardado.
4. Legibilidad móvil y rendimiento.
5. Arte y audio.
6. Cualquier sistema adicional.
