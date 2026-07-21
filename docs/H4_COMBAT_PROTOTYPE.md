# H4 — Combat Prototype

## Alcance

H4 valida el bucle de combate mínimo sobre la escena greybox: un jugador contra un único Mordeluz. Incluye ataque básico, salud, daño, cooldown, IA sencilla, muerte y feedback visual/sonoro provisional.

No incluye habilidades adicionales, loot, NPC, inventario, misiones, progresión, élite, economía, guardado, networking ni monetización.

## Contratos reutilizables

Los contratos viven en `Assets/Game/Domain/Combat` y no dependen de `UnityEngine`:

- `IHealth`: salud actual, salud máxima y estado vivo.
- `IDamageable`: recepción de `CombatDamage` y devolución de `DamageResult`.
- `ITargetable`: objetivo válido con salud y receptor de daño.
- `IAttacker`: daño, cooldown y ejecución de ataque sobre un objetivo.

Los adaptadores Unity están en `Assets/Game/Client/Combat`:

- `H4CombatHealth` conecta `CombatHealthModel` con GameObjects.
- `H4BasicAttacker` conecta `BasicAttackerModel` con el jugador y Mordeluz.
- `H4PlayerCombatController` busca objetivos `Enemy` dentro del rango.
- `MordeluzController` aplica el movimiento y la IA sobre `Rigidbody2D`.
- `H4CombatFeedback` gestiona flash, pulso, muerte y tonos generados localmente.

## IA de Mordeluz

```text
Idle → Detect → Follow → Attack
                         ↓
                    Return → Idle
                         ↓
                       Dead
```

- `Idle`: permanece en su spawn.
- `Detect`: confirma que el jugador está dentro del rango de detección.
- `Follow`: se acerca mientras permanece dentro del leash.
- `Attack`: aplica el ataque básico cuando el cooldown lo permite.
- `Return`: vuelve al spawn si el objetivo muere o abandona el leash.
- `Dead`: estado terminal después de llegar a cero de vida.

Mordeluz no bloquea físicamente al jugador. Esto mantiene la ruta H3 reproducible; el combate se decide por rango y estado, no por empuje de colliders provisionales.

## Controles

- Teclado QA: `Space`.
- Gamepad: `buttonSouth`.
- Android: botón táctil `ATK`, implementado con `OnScreenButton`.

Todos los controles pasan por el Input System. El ataque básico respeta el mismo cooldown independientemente del dispositivo.

## Criterios de aceptación

- Mordeluz aparece en la escena con Layer `Enemy`.
- Al entrar en detección, Mordeluz sigue al jugador y transita a `Attack` dentro de rango.
- Mordeluz daña al jugador sin atravesar cero de salud.
- El jugador puede atacar dentro de rango y el segundo ataque prematuro se rechaza por cooldown.
- Tres ataques básicos derrotan a Mordeluz.
- Tras morir, Mordeluz no vuelve a atacar y su feedback visual/audio se ejecuta.
- EditMode y PlayMode terminan sin fallos.

## Validación realizada

- Builder H4: batchmode, código 0.
- EditMode: 10 pasados, 0 fallidos.
- PlayMode: 2 pasados, 0 fallidos; incluye la regresión H3 y el recorrido completo del combate.
