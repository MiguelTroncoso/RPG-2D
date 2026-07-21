# H5 — Misión y recompensa

H5 valida el primer bucle de objetivo persistente local sobre el combate de H4B:

`Nara → aceptar → derrotar 3 Mordeluz + 1 Resonante → regresar → recibir recompensa → equipar`

## Alcance

- Nara Velaquieta vive en la plaza, en la posición de inicio del jugador.
- La interacción se valida por proximidad, con rango `1.65`.
- `F`/`buttonNorth` acepta la misión o entrega la recompensa según el estado.
- La misión requiere tres eventos `CombatantKind.Mordeluz` y uno `MordeluzResonante`.
- El objeto fijo es `Fragmento de Resonancia`, con ranura `Relic`.
- El inventario tiene seis espacios fijos; equipar no elimina el objeto del inventario.
- El HUD muestra estado, progreso, espacios y ranura equipada.

## Estados

```text
Available → Active → ReadyToTurnIn → Completed
```

Los eventos de derrota solo cuentan en `Active`. Cada entidad se identifica con `EntityId`; repetir el mismo evento no incrementa el progreso. La entrega requiere que el inventario tenga espacio o que ya contenga la recompensa, y después marca la misión como completada.

## Arquitectura

- `Game.Domain/Events/DomainEvents.cs`: bus y evento de derrota.
- `Game.Domain/Missions/H5MissionModel.cs`: estados, objetivos, progreso y entrega.
- `Game.Domain/Inventory/InventoryModels.cs`: inventario fijo y equipamiento.
- `Game.Client/Missions/H5CombatEventBridge.cs`: adapta `H4CombatHealth.Died` al evento de dominio.
- `Game.Client/Missions/H5NaraController.cs`: proximidad y delegación al modelo.
- `Game.Client/Missions/H5MissionRuntime.cs`: composición local del bus, misión, inventario y equipo.
- `Game.Client/Missions/H5MissionHud.cs`: presentación provisional.

## Criterios de salida

- La misión inicia como `Available` y acepta solo dentro del rango de Nara.
- El progreso queda en `3/3` Mordeluz y `1/1` Resonante tras las derrotas.
- La misión cambia a `ReadyToTurnIn` antes de volver con Nara.
- La entrega añade una sola recompensa al inventario y cambia a `Completed`.
- Repetir la interacción no aumenta el inventario.
- El jugador puede equipar la recompensa en la ranura `Relic` y el objeto continúa visible en el inventario.
- EditMode y PlayMode permanecen en verde junto con las regresiones anteriores.

## Fuera de H5

No se implementan XP, niveles, guardado, crafting, tienda, economía, drops aleatorios, networking, reemplazo de equipo, stacks, drag-and-drop, tooltips ni arte final.
