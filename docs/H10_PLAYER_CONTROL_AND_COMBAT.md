# H10 — Control del jugador, locomoción y combate táctil

> Proyecto: Lumbre de Nácar · Versión: v0.8.1 Alpha · Estado: IMPLEMENTADO CON OBSERVACIONES

## Objetivo y límite

H10 mejora la sensación del vertical slice sin añadir contenido ni sistemas nuevos. El jugador conserva el mundo, enemigos, misión, progresión, inventario, equipamiento, persistencia y reglas H3–H9, pero ahora tiene una ruta de input única para joystick Android, touch, teclado y gamepad de QA.

No se implementan H11, networking, servidor, login, multiplayer, economía, tienda, crafting, drops, nuevos mapas, enemigos, NPC, misiones, habilidades, niveles, balance, guardado nuevo ni arte final.

## Diagnóstico de causa raíz

Antes de H10, cada controlador consultaba `WasPressedThisFrame()` desde `Update`. Los botones móviles son `OnScreenButton`: su pulsación alimenta un dispositivo virtual y la transición puede procesarse en el siguiente update del Input System. En ese borde, una pulsación física podía no coincidir con el muestreo del controlador. Además, pausa, pérdida de foco y muerte no compartían una frontera de estado.

La corrección es acotada:

1. `H3PlayerInputReader` enlaza `InputAction.started` y conserva un latch hasta el frame de consumo.
2. Joystick, teclado, mouse, gamepad y touch siguen el mismo `PlayerInputActions` y los mismos contratos.
3. Al perder foco se limpian latches y se reinician solo dispositivos virtuales con uso `OnScreen`, evitando botones pegados al volver desde Android.
4. `PlayerActionStateModel` bloquea acciones durante `Paused`/`Dead` y coordina `Attacking`, `Defending`, `AreaAttack` e `Interacting`.

## Locomoción

- `MovementIntent` remapea dead zone y respuesta analógica, limita magnitud y normaliza diagonales.
- `PlayerLocomotionModel` aplica aceleración y desaceleración frame-rate independent fuera de Unity.
- `H3PlayerController` convierte la intención lógica a la base isométrica, conserva `CurrentLookDirection`, usa `Rigidbody2D` continuo, mantiene colisiones y conserva respawn H3.
- Velocidad máxima configurada: 2.4.
- Aceleración configurada: 64.
- Desaceleración configurada: 128.
- Dead zone: 0.12.
- Respuesta analógica: 1.0.

## Acciones

| Acción | Touch | QA | Regla preservada |
| --- | --- | --- | --- |
| ATK | `H4_AttackButton` | Space/gamepad South | daño y cooldown H4 |
| DEF | `H4B_DefenseButton` | Q/gamepad East | Calor/defensa H4B |
| AOE | `H4B_AreaButton` | E/gamepad West | coste/radio/cooldown H4B |
| Interactuar | `H5_InteractButton` | F/gamepad North | proximidad y misión H5 |
| Equipar | `H5_EquipButton` | G/gamepad Left Shoulder | inventario/equipamiento H5 |

Los botones H10 solo agregan feedback de pulsación en la capa de presentación. No ejecutan reglas ni crean una segunda ruta de gameplay.

El ataque básico conserva continuidad temporal del objetivo durante una cadencia corta si el enemigo se mueve alrededor del jugador entre dos golpes. Esto evita un retargeting accidental de la regresión H4 sin crear un lock persistente ni cambiar daño, alcance o cooldown.

## Pruebas y builder

- EditMode: 27/27 pasados, 0 fallidos, 0 ignorados. XML: `/tmp/h10-editmode-1.xml`.
- PlayMode: 35/35 pasados, 0 fallidos, 0 ignorados. XML: `/tmp/h10-playmode-1.xml`.
- La suite PlayMode contiene 24 pruebas H10 y conserva las regresiones H3, H4, H4B, H5, H6, H7, H8.1, H8 y H9.
- `H10PlayerControlBuilder.Build` y `H10PlayerControlBuilder.Validate` terminaron repetidamente con código 0.
- La validación comprueba jugador, estado, rutas `OnScreenButton`, feedback, safe area y ausencia de duplicación del scene graph.

## APK

Configuración: Android landscape, ARM64, OpenGLES3, Development Build, Allow Debugging y ConnectWithProfiler.

```text
Builds/Android/LumbreDeNacar-v0.8.1-H10.apk
Tamaño: 76,139,085 bytes
SHA-256: a78cf8d9aaefc9703032860023cc690cea19ddeceb641640125a3e236ff3004f
Generado: 2026-07-21 21:13:47 -0400
```

El APK queda excluido de GitHub por `.gitignore`.

## Estado de aceptación

**IMPLEMENTADO CON OBSERVACIONES.** La compilación, las suites automáticas, el builder y el APK están verificados. Falta validación física Android: instalación limpia, segundo arranque, arranque con save, minimizar/restaurar, force close, controles touch reales, cámara/HUD/safe area, pausa, interacción, equipamiento, logcat, Unity Profiler, FPS, memoria, temperatura y batería.

No se declara H10 aprobado únicamente con pruebas de Editor o existencia del APK. Se mantiene `v0.8.1 Alpha` y no se inicia H11 hasta la revisión técnica y la validación física.
