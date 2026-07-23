# H10.3 — Resumen de Implementación

> Proyecto: Lumbre de Nácar · Versión: v0.8.1 Alpha · Rama: `claude/h10-3-body-animation-combat-80s54o`

Objetivo del hito: eliminar la sensación de deslizamiento, dar peso a la
caminata, hacer el ataque legible por fases con daño exactamente en el impacto,
y agregar una reacción visible del enemigo — sin romper H1–H10 ni recurrir solo
a efectos.

## 1. Enfoque general

Los assets son ilustraciones únicas sin frames (ver `asset-audit.md`). En vez de
simular vida con escala/partículas, se recortó la ilustración del jugador en un
**cutout rig 2D** de 7 piezas y se anima por **huesos** (rotación/traslación de
articulaciones) desde modelos de dominio puros, sin dependencia de Unity. Así el
movimiento corporal es real y además testeable fuera del motor.

La lógica que decide poses, fases y reacción vive en `Game.Domain` (C# puro,
`noEngineReferences`). La capa cliente (`Game.Client`) solo traduce esas poses a
transforms de Unity. Esto cumple Clean Architecture y permite verificar la
lógica con tests EditMode y con un arnés offline.

## 2. Recorte del rig (assets agregados)

`Assets/Game/Presentation/Art/Characters/BastionParts/`:

| Pieza | Padre | Pivote (articulación) | Sorting |
| --- | --- | --- | --- |
| `bastion-cape.png` | root | manto | 28 |
| `bastion-head.png` | torso | cuello | 29 |
| `bastion-leg-l.png` (frontal) | root | cadera izq. | 30 |
| `bastion-leg-r.png` (trasera) | root | cadera der. | 30 |
| `bastion-torso.png` | root | pelvis | 31 |
| `bastion-arm-sword.png` | torso | hombro | 32 |
| `bastion-shield.png` | torso | hombro escudo | 33 |

- El recorte es determinista (prioridad por polígonos/cápsulas + relleno de
  huecos por *inpainting* bajo oclusores). La recomposición neutra reproduce la
  ilustración original con **diferencia de píxel 0**.
- Cada `.png.meta` fija pivote personalizado (alignment 9) en la articulación,
  para que la rotación por hueso sea anatómicamente correcta.

## 3. Locomoción corporal

Archivos nuevos:

- `Domain/Animation/PlayerBodyPose.cs` — estructura de pose (ángulos/offsets) + `Blend`.
- `Domain/Animation/PlayerBodyPoseModel.cs` — generador de poses de caminata.
- `Client/Presentation/H103BodyRigView.cs` — aplica la pose al rig en `LateUpdate`.

Claves contra el "deslizamiento":

1. **Fase de zancada ligada a la distancia física recorrida**, no al tiempo:
   `stridePhase += distanceDelta / strideLength`. Si el cuerpo no se traslada,
   la zancada no avanza; si se traslada rápido, cadencia rápida. Es
   imposible que los pies "patinen".
2. **Sincronía Animator ↔ física**: además, `H7CharacterView` ajusta
   `animator.speed` proporcional a la velocidad real (0.8–1.3) mientras el
   estado semántico es `Walk`, de modo que la animación heredada tampoco puede
   desincronizarse.
3. **Respuesta proporcional al joystick**: la amplitud del movimiento corporal
   escala con la intensidad analógica (`speed^0.7`), suavizada para que un
   frenazo asiente en lugar de cortar.
4. **Idle claro vs Walk real**: en idle la amplitud decae a ~0 y aparece una
   respiración sutil (breath) desde el mismo modelo; en walk las piernas
   alternan en oposición, el torso balancea y hay bob vertical de cadera.
5. **Facing estable con histéresis**: el volteo horizontal solo ocurre cuando la
   componente X del *look* cruza claramente un umbral, evitando parpadeo en
   diagonales. Las **8 direcciones** de locomoción física y facing se conservan
   (se heredan de H3/H10); la silueta del cuerpo es la vista frontal (limitación
   de asset documentada).

El rig se colocó como **hermano** de `H7_Player_Visual` (hijo directo del
jugador), no como hijo, para que las curvas de escala del Animator H7 (heredado)
no vuelvan a introducir un pulso de escala sobre el cuerpo. El renderer único
original se **deshabilita** pero conserva su sprite, manteniendo intacto el
contrato de presentación H7.

## 4. Ataque por fases (daño en el impacto)

Archivos nuevos:

- `Domain/Combat/AttackSequenceModel.cs` — secuencia temporizada Idle → Windup → Strike → Impact → Recovery → Idle.
- `Domain/Animation/PlayerAttackPoseCurve.cs` — poses clave por fase.

Modificado: `Client/Combat/H4PlayerCombatController.cs`.

- El input de ataque ya **no** aplica daño al pulsar. Inicia una secuencia; el
  daño se resuelve **exactamente cuando arranca el impacto visual**
  (`ImpactStarted`), es decir tras windup+strike. Ni antes ni después.
- `ImpactStarted` se dispara **exactamente una vez** por secuencia, incluso si
  un frame largo salta toda la ventana de impacto (garantía verificada por
  test). El daño nunca se pierde ni se duplica.
- El **cooldown H4 se mide impacto-a-impacto**: una pulsación se acepta si su
  instante de impacto futuro libera el cooldown, preservando la cadencia H4.
- El `H103BodyRigView` superpone la pose de ataque sobre la locomoción con un
  peso que sube/baja suave, así el jugador puede atacar en movimiento.
- La ruta programática `TryBasicAttack()` (síncrona, daño inmediato) se
  **conserva** para QA y para las regresiones H4B/H5/H6 que matan enemigos en
  bucle; el gameplay por input usa la secuencia visible.

## 5. Reacción visible del enemigo

Archivo nuevo: `Domain/Combat/HitReactionModel.cs`. Modificado:
`Client/Combat/MordeluzController.cs`.

Al recibir daño no letal, el enemigo:

1. Entra en **stagger** (0.28 s) que **interrumpe** su IA (deja de perseguir y
   de resolver ataques).
2. Recibe un **knockback pequeño** (0.15 u) alejándose del jugador, con easing
   que se asienta y **nunca** excede la distancia configurada.
3. Mantiene el estado de animación **Damage** durante toda la ventana de stagger
   (antes 0.18 s fijo), sumado al **flash** rojo existente (`H4CombatFeedback`).

El knockback solo se mueve mientras el controlador está activo; cuando un test
desactiva la IA del enemigo para matarlo en bucle, no hay desplazamiento, por lo
que las regresiones existentes se mantienen. El golpe letal no aplica reacción.

## 6. DEF y HAB (verificación)

No se cambió su lógica; se verificó y cubrió con tests que:

- **No dejan estados bloqueados**: DEF y HAB usan `try/finally` para liberar
  siempre la compuerta de acción (`PlayerActionState`), y la secuencia de ataque
  libera `Attacking` al completar, al cancelar, al pausar y al deshabilitar.
- **Sincronía**: los efectos siguen ligados a los eventos `DefenseActivated` /
  `AreaAttackActivated` (sin cambios).
- **Limpieza**: el modificador de daño de defensa se retira al expirar
  (verificado con un test que confirma que el daño vuelve a full tras la
  ventana). El anillo de área se apaga tras su duración.

## 7. Arquitectura y compatibilidad

- Unity 6 LTS (6000.3.20f1), Input System 1.19, Cinemachine 3.1.7 — sin cambios.
- Toda la lógica nueva de decisión vive en `Game.Domain` (C# puro); el cliente
  solo aplica poses. `Game.Domain` sigue sin referenciar UnityEngine.
- **No** se modificó networking, **no** se inició H11, **no** se serializa
  estado runtime (los modelos son transitorios, reconstruidos en `Awake`).
- El builder H10.3 (`Editor/H103BodyCombatBuilder.cs`) es **idempotente**:
  re-ejecutarlo converge a la misma escena sin duplicar nodos, y conserva el
  renderer/base y los clips H7.

## 8. Vista previa del resultado

`docs/captures/h10.3/walk-cycle-preview.png` y
`docs/captures/h10.3/attack-phases-preview.png` muestran, respectivamente, el
ciclo de caminata y las cinco poses del ataque **generadas con la matemática
exacta de los modelos runtime** (`PlayerBodyPoseModel` y `PlayerAttackPoseCurve`)
aplicada al rig recortado.

> Nota de honestidad: son *previews* de un compositor offline que reproduce la
> jerarquía y los pivotes del rig de Unity; **no** son capturas del editor ni de
> dispositivo físico, porque este entorno no dispone de Unity. La validación
> física en Android sigue pendiente (ver acceptance-matrix).
