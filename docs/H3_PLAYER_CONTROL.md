# H3 — Control del jugador

## Alcance

H3 implementa únicamente el desplazamiento del avatar sobre el greybox de H2. Incluye joystick virtual para Android, teclado para QA, movimiento basado en intención, colisiones 2D, seguimiento con Cinemachine, respawn y herramientas de depuración del Editor.

Quedan fuera combate, NPC, inventario, misiones, daño, interacción, arte final, guardado, networking, economía y monetización.

## Capas oficiales

| Layer | Nombre | Uso |
| --- | --- | --- |
| 8 | `WorldGround` | Suelo visual de plaza, sendero y cueva |
| 9 | `WorldObstacle` | Límites y celdas bloqueadas con colisión |
| 10 | `Player` | Avatar y visual provisional |
| 11 | `PlayerRespawn` | Punto de respawn |
| 12 | `NavigationDebug` | Preview y referencias de navegación |
| 13 | `PlayerUI` | Canvas y joystick móvil |

La fuente de estas constantes es `Assets/Game/Domain/Constants/ProjectLayers.cs`.

## Flujo de control

```text
Joystick Android / teclado QA / gamepad
                ↓
       H3PlayerInputReader
                ↓
         MovementIntent
                ↓
       H3PlayerController
                ↓
      Rigidbody2D + colisiones
                ↓
       Cinemachine Follow
```

`MovementIntent` se mantiene en `Game.Domain` y no conoce Unity ni el dispositivo. WASD sirve para QA en el Editor; `R` fuerza respawn. En Android, `OnScreenStick` alimenta la misma acción `Move` mediante `<Gamepad>/leftStick`.

## Gizmos de Editor

`GreyboxDebugGizmos` se encuentra en `H2_Greybox` y permite visualizar:

- grid isométrico lógico;
- obstáculos y límites;
- ruta BFS plaza → cueva;
- punto de respawn.

Los cuatro indicadores se activan por defecto y no crean geometría de gameplay en el build.

## Criterios de aceptación

- `VerticalSlice.unity` abre sin errores y contiene la cámara oficial de Cinemachine.
- El avatar aparece en la plaza, en `Player`, y el punto de retorno aparece en `PlayerRespawn`.
- WASD mueve el avatar durante QA y `R` lo devuelve al respawn.
- El joystick virtual usa la acción `Move` y comparte el contrato `MovementIntent`.
- Los límites y obstáculos bloquean el avatar; no se atraviesan las celdas bloqueadas.
- El avatar puede recorrer plaza → sendero → cueva y volver a la plaza.
- Se ejecutan 7 tests EditMode y 1 test PlayMode, todos en verde.

## Validación realizada

- Builder de escena H3: batchmode, código 0.
- EditMode: 7 pasados, 0 fallidos.
- PlayMode: 1 pasado, 0 fallidos; recorrido completo con gamepad virtual.
