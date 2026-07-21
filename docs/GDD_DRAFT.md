# Lumbre de Nácar — GDD inicial

> Versión: 0.1 · Estado: borrador de preproducción · Fecha: 2026-07-19

## 1. Resumen ejecutivo

**Lumbre de Nácar** es un MMORPG 2D isométrico de fantasía luminosa para Android, posteriormente adaptable a PC y otras plataformas. Está pensado para personas que disfrutan la exploración, la mejora gradual de su personaje y la cooperación espontánea sin exigir una conexión constante durante sesiones muy largas.

El primer objetivo no es simular un universo gigantesco, sino demostrar que un conjunto pequeño de sistemas produce una experiencia con identidad:

1. Salir de un asentamiento seguro.
2. Elegir una tarea visible y fácil de entender.
3. Explorar un espacio con rutas alternativas y secretos.
4. Combatir o recolectar con decisiones simples pero significativas.
5. Regresar para vender, fabricar, equipar y abrir nuevas rutas.
6. Encontrar a otros jugadores y cooperar cuando los objetivos se cruzan.

El producto debe sentirse generoso con el tiempo del jugador: derrotas recuperables, interfaz clara, economía trazable y recompensas que no obliguen a pagar.

## 2. Identidad del producto

### 2.1 Nombre provisional

Nombre de trabajo elegido: **Lumbre de Nácar**.

Alternativas consideradas:

- **Brumaria** — directo y fácil de recordar, aunque menos distintivo.
- **Veyra: Senderos de Luz** — enfatiza viaje y mundo, pero es más largo.
- **Las Agujas del Alba** — fuerte para una fantasía de descubrimiento, con tono más épico.
- **Ceniza de Marfil** — atmosférico, aunque sugiere un mundo más oscuro.

El nombre final debe validarse con búsqueda de marca, dominios, tiendas y disponibilidad legal antes de producir material público.

### 2.2 Visión

Un mundo persistente donde cada región tiene una economía, una silueta y un problema reconocibles. El jugador aprende el mundo por sus rutas, sonidos, habitantes y señales visuales, no por una lista interminable de sistemas.

### 2.3 Público objetivo

- Jugadores de 16 años o más interesados en fantasía y RPG online.
- Personas que disfrutaron MMORPG clásicos, pero prefieren una lectura visual más limpia y sesiones compatibles con móvil.
- Jugadores cooperativos casuales que quieren ayudar o comerciar sin depender de un grupo fijo.
- Usuarios de Android de gama media; el objetivo técnico inicial debe validarse con dispositivos reales.

No se propone inicialmente como un MMO competitivo hardcore, un juego de reflejos ni una experiencia de monetización agresiva.

### 2.4 Diferenciadores

1. **Cartografía viva:** cada región tiene una red de rutas de resonancia. Los jugadores reparan, protegen o redirigen estas rutas mediante eventos comunitarios, cambiando accesos, recursos y amenazas durante un periodo limitado.
2. **Cooperación de baja fricción:** objetivos públicos que recompensan contribuir, no solo formar un grupo antes de salir.
3. **Oficios conectados al mundo:** recolectar no es una actividad separada; los materiales reparan puentes, iluminación, talleres y puntos de transporte.
4. **Lectura móvil:** personajes, amenazas, interactuables y recompensas se distinguen a distancia sin saturar la pantalla.
5. **Economía comprensible:** pocos recursos base, sumideros visibles y registro de transacciones; el jugador debe entender por qué algo vale lo que vale.

### 2.5 Aprendizajes de las referencias

Las capturas aportadas muestran dos superficies relacionadas: un cliente/wiki de MMORPG con mucha profundidad sistémica y un portal oficial que funciona como portada, centro de noticias, descarga, comunidad y guía. Se toman como observación de producto, no como fuente de código, sprites, textos, nombres, layout exacto o identidad gráfica.

| Patrón observado | Adaptación original para Lumbre de Nácar | Momento |
| --- | --- | --- |
| Portada con ilustración de mundo, acceso, descarga y llamadas laterales | Portal con una ilustración propia de Puerto Candela, estado de servidores, registro, descarga y acceso a la biblioteca | Web posterior al MVP |
| Wiki con buscador, navegación lateral, índice de página y tarjetas relacionadas | Biblioteca de Astreva con búsqueda por región, criatura, objeto, misión y sistema; índice visible en escritorio y acordeón en móvil | Beta web |
| Panel de noticias y calendario de eventos | Diario de rutas con parches, eventos públicos, mantenimiento y cambios de economía, siempre con fecha y versión | MVP online / web |
| Misiones diarias por categorías y reinicio visible | **Contratos del día**: un contrato gratuito de exploración, combate o oficio; reinicio visible, sin obligación de completar todos | Post-prototipo |
| Drops clásicos y drops con giro aleatorio | Cofres de resonancia con tablas públicas, límites de duplicados cosméticos y registro de recompensas; sin poder de combate comprado | Post-MVP |
| Pantallas de entrenamiento acelerado | **Práctica de oficio**: una cola de acciones que reduce fricción y respeta límites diarios; no acelera estadísticas de combate ni se vende con ventaja | Diseño futuro |
| Mapa interactivo y paneles configurables | Mapa de rutas con marcadores propios, filtros de recursos, objetivos y eventos; paneles colapsables para no tapar el mundo | Post-MVP |
| Monturas y outfits con base/add-ons/completo | Colecciones cosméticas modulares: silueta base, accesorio y variante de color; nunca estadísticas | Post-MVP |
| Guía para principiantes y elección de vocación | Tutorial breve con sala de prueba; el jugador puede probar los cinco arquetipos antes de confirmar | Prototipo local |
| Mundos con reglas PvP diferentes y población visible | Regiones con reglas explícitas: segura, PvE con riesgo y arena opt-in; el portal muestra estado, versión y latencia aproximada | Online futuro |
| VIP con bonos de daño, experiencia, regeneración o habilidades | Se rechazan los bonos de combate. Se puede estudiar una membresía de conveniencia con aceleración moderada, topes y fuentes equivalentes para jugadores gratuitos | Política de producto |

La captura de transferencia bancaria no pertenece al diseño del juego. Se considera material sensible y no se almacena, reproduce ni incorpora al repositorio.

## 3. Principios de diseño

- Legibilidad antes que espectáculo.
- Decisiones pequeñas y frecuentes antes que árboles de sistemas interminables.
- El mundo debe enseñar mediante señales, NPC y cambios visibles.
- El servidor será la autoridad para todo lo que afecte a progreso o economía.
- La derrota debe crear una pausa o una elección, no un castigo irreversible.
- El PvP debe ser voluntario, localizado y balanceable.
- El contenido de pago no puede comprar una ventaja de combate directa.
- Un VIP puede ahorrar tiempo, pero no saltarse el juego: sus bonos de XP y oro deben ser moderados, limitados y auditables.
- Cada sistema nuevo debe justificar su coste de mantenimiento y su impacto en Android.
- Toda recompensa aleatoria debe tener reglas públicas, límites de frustración y un registro consultable.
- La documentación del juego debe ser parte del producto: enseñar sistemas es una forma de accesibilidad.

## 3.1 Auditoría crítica de alcance

La visión completa incluye un MMORPG online, una plataforma web y operación live ops. No se intentarán construir como un único primer entregable. El primer objetivo es un vertical slice offline que permita validar control, lectura isométrica, combate, recompensa, progresión mínima y guardado.

Quedan conscientemente fuera de ese slice: networking, cuentas, cinco arquetipos, varias regiones, streaming, mercado, crafting, profesiones, grupos, clanes, chat, PvP, temporadas, drops aleatorios, VIP, web, panel administrativo y publicación. Estos sistemas siguen en la visión de producto, pero no son requisitos para demostrar que el núcleo funciona.

La regla de alcance es: si un sistema no mejora directamente una sesión jugable de 10–15 minutos, se documenta y se pospone.

## 4. Modalidad estructural del mundo

### 4.1 Opción A — Mundo abierto continuo

Un gran mapa conectado con carga progresiva y fronteras casi invisibles.

**Ventajas:** alta fantasía de mundo único, navegación emergente y sensación de persistencia.

**Desventajas:** coste elevado de streaming, memoria, QA, navegación, sincronización y control de rendimiento; el contenido inicial puede sentirse vacío.

**Complejidad:** muy alta para un equipo pequeño.

**Riesgo principal:** invertir en tamaño antes de demostrar que el bucle de juego funciona.

### 4.2 Opción B — Mundo dividido por regiones

Cada bioma es una escena o conjunto de mapas con transiciones claras.

**Ventajas:** alcance controlable, carga y pruebas más simples, mejor control de densidad de jugadores y contenido.

**Desventajas:** las transiciones pueden romper la fantasía de continuidad; el diseño de rutas debe evitar que el mundo parezca un menú de niveles.

**Complejidad:** media-alta.

**Riesgo principal:** regiones desconectadas o con economías aisladas.

### 4.3 Opción C — Sistema híbrido recomendado

Regiones persistentes conectadas por caminos, puertas y puntos de transporte. Cada región se divide internamente en sectores cargables. Las transiciones son cortas, con continuidad de narrativa, clima, audio y rutas; el servidor mantiene la identidad del mundo aunque el cliente cargue un nuevo sector.

**Ventajas:** conserva la fantasía de un mundo conectado y permite controlar memoria, draw calls, población y complejidad de red. Facilita lanzar regiones nuevas sin rehacer el mapa completo.

**Desventajas:** requiere un buen contrato de transición, persistencia de estados y herramientas de edición de mapas; una mala transición puede sentirse artificial.

**Complejidad:** alta, pero adecuada para un producto escalable.

### 4.4 Recomendación

Adoptar el sistema híbrido. En la visión online se simula una región compuesta por asentamiento, exterior y mazmorra; el primer slice solo representa esas tres zonas en una escena compacta, sin streaming ni fronteras de red.

## 5. Bucle principal y progresión

### 5.1 Bucle principal

**Preparar → explorar → decidir → resolver → regresar → mejorar → abrir una ruta nueva.**

En la visión online, una sesión de 20 minutos podría incluir un contrato del asentamiento, una desviación hacia una cantera, un encuentro público y una mejora de oficio. En el primer slice se reduce a aceptar una misión, cruzar el sendero, combatir, vencer la arena de cueva, equipar una recompensa y volver.

### 5.2 Progresión inicial

- Nivel máximo de lanzamiento propuesto: **30**.
- Experiencia obtenida por misiones, combate, descubrimientos, eventos y primeras fabricaciones; se evita premiar el farmeo infinito de una sola criatura.
- Cada nivel concede una mejora pequeña y desbloquea decisiones, no una explosión de números.
- Habilidades activas en el lanzamiento online: 4 por arquetipo como máximo.
- Ranuras de equipo iniciales: arma, protección corporal, accesorio y herramienta de oficio; se agregan ranuras solo cuando mejoren la lectura.
- Calidad de equipo: común, trabajado, afinado y reliquia. La reliquia debe ser excepcional y no necesaria para completar el contenido normal.

Curva preliminar de experiencia:

```text
XP para el siguiente nivel = redondear(80 × nivel^1.55)
```

La fórmula es de referencia para pruebas, no una decisión final. La validación se hará con sesiones y telemetría: el jugador nuevo debería subir varios niveles en la primera hora sin sentirse obligado a repetir una única actividad.

### 5.3 Atributos

Para evitar una pantalla abrumadora, el personaje comienza con tres atributos:

- **Vigor:** vida máxima y resistencia a interrupciones.
- **Pulso:** daño de habilidades y regeneración de recurso.
- **Puntería:** precisión, crítico moderado y eficacia de recolección.

Derivados:

- Vida = 100 + 12 × nivel + 8 × Vigor.
- Recurso = 40 + 4 × nivel + 5 × Pulso.
- Daño base = arma + 1.2 × Pulso o Puntería según el arquetipo.
- Defensa = armadura + 0.7 × Vigor.

Los coeficientes son un punto de partida para balance, no deben exponerse como promesa al jugador hasta probarlos.

### 5.4 Muerte, recuperación y guardado

- En PvE, la derrota devuelve al jugador al último faro activado.
- Se pierde una pequeña cantidad de durabilidad temporal o tiempo de viaje; no se destruyen objetos ni se borra experiencia.
- El equipo se conserva y el botín no recogido queda en una bolsa recuperable durante un periodo breve.
- En zonas PvP se usan reglas separadas y previamente señalizadas.
- El progreso persistente se guarda en servidor en la futura versión online; el prototipo offline usará guardado local temporal con datos versionados.

## 6. Mundo inicial y mapas

### 6.1 Mundo: Astreva

Astreva está atravesada por filamentos de luz mineral llamados **lumbres**. No son una fuente de magia ilimitada: son un fenómeno natural que responde a presión, sonido, memoria y actividad de las comunidades. Los asentamientos se construyeron alrededor de nudos de lumbre, y sus rutas se debilitan cuando las personas abandonan una región.

### 6.2 Regiones propuestas

| Región | Propósito | Nivel | Enemigos/recursos | Conexiones |
| --- | --- | ---: | --- | --- |
| **Puerto Candela** | Ciudad principal, tutorial social, banco, mercado, forja, templo y transporte | 1–30 | materiales base, encargos, comercio | todas las rutas del MVP |
| **Aldea Vellón** | Comunidad agrícola y punto de descanso | 1–8 | fibras, semillas, fauna pequeña | Puerto Candela, Prado de Senda |
| **Aldea Marea Baja** | Pueblo costero, pesca y reparación naval | 8–18 | sal cristalina, conchas, algas | Costa de Umbral, Puerto Candela |
| **Prado de Senda** | Zona de principiantes y primer mapa exterior | 1–6 | bestias de pasto, madera, piedra | Puerto Candela, Vellón |
| **Bosque del Reverso** | Exploración, rutas ocultas y criaturas de emboscada | 5–12 | resina, hierbas, esporas | Prado, Mina de Aguja |
| **Mina de Aguja** | Recolección, verticalidad simulada y primer mini-jefe | 8–16 | mineral resonante, carbón azul | Bosque, Cueva del Eco |
| **Cueva del Eco** | Mazmorra corta, cooperación y lectura de sonido | 12–18 | cristales, reliquias, mini-jefe | Mina, Puerto mediante retorno |
| **Cordillera Vidriada** | Zona montañosa de riesgo ambiental | 16–24 | metal claro, hielo de lumbre | Mina, Costa, futuras regiones |
| **Pantano de las Campanas** | Debuffs, navegación lenta y eventos públicos | 18–26 | hongos, venenos, barro fértil | Bosque, Costa |
| **Costa de Umbral** | Exploración abierta, pesca, transporte y futuro PvP | 20–30 | sal, perlas opacas, restos de naufragio | Marea Baja, Cordillera |
| **Anfiteatro del Velo** | Arena PvP opt-in y tutorial de duelos | 10+ | recompensas cosméticas y marcas | Puerto Candela |

La zona desértica y el resto de mazmorras se reservan para una expansión posterior. La lista cumple la visión de mundo amplio sin forzar todo al primer slice.

### 6.3 Mapa del primer slice y expansión

El primer slice utiliza tres zonas visualmente conectadas en una sola escena:

1. **Plaza segura:** respawn, NPC de misión y salida.
2. **Sendero exterior:** rutas, obstáculos y encuentros con la criatura común.
3. **Arena de cueva:** espacio breve con la variante élite y recompensa fija.

La versión online futura podrá expandir estas zonas a Puerto Candela, Prado de Senda y Cueva del Eco con tiendas, recursos, eventos y varios sectores cargables.

### 6.4 Lógica espacial

- Tile lógico inicial: rombo isométrico de 64×32 px en una referencia de arte 2×; la resolución final se escala según plataforma.
- Coordenadas del mapa: `(x, y, z)` enteras, donde `z` representa nivel de suelo simulado.
- Capas: suelo, decoración bloqueante, decoración superior, entidades, efectos y UI.
- Colisiones: grid lógico separado del render, con celdas transitables, bloqueadas, escalera, puerta y transporte.
- Profundidad: orden de render por coordenada isométrica y altura; el servidor trabaja con celdas, no con píxeles.
- Tejados: ocultamiento por zona/volumen al entrar en interiores, nunca por transparencia global de toda la escena.
- Chunks: sectores pequeños y deterministas; el tamaño concreto queda para el prototipo tras medir memoria y draw calls.
- Sincronización futura: el servidor mantiene entidades relevantes por sector y envía snapshots priorizados; el cliente interpola únicamente movimiento autorizado.

## 7. Personajes jugables

Los cinco arquetipos iniciales son roles de fantasía propios, no clases con nombres prestados de otros juegos.

| Arquetipo | Fantasía y rol | Recurso | Fortaleza | Debilidad |
| --- | --- | --- | --- | --- |
| **Bastión de Brasa** | Protector de primera línea que convierte impactos en calor defensivo | Calor | aguante y control de espacio | baja movilidad |
| **Hiladora de Viento** | Exploradora que teje corrientes para reposicionar aliados | Impulso | movilidad y apoyo | daño sostenido menor |
| **Lector de Umbrales** | Cartógrafo místico que marca debilidades y altera rutas cortas | Trazos | utilidad y control | frágil si queda aislado |
| **Cantor de Raíces** | Sanador naturalista que intercambia vida, escudos y terreno | Savia | soporte y recuperación | requiere preparación |
| **Forjador de Chispas** | Artífice ofensivo de dispositivos y trampas | Carga | daño a distancia y zonas | vulnerable a presión cercana |

Cada arquetipo tendrá una silueta y color de acento distinto. Para la creación de personaje se separan arquetipo y apariencia: tono de piel, rostro, cabello, voz opcional, vestimenta inicial y dos paletas cosméticas. No se bloqueará el rol por género o apariencia.

Para el primer slice solo se implementa Bastión de Brasa con ataque básico, una defensa breve y un golpe de área. Movimiento/control, recuperación y los otros arquetipos quedan para una iteración posterior.

## 8. Criaturas y jefes iniciales

El catálogo completo de lanzamiento se diseñará más adelante; el siguiente conjunto sirve como dirección y alcance de prototipo.

### 8.1 Primer slice

- **Mordeluz:** fauna pequeña de pastizal; sirve como enemigo común y enseña objetivo, daño y derrota.
- **Mordeluz Resonante:** variante élite que reutiliza la base visual y añade una onda de peligro sencilla para la arena de cueva.

### 8.2 Dirección del catálogo de lanzamiento

Para la versión online futura se planifica un catálogo de 20 criaturas comunes, 10 intermedias, 5 élites, 5 mini-jefes, 3 jefes de mazmorra y 1 jefe mundial. Cada ficha definitiva deberá incluir hábitat, nivel, vida, daño, defensa, velocidad, patrón, detección, ataques, resistencias, debilidades, botín, experiencia, aparición, animaciones, efectos, sonido y riesgo de automatización.

La IA será autoritativa en servidor. El cliente solo muestra el estado recibido y puede hacer predicción visual no vinculante.

## 9. NPC, servicios y tiendas

### NPC de la visión de lanzamiento

- **Nara Velaquieta:** encargada del tablón de contratos; tutorial y misiones de salida.
- **Oren Martillo Claro:** forjador; tutorial de equipamiento y mejora básica.
- **Sio Páramo:** sanador; recuperación, reaparición y explicación de derrota.
- **Mira Tres Llaves:** banquera; almacenamiento y futura protección de transacciones.
- **Ivo Salitre:** comerciante general; compra y vende consumibles de baja complejidad.
- **Tarek del Umbral:** guardián de transporte; conecta rutas desbloqueadas.

### Reglas de tiendas

- El precio base será legible y estable cuando se implemente la economía inicial.
- Los vendedores compran a una fracción fija del precio y funcionan como sumidero de objetos.
- No se vende poder superior al obtenido jugando.
- Las futuras subastas o mercados entre jugadores tendrán límites, historial y confirmación explícita.
- Banco, inventario y comercio se implementarán con operaciones transaccionales en la versión online.

## 10. Economía

Monedas y recursos iniciales:

- **Fichas de cobre:** transacciones cotidianas y recompensas pequeñas.
- **Sellos de ruta:** progreso regional; no comerciables, usados para desbloquear servicios.
- **Materiales:** madera clara, piedra, fibra, mineral resonante y hierbas; se convierten en consumibles y mejoras.

Principios:

- Crear sumideros permanentes: reparaciones, fabricación, transporte y cosméticos.
- Evitar recursos con múltiples nombres para la misma función.
- Registrar cada creación y destrucción de moneda u objeto.
- No entregar recompensas duplicables por aceptar/rechazar una misión.
- Limitar transferencias nuevas y revisar anomalías en la fase online.

La monetización futura, si se aprueba, combinará cosméticos con una membresía VIP de conveniencia. El VIP podrá acelerar de forma moderada actividades ya disponibles para todos, pero nunca entregará estadísticas, equipo exclusivo, mejor calidad de botín o acceso a poder que no pueda obtenerse jugando.

### 10.1 Propuesta de VIP equilibrado

Nombre de trabajo: **Sello de Viajero VIP**. No entra en el MVP; primero se valida la progresión gratuita.

**Beneficios posibles:**

- Hasta **10% de XP adicional** únicamente en misiones, descubrimientos y primeras actividades del día; no se aplica a farmeo ilimitado de criaturas, PvP, rankings ni carreras de temporada.
- Hasta **5% de oro adicional** en recompensas de NPC y contratos con un tope diario; no se aplica a comercio entre jugadores, mercado, jefes, eventos competitivos ni creación de moneda sin límite.
- Una carga adicional de práctica de oficio o una cola más cómoda, sin multiplicar estadísticas ni producción de objetos raros.
- Más espacio de apariencia, títulos, emotes, decoración, tintes y monturas cosméticas.
- Reducción pequeña de fricción en viajes ya desbloqueados, nunca acceso a una zona, misión o recompensa exclusiva.

**Condiciones de equilibrio:**

- Los bonos son pequeños, visibles y con contador diario; el jugador siempre sabe cuánto le queda.
- Las mismas fuentes de XP y oro existen para jugadores gratuitos. El VIP mejora ritmo, no la tabla de recompensas.
- No se acumula con eventos de multiplicador sin un techo global; el servidor aplica el menor de los límites configurados.
- El bonus de XP/oro se desactiva en PvP, rankings, carreras de jefe mundial y cualquier actividad donde la velocidad de progresión afecte una clasificación.
- No otorga daño, vida, defensa, crítico, regeneración, precisión, rareza de botín, probabilidad de drop, más intentos de jefe ni ventaja de mercado.
- No se puede comprar oro directo ni intercambiar el beneficio VIP entre cuentas.
- Debe existir una vía gratuita de aceleración limitada, como la **Bendición del Fogón**, obtenida por jugar y con un límite menor.
- El backend registra cada bonus aplicado para detectar abuso, duplicación, inflación y bots.

**Criterio de rechazo:** si el VIP cambia quién puede completar contenido, competir en igualdad, dominar el mercado o alcanzar antes equipo relevante de forma determinante, el beneficio se elimina o se convierte en cosmético.

### 10.2 Contratos del día

Después del prototipo, cada cuenta podrá recibir un pequeño conjunto de contratos diarios rotativos:

- **Sendero:** descubrir un punto de interés o completar una ruta.
- **Vigilia:** derrotar criaturas en una zona recomendada.
- **Manos a la obra:** recolectar o fabricar un objeto sencillo.

El jugador podrá elegir uno o dos contratos, ver el tiempo restante para el reinicio y abandonarlos sin penalización permanente. El objetivo es dar dirección a una sesión corta, no imponer una rutina. No se añadirá un nivel premium con mejores estadísticas al lanzarse el sistema.

### 10.3 Recompensas y cofres de resonancia

El primer slice usa botín directo y predecible. En una fase posterior se pueden añadir cofres de resonancia para cosméticos, materiales o consumibles:

- tabla de recompensas visible antes de abrir;
- probabilidades o bandas de probabilidad comunicadas de forma clara;
- protección contra duplicados para cosméticos ya desbloqueados;
- registro de apertura y entrega en servidor;
- ningún cofre vendido puede contener una mejora de poder exclusiva;
- límites de gasto y confirmación antes de cualquier compra;
- alternativa de obtener cada cosmético por juego o por una ruta no aleatoria.

Esto conserva la emoción del descubrimiento sin convertir la progresión en una caja negra.

### 10.4 Práctica de oficio

La referencia muestra el valor de una actividad que reduce la fricción de entrenar. En Astreva se estudiará como una cola de práctica para recolección y fabricación: el jugador entrega materiales, define una orden y recibe el resultado después de un tiempo. La cola tendrá límites, no aumentará daño/vida y no se monetizará con multiplicadores de combate. Solo entra si la economía necesita una forma cómoda de procesar materiales.

## 11. Alcance del primer vertical slice

### Incluido

- Menú inicial simple y entrada a una escena única.
- Un personaje: Bastión de Brasa, con una silueta y una paleta alternativa.
- Un mapa compacto con plaza segura, sendero exterior y arena de cueva.
- Joystick virtual para Android; teclado/mouse solo en el editor.
- Cámara isométrica, grid lógico, navegación y colisiones.
- Un NPC que entrega una misión.
- Una criatura común y una variante élite que reutiliza componentes.
- Ataque básico y dos habilidades.
- Vida, recurso, daño, cooldown, derrota y respawn.
- Una recompensa fija, inventario de seis espacios y una ranura de equipamiento.
- XP y progreso de nivel 1 a 2.
- Guardado local JSON versionado y restauración tras reiniciar.
- HUD de vida/recurso, objetivo, misión, inventario y pausa.
- Arte provisional original y efectos básicos, sin assets descargados sin licencia.

### Fuera del primer slice

- Registro, login, networking, servidor, chat, grupos, clanes y amigos.
- Cinco arquetipos, creación de personaje completa y selección de mundo.
- Varias escenas, chunks, streaming y Addressables.
- Tienda, oro comerciable, mercado, crafting, profesiones y vivienda.
- PvP, rankings, temporadas, battle pass, drops aleatorios y VIP.
- Contratos diarios, práctica de oficio, monturas, mascotas, outfits modulares y eventos dinámicos.
- Web, wiki, panel administrativo, analítica productiva y Google Play.
- Arte final, música final, localización completa y contenido live ops.

### Criterio de éxito

Una persona nueva debe completar la misión, derrotar tres criaturas, vencer la variante élite, equipar la recompensa, alcanzar nivel 2 y recargar su progreso en una sesión de 10–15 minutos. Si no puede hacerlo de forma clara, no se amplía el alcance.

## 12. Arquitectura tecnológica preliminar

### Cliente

- Unity LTS estable seleccionada al iniciar producción y fijada en control de versiones.
- C# con módulos `Core`, `Gameplay`, `Presentation`, `UI`, `Data`, `Persistence`, `Audio` y `Configuration`.
- Tilemap isométrico, Input System, UI responsive y render 2D optimizado.
- ScriptableObjects para definiciones estáticas; DTO separados de modelos de dominio.
- Addressables y atlas de sprites solo después de medir la necesidad.
- Las reglas de dominio y contratos se mantienen en C# sin dependencia de `UnityEngine`.
- La sesión offline usa comandos, eventos, `IClock`, `IRandomSource` y repositorios reemplazables.
- El guardado local tiene IDs estables y versión de esquema desde el primer prototipo.

### Online futuro

- Servidor autoritativo separado del cliente.
- `RemoteSession` reemplaza la sesión local; la UI consume el mismo contrato y no conoce si la fuente es local o remota.
- El servidor recibe intenciones, valida comandos y decide daño, XP, moneda, botín, posición válida y persistencia.
- Comenzar con un modular monolith, no con microservicios; extraer servicios solo cuando el tráfico o la operación lo justifiquen.
- Servicios iniciales posteriores: autenticación, cuentas, personajes, mundo, combate, inventario/economía, chat y administración.
- API segura para operaciones de cuenta; transporte de tiempo real elegido tras pruebas de latencia.
- Base de datos relacional para identidad, personajes y transacciones; caché o cola solo cuando exista evidencia de necesidad.
- Logs de auditoría para inventario, economía, sanciones y acciones administrativas.
- Entornos separados para desarrollo, pruebas y producción; ningún secreto dentro del APK.

### Rendimiento objetivo

- 60 FPS cuando el dispositivo lo permita; modo de 30 FPS para gama baja.
- Pocas entidades visibles, pooling de objetos, atlas y culling por sector.
- No se habilitan streaming, postprocesado ni iluminación compleja sin una medición que lo justifique.
- Presupuesto de memoria, draw calls y temperatura definido a partir de un teléfono Android de referencia, todavía pendiente de elegir.

### Portal oficial y biblioteca futura

La web no será una réplica del cliente. Tendrá una arquitectura propia y ligera:

- portada visual con registro, descarga, estado de servidores y noticias;
- biblioteca/wiki con búsqueda, filtros, breadcrumbs e índice de contenido;
- calendario de eventos y notas de versión;
- mapa interactivo con filtros, no como imagen estática única;
- guías de inicio, arquetipos, criaturas, objetos, misiones y reglas de economía;
- comunidad y soporte, con moderación separada del servidor de juego;
- accesibilidad, responsive y localización prioritaria.

La web podrá consumir la misma API pública de lectura del juego, pero no debe exponer secretos ni convertirse en un panel administrativo. El portal se planifica después de demostrar el juego base.

## 13. Riesgos principales

| Riesgo | Impacto | Mitigación inicial |
| --- | --- | --- |
| Alcance de MMORPG demasiado grande | Muy alto | validar un vertical slice y congelar el MVP |
| Complejidad de red y autoridad del servidor | Muy alto | prototipo offline primero; contrato de dominio temprano |
| Rendimiento Android | Alto | dispositivo objetivo, perfilado y contenido sectorizado |
| Economía explotable | Alto | transacciones auditables, sumideros y pruebas de abuso |
| Contenido insuficiente al lanzamiento | Alto | eventos regionales y herramientas de datos, no solo más mapas |
| Producción de arte lenta | Alto | siluetas consistentes, kits de tiles y un estilo que tolere reutilización |
| Retención basada en presión | Medio-alto | progresión saludable, sesiones cortas y VIP acotado sin poder competitivo |
| Propiedad intelectual | Alto | assets originales, inventario de licencias y revisión de nombres |
| Soporte y operación | Alto | logs, reportes y panel posterior, con alcance limitado al principio |

## 14. Preguntas abiertas para aprobar la preproducción

1. ¿El nombre provisional y el tono de fantasía luminosa representan la identidad deseada?
2. ¿Se confirma el sistema híbrido de regiones como base de producción?
3. ¿Se acepta nivel máximo 30 y cinco arquetipos planificados, con un solo arquetipo en prototipo?
4. ¿El primer vertical slice debe priorizar combate o exploración si el tiempo obliga a recortar?
5. ¿Qué teléfono Android de gama media se usará como dispositivo de referencia?
6. ¿Se prefiere un modo de control principal por toque o joystick, dejando el otro como alternativa?
7. ¿Se mantendrá solo PvE en el primer prototipo?
8. ¿Qué modelo de monetización se quiere explorar después de demostrar diversión y retención?
9. ¿El juego tendrá localización inicial en español, portugués, inglés o una combinación?
10. ¿Cuál es el equipo y presupuesto objetivo para estimar mejor calendario y operación?

## 15. Criterios de preproducción terminada

La Fase 0 se considera aprobada cuando:

- La identidad, el nombre de trabajo y el público objetivo están aceptados.
- La modalidad híbrida, la orientación móvil y el principio PvE están confirmados.
- El alcance del primer vertical slice tiene una lista cerrada de incluido/fuera.
- Existe un arquetipo, un mapa de prueba, una criatura común y una variante élite aprobados como dirección.
- Los riesgos principales tienen propietario y mitigación.
- Se ha elegido un dispositivo Android de referencia.
- Se han registrado las decisiones técnicas abiertas y las alternativas descartadas.
- El siguiente hito tiene entradas, archivos esperados, pruebas y criterio de salida.
