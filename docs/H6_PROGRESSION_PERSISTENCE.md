# H6 — Progresión y persistencia local

H6 completa el vertical slice offline con el primer ciclo de progreso persistente:

`derrotar → ganar XP → nivel 2 → equipar → guardar → recargar → continuar sin duplicar`

## Progresión

- Nivel inicial: `1`.
- Nivel máximo del slice: `2`.
- Umbral exacto: `100 XP`.
- Mordeluz: `10 XP` cada uno.
- Mordeluz Resonante: `30 XP`.
- Completar la misión de Nara: `40 XP`.

El recorrido normal suma exactamente `100 XP`. `ExperienceModel` permanece libre de Unity y conserva IDs de fuentes aplicadas para rechazar replay. `H6ProgressionModel` traduce eventos de combate/misión a `ExperienceGainedEvent` y `LevelUpEvent`.

## Guardado

El archivo local se crea en `Application.persistentDataPath` con nombre `lumbre-h6-save.json`. Su estructura es un DTO JSON con `schemaVersion` y contiene:

- experiencia, nivel y fuentes aplicadas;
- misión, estado, contadores y derrotas procesadas;
- inventario y capacidad;
- reliquia equipada;
- posición segura.

`JsonFileSaveRepository` escribe primero a `lumbre-h6-save.json.tmp` y luego reemplaza el archivo objetivo. Un archivo ausente inicia una partida nueva; un archivo corrupto o de versión incompatible inicia una partida limpia y registra el diagnóstico sin bloquear la escena.

## Restauración

La runtime H6 carga después de que H5 haya creado sus modelos. Restaura misión, inventario y equipamiento en `Awake`, y aplica la posición al jugador en `Start`. Los enemigos se recrean al recargar la escena, pero sus IDs ya procesados permanecen en el save, así que no vuelven a dar XP, progreso ni recompensa.

Controles QA:

- `F5`: guardar ahora.
- `F9`: borrar el save y recargar la escena para iniciar una partida limpia.

## Criterios validados

- Completar la misión deja al jugador en nivel 2 con 100 XP.
- El Fragmento de Resonancia sigue en el inventario y equipado después de recargar.
- La misión continúa `Completed` con progreso `3/3` y `1/1`.
- La posición guardada se restaura dentro de tolerancia.
- Repetir la interacción o publicar una derrota ya procesada no duplica recompensa, progreso ni XP.

## Fuera de H6

No se implementan tienda, economía, monedas, crafting, profesiones, atributos avanzados, nivel 3+, varias partidas, nube, cuentas, login, networking, multiplayer, arte final ni audio final.
