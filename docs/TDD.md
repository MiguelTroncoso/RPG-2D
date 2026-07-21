# Lumbre de Nácar — TDD de H8

> Estado: H8 completado · alcance autorizado: optimización, UX, configuración local, herramientas de QA y preparación Android.

## Objetivo

H8 optimiza y completa la capa de presentación del slice estable de H7 sin cambiar sus reglas. No valida crafting, tienda, economía, monedas, profesiones, atributos avanzados, más niveles, varias partidas, nube, cuentas, login, networking, servidor, multiplayer ni arte final de producción. La medición sobre hardware Android real es una responsabilidad posterior de H9.

## Línea técnica

- Unity 6.3 LTS, última LTS estable disponible en Unity Hub al comenzar H1.
- Revisión efectiva del entorno: `6000.3.20f1`.
- Plataforma inicial: Android en landscape.
- Lenguaje: C#.
- Render: configuración mínima del proyecto; no se incorpora URP ni otro paquete de render durante H1.
- Cámara: Cinemachine 3.1.7 como integración oficial.
- Entrada: Input System 1.19.0; acciones preparadas para Touch, Keyboard&Mouse y Gamepad.
- Tests: Unity Test Framework 1.6.0, disponible como paquete integrado en esta línea de Unity, con una prueba EditMode de arquitectura y sesión local.

Las versiones de paquetes son las resoluciones actuales del proyecto. La política de producto sigue siendo actualizar la LTS de forma deliberada y repetir la validación cuando corresponda.

## Estructura

```text
Assets/Game/
  Config/                  # configuración editable futura; vacía en H2
  Domain/                 # C# puro; sin UnityEngine
    Constants/             # constantes estructurales compartidas
    Combat/                # salud, daño, ataque e IA puros
    Events/                # bus y eventos de dominio
    Inventory/             # seis espacios y modelo de equipamiento
    Missions/              # estados, progreso y entrega idempotente
    Persistence/           # DTOs puros del save, sin JSON ni archivos
    Progression/           # XP, nivel, deduplicación y eventos
    Navigation/            # grid, zonas y pathfinding puros
  Application/            # contratos de sesión y comandos
  Client/                 # presentación, cámara e input futuro
    Input/
    Camera/
    Combat/                # adaptadores Unity del prototipo
    Missions/              # Nara, puentes de eventos y HUD provisional
  Infrastructure/
    Local/                 # sesión local y repositorio JSON reemplazables
  Scenes/
  Tests/
    EditMode/
    PlayMode/
Assets/Editor/             # herramienta de construcción de escenas H1
docs/
Packages/
ProjectSettings/
```

## Reglas de dependencia

```text
Game.Domain  <- Game.Application <- Game.Infrastructure.Local
      ^               ^                    ^
      |               |                    |
      +----------- Game.Client ------------+
```

`Game.Domain` y `Game.Application` no referencian `UnityEngine`. El cliente puede depender de Unity y de paquetes de presentación, pero las reglas del mundo no entran en `MonoBehaviour`. La futura sesión remota implementará `IGameSession` fuera de la capa de presentación; H1 no la crea.

## Contrato inicial

`IGameSession` expone únicamente estado de sesión, inicio, detención, envío de comandos y eventos. No contiene métodos de combate, inventario ni misión. Los comandos y eventos se mantienen como mensajes extensibles para que una sesión local y una futura sesión autoritativa compartan forma, no autoridad.

`Game.Client` referencia `Game.Infrastructure.Local` únicamente para componer el slice offline (`JsonFileSaveRepository`). La regla de gameplay y los DTOs siguen fuera de Unity; una composición posterior puede sustituir esa referencia por un adaptador remoto.

## Cámara e input

- Las escenas tienen una cámara Unity con `CinemachineBrain`.
- Cada escena tiene una `CinemachineCamera` oficial como punto de integración.
- `PlayerInputActions.inputactions` declara `Move`, `Attack`, `Defense`, `AreaAttack`, `Point`, `Click`, `Submit` y `Cancel`.
- Android/touch usa `OnScreenStick` sobre un Canvas y `InputSystemUIInputModule`; el stick virtual publica el control `<Gamepad>/leftStick`.
- El teclado usa WASD y la tecla `R` para respawn durante QA/editor. El gamepad se mantiene disponible para pruebas automatizadas y QA.
- Joystick y teclado pasan por `H3PlayerInputReader` y producen `MovementIntent`; el dominio no conoce dispositivos concretos.
- El teclado usa `Space`, el gamepad usa `buttonSouth` y el botón móvil `ATK` publica el mismo control virtual.
- Defensa usa `Q`/`buttonEast` y área usa `E`/`buttonWest`; los botones móviles publican esos controles mediante `OnScreenButton`.
- Interacción usa `F`/`buttonNorth` y equipamiento usa `G`/`leftShoulder`; los botones móviles publican esos controles mediante `OnScreenButton`.

## Combat Prototype H4

- `IHealth` expone salud actual, máxima y estado vivo.
- `IDamageable` recibe `CombatDamage` y devuelve `DamageResult`; el daño se limita a la salud restante y la muerte es terminal.
- `ITargetable` expone la salud y el receptor de daño de una entidad válida.
- `IAttacker` aplica daño mediante `BasicAttackerModel`, que controla el cooldown y rechaza objetivos inválidos o ataques demasiado rápidos.
- `MordeluzAiStateMachine` es puro y transita `Idle → Detect → Follow → Attack → Return`; `Dead` es un estado terminal.
- `MordeluzController` mueve el Rigidbody2D, ataca al jugador dentro de rango y devuelve su posición al spawn si pierde el objetivo.
- `H4CombatFeedback` ejecuta flash, pulso, desaparición y tonos procedurales de golpe/ataque/muerte.

## Habilidades y élite H4B

- `ICombatTimeSource` abstrae el reloj; las pruebas usan una fuente manual y el cliente usa `UnityCombatTimeSource`.
- `HeatResourceModel` limita Calor, gasto y recarga por ataques básicos; no depende de Unity.
- `DefenseAbilityModel` controla activación, duración, cooldown y reducción mediante `IDamageModifier`.
- `AreaAttackAbilityModel` valida coste, cooldown y parámetros; el cliente resuelve objetivos por `Physics2D`.
- `ResonantWaveAttackModel` separa inicio, anticipación y resolución de la onda; `H4BWaveTelegraph` solo presenta la zona de peligro.
- `MordeluzResonanteController` hereda la IA común y solo reemplaza la ejecución del ataque por la onda.
- `H4BPlayerAbilityController`, `H4BAbilityFeedback` y `H4BAbilityHud` son adaptadores de input, feedback y UI; no definen reglas alternativas.

## Misión y recompensa H5

- `DomainEventBus` publica `CombatantDefeatedEvent` sin conocer Unity. `H5MissionModel` escucha derrotas, filtra por tipo y avanza el progreso solo en estado `Active`.
- La misión expone los estados `Available`, `Active`, `ReadyToTurnIn` y `Completed`; la transición a entrega requiere 3 Mordeluz comunes y 1 Resonante.
- `H5MissionRuntime` compone un bus, `H5MissionModel`, un `InventoryModel` de seis espacios y un `EquipmentModel` con ranura `Relic`.
- `TryTurnIn` añade el objeto fijo `Fragmento de Resonancia` antes de publicar la recompensa y marcar la misión como completada. Comprueba duplicados y la operación completada para que repetir la interacción no duplique el objeto.
- `H5CombatEventBridge` convierte la muerte de cada enemigo de escena en un evento de dominio; `H5NaraController` solo valida proximidad y delega aceptación/entrega al modelo.
- `H5MissionHud` y `H5PlayerMissionController` son presentación y entrada provisional; no deciden progreso ni inventario.

## Progresión H6

- `ExperienceModel` inicia en nivel 1, usa 100 XP como umbral del vertical slice y limita el nivel máximo a 2.
- Las fuentes son: Mordeluz común `10 XP`, Mordeluz Resonante `30 XP` y completar la misión `40 XP`. El recorrido normal suma exactamente 100 XP.
- `H6ProgressionModel` escucha eventos de dominio y publica `ExperienceGainedEvent` y `LevelUpEvent`. Cada fuente usa un `sourceId` estable; los IDs aplicados se guardan para impedir replay después de una carga.
- La subida a nivel 2 es terminal para este slice: XP adicional no aumenta el total ni produce otra subida.

## Persistencia H6

- `SaveGameData` y sus DTOs contienen solo datos: versión, experiencia, misión, IDs de derrotas procesadas, inventario, equipo y posición segura.
- `ISaveRepository` define `Load`, `Save` y `Reset` en `Game.Application`; `JsonFileSaveRepository` es la implementación local en `Game.Infrastructure.Local`.
- El repositorio escribe a `lumbre-h6-save.json.tmp` y reemplaza `lumbre-h6-save.json`. Si la plataforma no soporta `File.Replace`, usa una ruta compatible de reemplazo después de borrar únicamente el archivo objetivo.
- Un archivo inexistente crea partida nueva. JSON corrupto o una versión incompatible devuelve partida nueva y un diagnóstico; el runtime lo registra con `Debug.LogWarning` y no bloquea el juego.
- `H6ProgressionRuntime` compone el repositorio local, carga en `Awake`, aplica la posición segura en `Start`, guarda en cambios importantes, al pausar/salir y con `F5`; `F9` reinicia la partida QA y recarga la escena.
- Los enemigos se recrean al cargar la escena, pero `H5MissionModel` restaura `ProcessedDefeatIds` y `ExperienceModel` restaura `AppliedSourceIds`, por lo que no vuelven a conceder progreso o XP.

## Control H3

- `MovementIntent` vive en `Game.Domain/Movement` y aplica deadzone y clamp de magnitud.
- `H3PlayerController` convierte la intención lógica isométrica a dirección de mundo y mueve un `Rigidbody2D` con `MovePosition`.
- `WorldObstacle` contiene los colliders del greybox; el jugador usa `Player` y no atraviesa los límites ni las celdas bloqueadas.
- `H3CinemachineFollowTarget` enlaza el avatar con la `CinemachineCamera` oficial.
- `H3RespawnPoint` define el punto de retorno; el avatar respawnea con `R`, y también si abandona el volumen del greybox.
- `GreyboxDebugGizmos` dibuja grid lógico, obstáculos, ruta BFS y respawn en el Editor sin crear geometría runtime.

## Greybox H2

- `ProjectConstants` centraliza dimensiones, límites de zonas, obstáculos y puntos de navegación.
- `GreyboxLayout` construye una `WalkabilityGrid` de 32×20 con tres zonas: plaza, sendero y cueva.
- `GridPathfinder` usa BFS con presupuesto de nodos explícito; no depende de `UnityEngine`.
- La escena agrega una malla por zona, no un GameObject por celda.
- Los límites y obstáculos usan `BoxCollider2D` y `PolygonCollider2D` estáticos.
- La línea amarilla de preview demuestra una ruta válida y se reutiliza como referencia de depuración del control H3.

## Presentación H7

- `H7CharacterView` presenta sprites y clips Animator, pero solo observa controladores y eventos existentes.
- `H7PresentationRuntime` conecta ataques, habilidades, daño, muerte, misión, recompensa, equipamiento y nivel con VFX, audio, cámara y HUD.
- `H7VfxPool` se prewarmiza con 18 elementos y limita el máximo activo a 24. El pool detiene cada `ParticleSystem` antes de configurarlo para evitar reproducción accidental durante `Awake`.
- `H7AudioFeedback` crea one-shots y tres ambientes locales; no depende de red ni de un servicio externo.
- `H7CameraPolish` usa `CinemachineCamera` y `CinemachineFollow` oficiales, aplicando solo offsets y zoom temporales.
- `H7PresentationBuilder` importa texturas originales, configura SpriteRenderer/Animator, conserva colliders y guarda la escena de forma idempotente.
- `H7PresentationPlayModeTests` valida pool, audio, cámara, HUD, mundo, estados Animator y persistencia visual después de recargar.

## Optimización y UX H8

- `H8OptimizationBuilder` reconstruye la UI H8 de forma idempotente y deja el marker `H8OptimizationUx` en la escena.
- `H8PauseController` gestiona pausa, continuar, opciones, volumen de música/FX, vibración, FPS, debug, calidad gráfica, salida y etiqueta de versión.
- `H8LocalSettings` persiste preferencias de presentación con claves propias de `PlayerPrefs`; no reemplaza `ISaveRepository` ni serializa estado de partida.
- `H8PerformanceOverlay` usa `ProfilerRecorder` para presentar FPS, frame time, memoria, GC, draw calls y contadores CPU/GPU opcionales cuando QA activa los diagnósticos.
- `H8Tooltip` comparte un panel contextual para no multiplicar Canvas o ventanas.
- El HUD H7 actualiza estado textual a 10 Hz y conserva animación/pulso por frame; el pool VFX, Animator, texturas y Canvas reciben configuración de bajo coste.
- El build Android de desarrollo se genera desde `H8OptimizationBuilder.BuildAndroidDevelopment`; la prueba de hardware real, temperatura y batería no se simula en Editor.

## Fuera de H8

No se incorporan Addressables, networking, autenticación, economía, VIP, servidor, servicios externos, crafting, tienda, monedas, profesiones, atributos avanzados, más niveles, nube, cuentas, login, multiplayer, nuevas criaturas, nuevas misiones, nuevas habilidades, arte final de producción ni perfilado Android real.
