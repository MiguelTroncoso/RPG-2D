# H10.3 — Auditoría de Assets de Animación

> Proyecto: Lumbre de Nácar · Versión: v0.8.1 Alpha · Hito: H10.3 Body Animation & Visible Combat

Esta auditoría revisa el estado real de los assets de animación heredados de
H7–H10.2 para determinar si permiten una caminata y un ataque corporales
convincentes, o si obligan a documentar una limitación.

## 1. Animator Controllers

Ubicación: `Assets/Game/Presentation/Animations/`.

| Controller | Estados | Parámetros | Transiciones |
| --- | --- | --- | --- |
| `BastionBrasaController` (jugador) | Idle, Walk, Attack, Defense, Area, Damage, Death | `Moving` (bool, sin uso real) | Ninguna transición conectada |
| `MordeluzController` | Idle, Walk, Attack, Damage, Death | `Moving` | Ninguna |
| `Mordeluz_2Controller` | Idle, Walk, Attack, Damage, Death | `Moving` | Ninguna |
| `Mordeluz_3Controller` | Idle, Walk, Attack, Damage, Death | `Moving` | Ninguna |
| `Mordeluz_ResonanteController` | Idle, Walk, Attack, Telegraph, Damage, Death | `Moving` | Ninguna |
| `NaraVelaquietaController` | Idle, Talk, MissionReady | — | Ninguna |

Los controllers **no usan transiciones**: la capa de presentación
(`H7CharacterView`) cambia de estado llamando `Animator.Play(state)`
directamente. El parámetro `Moving` existe pero nunca gobierna una transición.

## 2. Animation Clips

Cada estado tiene un clip generado por `H7PresentationBuilder`. Su contenido
real es la clave del problema:

- **1 solo keyframe de sprite** (`m_PPtrCurves` con un único `ObjectReferenceKeyframe` en `time: 0`). No hay flipbook de frames.
- Curvas de **escala** (`m_LocalScale.x/y`) con 3 keys: un pulso 1.0 → 1.12 → valor final. Es decir, la única "animación" es un **escalado** del mismo sprite.
- `Idle` y `Walk` tienen `m_LoopTime: 1`; el resto no.

Ejemplo verificado en `BastionBrasaController_Walk.anim`: 3 keys de escala,
todas con valor `1` (Walk ni siquiera pulsa), y un único sprite
`H7_Player_Visual`. **El clip Walk no contiene movimiento corporal alguno.**

### Frames por animación

| Animación | Frames de sprite | Contenido de movimiento |
| --- | --- | --- |
| Idle | 1 | Escala constante (sin cambio) |
| Walk | 1 | Escala constante (sin cambio) — **no transmite caminar** |
| Attack | 1 | Pulso de escala 1.0→1.12→1.0 (~0.38 s) |
| Area / Telegraph | 1 | Pulso de escala 1.0→1.12 |
| Damage | 1 | Pulso de escala 1.0→0.92 |
| Death | 1 | Escala 1.0→0.08 (encogimiento) |

## 3. Blend Trees

**No existen Blend Trees** en el proyecto. La locomoción direccional (8
direcciones) no está soportada por ningún blend tree de animación; el facing
se resolvía únicamente girando el vector lógico en `H3PlayerController`.

## 4. Sprites y SpriteSheets

Ubicación: `Assets/Game/Presentation/Art/Characters/`.

| Sprite | Dimensión | Modo | SpriteSheet |
| --- | --- | --- | --- |
| `bastion-brasa.png` (jugador) | 1254×1254 | Single | Ninguno |
| `mordeluz.png` | 1159×1358 | Single | Ninguno |
| `mordeluz-resonante.png` | 1122×1402 | Single | Ninguno |
| `nara-velaquieta.png` | 1024×1536 | Single | Ninguno |
| `*-source.png` (4) | idem | Single (mode 0) | Copias fuente sin recortar |

**Ninguna textura está en modo Multiple.** No hay atlas, no hay filas/columnas
de frames (`flipbookRows: 1`, `flipbookColumns: 1`). Cada personaje es **una
única ilustración estática, de vista frontal 3/4**, con pivote central.

### Contenido de píxeles (jugador)

- RGBA, 68.2 % transparente, cuerpo entero opaco en bbox x∈[102,1166], y∈[79,1175].
- Es una figura completa de pie: cabeza, torso con emblema, brazo con espada (mano izquierda en pantalla), escudo, capa y dos piernas claramente separables.

## 5. Prefabs

**No hay prefabs de personaje** (`.prefab`) en el proyecto. Los personajes se
construyen por código en la escena `VerticalSlice.unity` mediante los builders
de Editor (`H3PlayerBuilder`, `H4CombatBuilder`, `H7PresentationBuilder`, …).

## 6. Limitaciones encontradas

1. **No hay frames de animación.** Cada estado es un solo sprite; toda la
   "vida" venía de pulsos de escala. Esto es exactamente la causa del
   "sprite deslizándose" y del "ataque fantasma": no existe movimiento
   corporal, solo un cambio de tamaño.
2. **No hay spritesheets ni vistas laterales/traseras.** Solo existe una vista
   frontal 3/4 por personaje. Una caminata con vistas por dirección (N/S/E/O y
   diagonales) es **imposible con los assets actuales** sin arte nuevo.
3. **No hay Blend Trees** que soporten locomoción analógica direccional.
4. **No hay huesos ni rig** (`bones: []`, `spriteBones` vacío) en las texturas.

## 7. Conclusión de la auditoría y decisión

Los assets **no permiten** una caminata ni un ataque corporal convincente por
la vía tradicional (frames o spritesheets), y **tampoco** deben resolverse solo
con efectos (partículas/escala/cámara), como pide explícitamente el hito.

Sin embargo, la ilustración del jugador **sí es separable en partes anatómicas
coherentes** (cabeza, torso, brazo-espada, escudo, dos piernas, capa). Esto
habilita una tercera vía **honesta y sin inventar arte**: construir un
**cutout rig 2D** recortando la propia ilustración en piezas y animándolas por
huesos (rotación/traslación de articulaciones). El movimiento corporal es
entonces **real** (las piernas alternan, el torso carga y descarga en el
ataque), no un efecto sobrepuesto.

Se generó un recorte determinista de la ilustración del jugador en 7 piezas
cuya recomposición neutra reproduce el original con **diferencia de píxel 0**
(verificado). Las piezas se guardan en
`Assets/Game/Presentation/Art/Characters/BastionParts/` con pivotes ubicados en
las articulaciones.

### Limitación que se mantiene y se documenta

- El rig se deriva de **una sola vista frontal**. El *facing* se resuelve
  volteando horizontalmente (izquierda/derecha); **no** existen vistas
  laterales ni traseras reales. El ataque, por tanto, es un tajo frontal con
  volteo L/R, no una animación direccional completa por cada una de las 8
  direcciones. Las 8 direcciones se mantienen a nivel de **locomoción física y
  facing**, pero la silueta del cuerpo es siempre la vista frontal.
- Los enemigos (Mordeluz) **no** se rigearon en este hito: su ilustración es
  una criatura cuadrúpeda sin articulación bípeda equivalente. Su reacción de
  golpe se resuelve con stagger + knockback + flash + interrupción (ver
  implementation-summary), que sí es claramente visible sobre el sprite único.

Esta decisión respeta el mandato del hito: no se inventa una solución basada
únicamente en efectos, y las limitaciones reales de los assets quedan
documentadas en lugar de ocultarse.
