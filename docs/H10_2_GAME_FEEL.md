# H10.2 — Game Feel, locomoción natural y feedback de combate

> Proyecto: Lumbre de Nácar · Versión: v0.8.1 Alpha · Estado: IMPLEMENTADO CON OBSERVACIONES

## Resumen ejecutivo

H10.2 convierte la locomoción y el combate táctil existentes en una experiencia más legible sin añadir contenido ni cambiar balance. El movimiento conserva la simulación H3 fuera de Unity, mientras la presentación refleja velocidad, reposo y acción. El ataque básico ahora comunica anticipación, impacto y recuperación; el daño se aplica una sola vez en el impacto lógico. DEF y AOE recibieron una anticipación visual y un cierre coherente con sus reglas actuales.

No se inició H11, no se modificó `main`, no se añadieron enemigos, mapas, misiones, habilidades, economía, networking ni backend. La versión continúa en `v0.8.1 Alpha`.

## Rama y base

- Rama: `codex/h10-2-game-feel-combat-feedback`
- Commit base: `c1aa5aab42a43f1ba42172a6da9167bc2f47ae7c`
- Base: `fix(android): prevent H10 scene deserialization crash`

## Diagnóstico y solución

### Locomoción lineal

La intención ya tenía aceleración, desaceleración y respuesta analógica, pero la conversión isométrica podía producir una magnitud mundial superior a `MaxSpeed` en diagonales. H10.2 conserva el modelo de dominio y limita la velocidad después de convertir la intención a la base isométrica. La presentación usa la velocidad normalizada y un bob visual discreto para distinguir `Idle` y `Moving` sin mover el `Rigidbody2D` de forma artificial.

También se conserva la última dirección válida de facing y la respuesta analógica parcial; no se añadió `SmoothDamp` ni una segunda ruta de input.

### Ataque fantasma

Antes, el daño podía percibirse como una aplicación instantánea al solicitar ATK. La nueva secuencia runtime separa solicitud, anticipación, impacto y recuperación mediante `AttackSequenceModel`, que vive en el dominio y no depende de Unity. El controlador Unity coordina la coroutine, pero las reglas de fases y la garantía de un único impacto permanecen fuera de `MonoBehaviour`.

El ataque rechazado publica feedback de rechazo sin hit-spark ni cámara. Un impacto exitoso dispara feedback del atacante, reacción del objetivo, sonido existente y un impulso Cinemachine sutil.

### Diagnóstico Android H10.2

La matriz física comparó el APK H10.1 exacto, H10.2 con los tiempos de secuencia serializados y H10.2 con esos tiempos reconstruidos en runtime. H10.1 arrancó correctamente. La variante H10.2 que persistía los cuatro tiempos en los componentes de escena reprodujo `CachedReader::OutOfBoundsError` durante `LoadSceneOperation::Perform`, mientras que la variante final conserva los mismos valores y configuración mediante `ConfigureTiming` sin escribirlos en `VerticalSlice.unity` y arrancó correctamente en el mismo dispositivo.

La escena final solo cambia su marcador a `H10_2GameFeelCombatFeedback`; no contiene los campos temporales serializados. Esta es la corrección aplicada al crash de deserialización sin cambiar reglas de combate, balance ni contenido.

## Assets y presentación reutilizados

- El jugador ya tenía clips `Idle`, `Walk`, `Attack`, `Defense`, `Area`, `Damage` y `Death` en su Animator; H10.2 los conserva y activa los estados transitorios existentes.
- Los enemigos ya tenían clips de ataque, daño, muerte y telegraph; no se crearon controladores paralelos.
- No había archivos de audio finales dedicados a H10.2. Se reutilizan los tonos procedurales permitidos de H7 y se añade el rechazo a la interfaz de audio existente.
- `H4CombatFeedback` mueve y escala únicamente el hijo visual para no alterar colisiones ni posición lógica.
- `H4BAbilityFeedback` mantiene el efecto DEF durante toda su duración y muestra un anillo de anticipación AOE con el mismo radio lógico (`2.2`).
- `H7PresentationRuntime` conserva la cámara oficial Cinemachine y aplica shake solo sobre impactos exitosos.

## Cambios técnicos

- `AttackSequencePhase` y `AttackSequenceModel`: fases puras `Idle → Anticipation → Impact → Recovery` y consumo de impacto único.
- `H4PlayerCombatController`: timing de ATK `0.10 s / 0.18 s`, validación al impacto y eventos separados para secuencia y resultado.
- `H4BPlayerAbilityController`: timing de AOE `0.14 s / 0.12 s`, sin alterar coste, radio ni cooldown.
- `H3PlayerController`: límite de velocidad mundial después de la conversión isométrica y `CurrentSpeedNormalized` para presentación.
- `H4CombatFeedback`, `H4BAbilityFeedback`, `H7CharacterView` y `H7PresentationRuntime`: feedback visual, audio y Cinemachine reutilizable.
- `H10_2GameFeelBuilder`: configuración y validación idempotente sin persistir estado runtime.

No se duplicó input: teclado, gamepad y touch siguen pasando por las mismas Input Actions. No se duplicó daño: solo `H4BasicAttacker` aplica el resultado y la fase de impacto impide doble consumo.

## Testing

- EditMode: **30/30 pasados**, 0 fallidos, 0 ignorados. XML: `/tmp/h10-2-editmode-noserialized.xml`.
- PlayMode: **38/38 pasados**, 0 fallidos, 0 ignorados. XML: `/tmp/h10-2-playmode-noserialized.xml`.
- Se conservan las regresiones H3–H9 y se añadieron pruebas de fases de ataque, impacto único, rechazo sin objetivo, ventana visual DEF, respuesta analógica, límite diagonal y radio AOE.
- `H10_2GameFeelBuilder.Build` y `Validate` se ejecutaron dos veces; las cuatro ejecuciones terminaron con código 0 y la validación informó ausencia de duplicados.

## Validación Android

### APK

```text
Builds/Android/LumbreDeNacar-v0.8.1-H10.2.apk
Tamaño: 76,184,173 bytes
SHA-256: 3e2e9fa13d8b2100e3d6250ad5db54f4dc2ebeb3da5313fe35bf3c42b9c2393b
Generado: 2026-07-22 20:26:10 -0400
Configuración: ARM64 · OpenGLES3 · Development · AllowDebugging · ConnectWithProfiler · CleanBuildCache
```

El APK está excluido de GitHub por `.gitignore`.

### Dispositivo

- Fabricante/modelo: Xiaomi `24090RA29G`
- Android: 16, API 36
- Resolución física: `1220×2712`; render landscape observado: `2712×1220`
- ABI: `arm64-v8a`
- GPU: Mali-G615 MC2
- RAM reportada por Unity: 7428 MB
- Paquete: `com.LumbredeNacarStudio.LumbredeNcar`
- Instalación: `adb install -r` completada con `Success`

La instalación de la variante corregida terminó con `Success` y la aplicación se abrió mediante `adb shell monkey`. Tras completar Bootstrap y la carga asíncrona, la escena VerticalSlice se mostró correctamente con `VerticalSlice activated`, `Player initialized: True`, `Camera initialized: True` y `HUD initialized: True`. En `/tmp/h10-2-fixed-launch-logcat.txt` no aparecen `CachedReader::OutOfBoundsError`, `FATAL EXCEPTION`, `SIGSEGV`, corrupción de `level1` ni ANR.

La captura física incluida en [`docs/captures/android-h10.2-physical/01-gameplay-ready.png`](captures/android-h10.2-physical/01-gameplay-ready.png) corresponde a esta variante corregida y confirma el encuadre landscape, safe area, HUD, cámara y visibilidad de joystick y botones sin recortes ni zonas negras persistentes después de cargar.

### Observaciones de hardware

- Memoria puntual: TOTAL PSS aproximado `542097 kB`; Graphics `149391 kB`; Native Heap `35268 kB`.
- Batería observada: 66%; temperatura reportada: 24 °C.
- `dumpsys gfxinfo` no entregó percentiles utilizables para esta ejecución de Unity; no se presenta un FPS universal.
- El uso de `ConnectWithProfiler` produjo advertencias de conexión UDP y buffer del Profiler (`134234112 bytes`) al no estar conectado el Editor. No son crashes del juego.
- Android 16 bloqueó `adb shell input tap/swipe` con `SecurityException` por permisos de inyección. Por ello la automatización ADB no se presenta como validación de pulsación táctil; la respuesta manual de ATK/DEF/AOE, joystick, pausa y cambio de dirección queda pendiente de comprobación interactiva en el dispositivo.

## Estado y riesgos pendientes

**IMPLEMENTADO CON OBSERVACIONES.** Código, tests, Builder, APK, instalación, arranque, estabilidad nativa y layout físico están verificados; además, la causa del crash de H10.2 quedó demostrada por la comparación entre las variantes serializada y no serializada. La validación táctil interactiva completa no pudo automatizarse desde este Mac debido a la política de Android 16, y el Profiler no pudo conectarse por la configuración de desarrollo. No se ocultan estas limitaciones.

Riesgos reales:

- repetir la validación manual de las ocho direcciones, frenado, facing, ATK exitoso/rechazado, DEF, AOE y pausa directamente en el teléfono;
- obtener una sesión de Unity Profiler conectada o métricas nativas equivalentes;
- mantener bajo observación el tiempo de primer arranque de IL2CPP en instalaciones limpias.

## Archivos y limpieza

No se eliminaron archivos del proyecto. `Library/`, `Temp/`, `Logs/`, `Obj/`, `Builds/`, XML temporales y APK no se incluyen en el commit. El árbol se revisó con `git diff --check` antes de publicar.

La recomendación es mantener H10.2 como **implementado con observaciones**, publicar la rama para revisión técnica y no abrir H10.3 ni H11 hasta cerrar la validación táctil manual y revisar el comportamiento del Profiler.
