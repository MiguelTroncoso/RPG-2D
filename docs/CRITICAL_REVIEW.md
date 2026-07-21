# Lumbre de Nácar — Revisión crítica antes de Unity

> Versión: 0.2 · Estado: H0 aprobado; revisión aplicada en H1 · Fecha: 2026-07-19

Este documento revisa la propuesta anterior con una regla sencilla: antes de construir un MMORPG, debemos demostrar un bucle pequeño, entendible y medible. La regla de no crear código/proyecto correspondía a la revisión previa; H0 ya fue aprobado y H1 ejecutó únicamente el arranque técnico autorizado.

## 1. Diagnóstico de alcance

La propuesta original mezclaba tres productos distintos:

1. Un vertical slice offline.
2. Un MMORPG online persistente.
3. Una plataforma web con wiki, tienda, comunidad, administración y soporte.

Construir los tres a la vez produciría riesgo técnico y contenido superficial. Los siguientes sistemas quedan fuera del primer slice:

| Sistema | Problema para el primer slice | Decisión |
| --- | --- | --- |
| Mundo persistente online | Obliga a resolver autoridad, cuentas, reconexión, latencia y operación antes de validar diversión | Posponer |
| Cinco arquetipos | Multiplica animaciones, balance, UI, VFX y pruebas | Implementar uno |
| Varias regiones y streaming | Crea deuda de carga, chunks, memoria y navegación | Un mapa compacto, sin streaming |
| 20+ criaturas, élites y jefes | Exige catálogo, IA, animaciones, loot y QA | Una criatura común y una variante élite |
| Clanes, grupos, chat y amigos | Son sistemas sociales y de moderación completos | Posponer |
| Mercado entre jugadores | Es un sistema financiero con fraude, duplicación e inflación | Posponer |
| Profesiones, crafting y vivienda | Añaden muchas tablas y sumideros antes de probar el combate | Posponer |
| Drops aleatorios, temporadas y battle pass | Riesgo de diseño de monetización y balance prematuro | Botín directo y fijo |
| VIP | Puede acelerar indirectamente el poder y la economía | Solo diseñar reglas; no implementar |
| Web, wiki y panel admin | Producto separado con contenido y seguridad propios | Posponer |
| Microservicios y escalado horizontal | Optimización de arquitectura sin tráfico real | Modular monolith futuro |
| Anti-cheat completo | No existe amenaza online mientras el slice sea offline | Diseñar requisitos, no construirlo |

## 2. Vertical slice realista

### Objetivo

Una demo offline de **10–15 minutos** que comunique la fantasía de un MMORPG 2D isométrico sin prometer que ya es un MMO.

### Contenido incluido

- Una escena única en horizontal con tres zonas conectadas: plaza segura, sendero exterior y arena de cueva.
- Un personaje jugable: **Bastión de Brasa**, con una silueta, un arma y una paleta alternativa.
- Controles: joystick virtual en Android; teclado/mouse solo para pruebas dentro del editor.
- Cámara isométrica, navegación sobre grid y colisiones básicas.
- Un NPC que entrega una misión de combate.
- Una criatura común y una variante élite que reutiliza el mismo esqueleto y reglas base.
- Combate con ataque básico y dos habilidades: defensa breve y golpe de área.
- Vida, recurso, daño, derrota, respawn en la plaza y feedback audiovisual provisional.
- Objetivo: derrotar tres criaturas, entrar a la cueva, vencer la variante élite y regresar.
- Recompensa fija: una mejora de arma o accesorio; sin drops aleatorios.
- Inventario de seis espacios y una ranura de equipamiento relevante.
- Nivel 1 a 2 solamente; XP suficiente para demostrar la sensación de progreso.
- Guardado local versionado de nombre, nivel, equipo, misión y posición segura.
- UI de vida/recurso, objetivo, misión activa, inventario y pausa.
- Arte provisional original: tiles compactos, dos criaturas, un NPC, un avatar y efectos simples.

### Contenido explícitamente fuera

- Networking, cuentas, servidores, login, chat y persistencia online.
- Cinco arquetipos, creación de personaje completa y selección de mundo.
- Streaming, chunks, Addressables, multirregión y mundo abierto.
- Tienda, mercado, oro comerciable, crafting, profesiones y VIP.
- Drops aleatorios, battle pass, temporadas y eventos dinámicos.
- Web, wiki, administración, monetización y publicación.

### Regla de recorte

Si el slice no cabe en una sesión de 10–15 minutos, se elimina contenido antes de añadir tecnología. La calidad del control, combate, feedback, guardado y lectura visual tiene prioridad sobre la cantidad de mapas o sistemas.

## 3. Deuda técnica que debemos evitar

1. **Lógica dentro de `MonoBehaviour`:** dificulta pruebas y la migración al servidor. Las reglas de dominio deben vivir en C# sin `UnityEngine`.
2. **Movimiento basado solo en `Transform`:** hace difícil validar posiciones online. Usar coordenadas lógicas y comandos de intención desde el primer prototipo.
3. **Tiempo global y azar global:** cooldowns y drops deben depender de `IClock` y `IRandomSource` inyectables, aunque el slice use implementaciones locales.
4. **Guardado sin versión ni IDs estables:** usar un esquema versionado y IDs de datos desde el primer guardado.
5. **ScriptableObjects como estado vivo:** sirven para definiciones estáticas; el progreso del jugador debe ser un modelo serializable separado.
6. **Addressables antes de medir:** agrega complejidad de catálogo y carga. Se pospone hasta que el profiler demuestre necesidad.
7. **Singletons y eventos globales:** pueden ocultar dependencias y generar estados imposibles. Preferir servicios explícitos y eventos de dominio acotados.
8. **Microservicios prematuros:** cuando llegue online, comenzar con un servidor modular monolítico y extraer servicios solo si hay presión real.
9. **Compartir toda la lógica cliente-servidor sin límites:** se pueden compartir contratos y reglas verificables, pero el cliente nunca es autoridad; los secretos y decisiones finales viven en servidor.
10. **Migrar el save offline a la cuenta online:** el guardado local es una sandbox y no se convierte en progreso online automáticamente.

## 4. Arquitectura offline → online

La primera demo debe usar interfaces pequeñas, no una simulación artificial de todo el backend.

```text
Input/UI ──> IGameSession ──> Commands ──> Simulation ──> Domain Events ──> Presentation
                 │                         │
                 ├─ OfflineSession         └─ RemoteSession (futuro)
                 │                              │
                 └─ LocalSaveRepository         └─ Transport + servidor autoritativo
```

### Módulos

- **`Game.Domain`:** entidades, IDs, comandos, eventos, reglas de combate, progresión, estados y validaciones puras. Sin `UnityEngine`, red, archivos ni UI.
- **`Game.Application`:** `IGameSession`, casos de uso, reloj, azar, repositorios y límites de comandos.
- **`Game.Client`:** input, cámara, animaciones, HUD, audio y composición de escenas Unity.
- **`Game.Infrastructure.Local`:** JSON versionado, reloj local, azar con semilla y sesión offline.
- **`Game.Infrastructure.Remote` (futuro):** transporte, autenticación, snapshots, reconexión y sesión remota.
- **Servidor futuro:** proceso autoritativo que recibe intenciones, valida comandos, ejecuta la simulación y publica eventos/snapshots. Inicialmente será un modular monolith .NET o Unity headless solo si las pruebas lo justifican.

El cliente offline puede ejecutar la misma simulación para probar el juego. En online, `RemoteSession` reemplaza la ejecución local y el servidor decide daño, XP, oro, botín, posición válida y estado persistente. Los comandos y eventos se versionan para evitar rehacer la UI cuando llegue la red.

## 5. Rendimiento Android

Riesgos prioritarios:

- overdraw por fondos grandes, transparencias, sombras y paneles apilados;
- demasiadas etiquetas, números de daño, partículas y entidades activas;
- texturas HD sin atlas o con compresión inadecuada;
- `Instantiate/Destroy` durante combate y asignaciones por frame;
- IA y búsqueda de ruta actualizadas para todos los enemigos en cada frame;
- iluminación/postprocesado innecesarios para 2D;
- UI que redibuja todo el inventario o minimapa cada frame;
- memoria alta por mantener regiones completas cargadas;
- calentamiento, throttling y caída de FPS después de varios minutos.

Medidas para el slice:

- una escena pequeña, dos enemigos activos y efectos limitados;
- atlas solo cuando haya medición que lo justifique, pero evitar texturas duplicadas;
- pooling para proyectiles, daño y efectos repetidos;
- no usar postprocesado ni luces dinámicas complejas;
- actualizar HUD solo al cambiar estado;
- perfilar en un Android de referencia antes de ampliar contenido;
- objetivo de demo: 30 FPS sostenidos como mínimo durante 15 minutos en el dispositivo de referencia, sin bloqueos ni picos de frame mayores de 250 ms; registrar también memoria, temperatura y batería.

## 6. Seguridad online futura

No se implementa todavía, pero la arquitectura debe asumir:

- el cliente puede estar modificado;
- posición, velocidad, daño, XP, oro, botín y cooldowns son datos no confiables;
- autenticación, tokens, contraseñas y PII requieren almacenamiento y transporte seguros;
- inventario y economía necesitan operaciones idempotentes, transacciones y auditoría;
- habrá que protegerse contra replay, duplicación, speed hacks, teleport hacks, bots, spam, abuso de chat y manipulación de mercado;
- los administradores requieren permisos mínimos, doble confirmación para acciones sensibles y logs inmutables;
- límites de frecuencia y validación de tamaño deben aplicarse a cada endpoint/comando;
- ningún secreto, clave privada o regla de autoridad se entrega dentro del APK.

La primera prueba online, posterior a la demo, será de movimiento, combate, reconexión y persistencia con pocos jugadores. No se intentará lanzar la economía completa ni anti-cheat de producción en ese hito.

## 7. Criterios de aceptación de la primera demo

La demo se acepta solo si cumple todos los criterios críticos:

1. Arranca desde menú, entra a la escena y permite reiniciar sin errores críticos en consola.
2. En Android horizontal, el jugador mueve al avatar con joystick, ve la cámara isométrica y no atraviesa celdas bloqueadas.
3. El jugador acepta una misión, derrota tres criaturas, entra a la arena de cueva y derrota la variante élite.
4. El combate muestra objetivo, ataque, daño, vida, recurso, cooldown y derrota de forma legible.
5. La recompensa fija se recibe una sola vez, aparece en inventario y se puede equipar.
6. El jugador alcanza nivel 2 y el HUD refleja XP y nivel correctamente.
7. Al cerrar y abrir la demo, el guardado restaura misión, nivel, equipo e inventario sin duplicar recompensa.
8. Una persona que no conoce el proyecto completa el recorrido en 10–15 minutos sin instrucciones del desarrollador.
9. El dispositivo de referencia mantiene al menos 30 FPS durante 15 minutos, sin crash y sin frame hitch mayor de 250 ms.
10. Todos los assets utilizados son propios o tienen licencia registrada; no se incluye contenido de las referencias.

Un criterio fallido bloquea la siguiente fase. No se añade networking hasta que la demo sea estable y comprensible.

## 8. Aprobación solicitada — cerrada para H0

La aprobación explícita recibida autorizó:

- este vertical slice reducido;
- el arquetipo Bastión de Brasa como único personaje inicial;
- joystick como control principal en Android;
- una escena con plaza, sendero y cueva;
- botín fijo, nivel 1→2 y guardado local versionado;
- la arquitectura por comandos/sesiones indicada arriba;
- el dispositivo Android de referencia y el objetivo mínimo de rendimiento.

H1 se ejecutó con esos límites. El proyecto quedó preparado, sin gameplay ni sistemas H2. La siguiente aprobación necesaria es únicamente la entrada a H2 — greybox isométrico.
