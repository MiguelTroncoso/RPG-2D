# Lumbre de Nácar — Guía de arte inicial

> Versión: 0.2 · Estado: dirección base validada en H7 · Fecha: 2026-07-20

## 1. Intención visual

La identidad visual debe comunicar **fantasía cálida, misterio natural y aventura legible en una pantalla pequeña**. La referencia de la captura ayuda a observar la densidad de personajes, la perspectiva isométrica, la presencia de nombres sobre entidades y una jerarquía de llamada a la acción. El proyecto desarrollará una composición, marca, iconografía, personajes, escenarios y UI propios.

El resultado deseado es pixel art HD estilizado: bordes nítidos, formas simples en primer vistazo, volumen suficiente para leer iluminación y equipo, y detalles reservados para acercar la mirada.

## 1.1 Patrones de referencia que sí se reutilizan

- **Ilustración como marco:** una escena panorámica puede presentar el mundo, pero debe usar personajes, arquitectura, colores y composición propios.
- **Lectura por paneles:** la información densa funciona mejor si se divide en módulos con títulos, índices y estados de apertura; en móvil se convierte en acordeón.
- **Contraste de superficies:** el cliente de juego puede ser atmosférico; la biblioteca web puede usar un fondo oscuro y paneles sobrios para priorizar lectura.
- **Jerarquía de llamadas:** descarga, iniciar sesión, estado del servidor, noticias y comunidad deben tener una prioridad clara, sin llenar toda la portada de botones.
- **Colecciones:** mostrar outfits, monturas y recompensas en tablas o tarjetas ayuda a comprender progresión cosmética; las miniaturas deberán ser originales.

No se copiarán la combinación exacta de azul/dorado, marcos, logos, ilustraciones, nombres de secciones, iconos, personajes, fondos ni la distribución pixel a pixel de las referencias.

## 2. Reglas de propiedad intelectual

- No se copian sprites, tiles, personajes, monstruos, mapas, nombres, logotipos, tipografías propietarias, música, textos, misiones, código o bases de datos de RubinOT, Tibia ni otros juegos.
- La captura y la URL de referencia no son fuentes de assets.
- Todo asset se crea internamente o se incorpora con licencia documentada, autor, fecha, alcance, archivo de licencia y restricciones.
- Los recursos de IA, si se usan, deben revisarse, transformarse cuando corresponda y tener trazabilidad de su origen y términos.
- No se incluirán marcas o nombres de terceros en material público del juego.

## 3. Cámara y grid

- Perspectiva isométrica 2:1, con rombos lógicos de 64×32 px en el master de arte provisional.
- Escala de referencia: producir a 2× de la resolución de visualización del prototipo para conservar nitidez al reducir.
- Cámara horizontal con zoom limitado; el zoom no debe destruir la legibilidad de nombres, indicadores o botones.
- Personajes anclados a una celda lógica; pies y sombras ayudan a leer profundidad.
- Las diagonales principales de caminos y techos deben respetar el grid para evitar vibración visual.

La relación exacta entre pixel lógico, píxel de pantalla y densidad Android se validará con un dispositivo real. La nitidez es un requisito, no una dependencia de una resolución única.

## 4. Paleta provisional

Paleta base de alto contraste, con luz cálida contra ambientes fríos:

| Uso | Color guía | Intención |
| --- | --- | --- |
| Lumbre principal | `#F6C65B` | energía, recompensa, navegación |
| Cielo/bruma | `#22304A` | profundidad, interfaz nocturna |
| Azul mineral | `#3F7EA6` | agua, rutas, estados informativos |
| Verde musgo | `#5E8A62` | bosque, recursos naturales |
| Coral de peligro | `#D95A4F` | daño, amenaza, alertas |
| Marfil de lectura | `#F5EBDD` | texto principal y brillos |
| Tinta | `#151A24` | contornos, paneles y sombra |

Las muestras son guía de diseño, no una paleta final bloqueada. Cada bioma tendrá una paleta secundaria, manteniendo un color de interacción común y un color de peligro común.

## 5. Escenarios

### Puerto Candela

Piedra gris azulada, madera tostada, telas mostaza, luminarias suspendidas y geometrías de nudo. Debe sentirse seguro sin verse plano. El centro de la plaza será un gran faro bajo, que funciona como landmark y punto de reaparición.

### Prado de Senda

Verdes desaturados, tierra cálida y flores de lumbre pequeñas. Siluetas de árboles separadas para no formar una pared de ruido. Los caminos tendrán bordes repetibles y señales verticales reconocibles.

### Cueva del Eco

Azules profundos, tiza pálida y brillos ámbar. Las zonas de peligro se leen por ondas y polvo, no solo por un círculo rojo. La arquitectura debe enseñar la idea de sonido y resonancia a través de ritmo visual.

### Regiones posteriores

Bosque del Reverso: verdes oscuros y morados. Mina de Aguja: cobre, azul mineral y carbón. Cordillera Vidriada: blanco sucio, cian y sombras violetas. Pantano de las Campanas: verde ácido controlado, barro y metal oxidado. Costa de Umbral: arena fría, índigo y reflejos coral.

## 6. Personajes y criaturas

- Siluetas identificables en menos de un segundo.
- El cuerpo y el arma deben conservar un contorno claro a distancia.
- Los nombres sobre entidades se reservan para jugadores, NPC relevantes y objetivos; no se llena la pantalla de etiquetas permanentes.
- El color de clase/arquetipo no debe ser la única forma de distinguir un personaje.
- Equipo y cosméticos cambian bloques de color grandes antes que añadir ruido de píxel.
- Animaciones de ataque: anticipación breve, impacto legible y recuperación clara.
- Criaturas con patrón distinto: huir, emboscar, zona, carga, apoyo o jefe por fases.
- Las sombras son blandas y sencillas; no competirán con la celda transitable.

## 7. UI y UX visual

La UI debe parecer parte de Astreva: paneles de tinta azul, marcos de metal claro, botones con pequeños nudos de lumbre y pictogramas simples.

Para la futura web de Astreva se explorará una variante más editorial: fondo oscuro, paneles de lectura, navegación lateral, buscador global y un índice contextual. Será una identidad hermana de la UI del juego, no una copia de la wiki de referencia.

### Jerarquía móvil

1. Vida, recurso y estado de combate.
2. Movimiento y ataque/acción primaria.
3. Objetivo seleccionado y telemetría de daño.
4. Minimapa y dirección de objetivo.
5. Acceso a inventario, misión, chat y ajustes.

Reglas:

- Área táctil mínima recomendada: 44 dp, ajustada tras pruebas de usabilidad.
- No depender de texto pequeño para comunicar peligro o éxito.
- El estado seleccionado siempre debe tener una señal visual y otra sonora opcional.
- Las ventanas modales deben poder cerrarse con una acción evidente.
- Reservar espacio para notch, barras del sistema y relaciones de aspecto anchas.
- No ocultar el mundo completo para una acción cotidiana.

### Biblioteca y portal

- En escritorio: navegación lateral, contenido central y un índice contextual limitado.
- En móvil: navegación colapsada, búsqueda persistente y tarjetas apiladas.
- Los paneles deben conservar una anchura cómoda para lectura; evitar tablas que requieran zoom constante.
- Las noticias deben mostrar fecha, versión, tipo y estado: publicado, programado o archivado.
- El estado del servidor debe usar texto además de color: operativo, mantenimiento, degradado o fuera de servicio.
- Los mapas deben tener una versión de alto contraste y una lista alternativa de ubicaciones.

## 8. Tipografía e iconografía

- Usar una sans serif legible con licencia compatible para cuerpo y una display angular propia o licenciada para títulos.
- Evitar tipografías excesivamente decorativas en botones pequeños.
- Iconos de 24/32/48 px con dos grosores de trazo y silueta reconocible en monocromo.
- Color, forma y texto deben reforzarse mutuamente; nunca codificar rareza o estado solo con color.

La fuente final se elegirá después de validar lectura en Android de gama media. Se guardará la licencia junto a los metadatos del asset.

## 9. Animación y efectos

- Animaciones cortas y expresivas, con pocos frames pero poses claras.
- Parallax moderado en fondos; nunca debe desplazar la referencia de las celdas.
- Efectos de golpe con un color de acento y una forma asociada al tipo de daño.
- Partículas con límites por sector y pooling; no crear miles de objetos por evento.
- Lumbre: pulsación suave y ruido mínimo, no un brillo constante que fatigue la vista.
- Priorizar feedback de interacción, ataque, impacto, muerte, recompensa y transición.

## 10. Audio provisional

La música final no se produce en este hito. La dirección busca instrumentación acústica sencilla con capas ambientales por región. Los sonidos de interacción deben distinguir confirmación, error, peligro, misión lista, impacto y botín. Se evitarán sonidos demasiado parecidos a referencias conocidas.

### 10.1 Aplicación H7

H7 usa audio procedural local como placeholder trazable: pasos, ataque, impacto, muerte, misión, equipamiento, subida de nivel, UI y tres ambientes con crossfade entre plaza, sendero y cueva. Estos tonos validan ritmo y jerarquía, pero no se consideran banda sonora ni assets finales.

## 11. Rendimiento y producción

- Atlas por familia de tiles y por región cuando las mediciones lo justifiquen.
- Pooling de criaturas, proyectiles, números de daño y efectos.
- Culling por sector y límite de entidades visibles.
- Texturas nítidas con compresión compatible; comprobar que no se emborronen en Android.
- Evitar transparencias grandes y capas superpuestas innecesarias.
- Mantener una escena de benchmark con densidad alta para probar el peor caso.
- Registrar memoria, FPS, draw calls, tiempo de carga y batería en cada hito visual.
- H7 mantiene un pool de VFX prewarmado y evita `Instantiate/Destroy` por evento; el perfilado real queda para H9.

### 11.1 Aplicación H7

El arte base original se importa como sprites con transparencia, sin mipmaps y con límite de 2048 px. El backdrop es una sola pieza, los visuales de greybox se ocultan sin retirar sus colliders y la presentación no modifica el grid lógico ni la navegación.

### 11.2 Aplicación H8

H8 conserva el arte base como una escena pequeña: cinco texturas de presentación, un backdrop, Animator con culling y un pool de VFX acotado. La importación usa compresión móvil y evita mipmaps/readable; el Canvas desactiva canales adicionales y el HUD limita refrescos de texto. No se introduce todavía un atlas ni Addressables porque la escena actual no justifica añadir esa dependencia sin una medición de H9.

## 12. Checklist de asset

Cada asset debe incluir:

- ID y nombre descriptivo.
- Tipo, resolución, escala y región.
- Autor y fecha.
- Licencia o confirmación de creación interna.
- Dependencias y atlas.
- Estado: boceto, provisional, aprobado o retirado.
- Presupuesto aproximado de memoria y draw calls si aplica.
- Captura de uso en contexto.

## 13. Criterios de aprobación visual

La dirección visual inicial está lista cuando:

- Un jugador distingue suelo, bloqueo, entidad, interactuable y peligro sin leer un manual.
- Los tres sectores del MVP se reconocen por color, landmark y silueta.
- El personaje, la criatura común y la variante élite son distinguibles a la escala de juego.
- La UI sigue siendo usable en un teléfono pequeño y una tablet.
- El estilo puede producir contenido adicional sin depender de una ilustración única.
- Todos los assets utilizados tienen origen trazable y no derivan de la referencia externa.
