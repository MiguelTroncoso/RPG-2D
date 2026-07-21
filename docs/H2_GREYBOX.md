# H2 — Greybox isométrico

## Alcance

H2 construye exclusivamente la lectura espacial de la primera escena: plaza segura, sendero exterior y cueva. No incluye avatar, joystick, combate, NPC, inventario, misiones, arte final ni guardado.

## Layout lógico

- Grid: 32×20 celdas.
- Zonas: `Plaza`, `Trail` y `Cave`.
- Inicio de navegación: `(3,10)` en plaza.
- Objetivo de navegación: `(28,10)` en cueva.
- Obstáculos lógicos: bloque 2×2 en plaza y bloque 2×1 en cueva.
- Navegación: BFS de cuatro direcciones con presupuesto máximo de 640 nodos.

## Representación visual

La escena usa tres meshes agregadas con materiales planos de color y una línea de preview de navegación. No hay sprites, tiles finales, texturas externas ni un objeto Unity por celda. Esto mantiene bajo el coste estructural del greybox y deja una base medible para H9.

## Colisiones y cámara

La escena incluye cuatro límites `BoxCollider2D` y seis `PolygonCollider2D` para los obstáculos del grid. La cámara principal tiene `CinemachineBrain` y la cámara virtual oficial de Cinemachine; el encuadre es ortográfico y horizontal.

## Aceptación H2

- La escena `VerticalSlice.unity` abre sin errores.
- Se distinguen las tres zonas por geometría y color de greybox.
- La ruta lógica conecta plaza y cueva y evita las celdas bloqueadas.
- Las colisiones están separadas del mesh visual.
- La escena contiene como máximo tres meshes de zona y una línea de navegación.
- No se incorporan arte final, combate, NPC, inventario ni control del jugador.
- Los tests de navegación pasan y el proyecto compila en batchmode.

## Resultado

H2 completado: la escena y el contrato lógico cumplen el alcance aprobado. La validación automática registró 4 tests pasados y 0 fallidos. El siguiente hito queda bloqueado hasta aprobar H3.
