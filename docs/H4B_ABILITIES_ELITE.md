# H4B — Habilidades y Mordeluz Resonante

## Objetivo

Validar dos habilidades del jugador y una variante élite sin ampliar el MVP con progresión, contenido persistente o backend.

## Implementado

- Calor de 0 a 100; los ataques básicos exitosos generan 25 y el área consume 50.
- Defensa: reducción del 65%, duración de 1.5 s y cooldown de 4 s.
- Área: 45 de daño, radio 2.2 y cooldown de 2.5 s.
- Mordeluz Resonante: 120 de vida y onda de 18 de daño, radio 1.8, anticipación de 0.8 s y cooldown de 2.4 s.
- Feedback provisional: flash de defensa, anillo de área, telegraph de onda, tonos procedurales y HUD de Calor.

## Arquitectura

Las reglas están en `Assets/Game/Domain/Combat` y no dependen de `UnityEngine`:

- `HeatResourceModel`
- `DefenseAbilityModel`
- `AreaAttackAbilityModel`
- `ResonantWaveAttackModel`
- `ICombatTimeSource`
- `IDamageModifier`

Los MonoBehaviours de `Assets/Game/Client/Combat` traducen input, física, objetivos, UI y feedback. El reloj real se inyecta con `UnityCombatTimeSource`; las pruebas usan un reloj manual.

## Criterio de salida

La escena `VerticalSlice` permite recorrer plaza, sendero y cueva; usar defensa y área; derrotar tres Mordeluz comunes; leer la anticipación de la onda; recibir daño solo al permanecer dentro de la zona; y derrotar al Resonante.

Validación final: EditMode 14/14 y PlayMode 3/3.

## Fuera de alcance

No se implementaron NPC, misiones, inventario, equipamiento, loot, XP, niveles, guardado, economía, networking ni arte final.
