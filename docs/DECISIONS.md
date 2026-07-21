# Lumbre de Nácar — Registro de decisiones

> Las decisiones que afecten alcance, plataforma, arquitectura, economía o identidad deben registrarse aquí. Las decisiones de este documento son provisionales hasta aprobación explícita.

## DEC-0001 — Documento de trabajo y alcance inicial

- **Fecha:** 2026-07-19
- **Estado:** propuesta
- **Problema:** la visión solicita un MMORPG completo, pero implementarlo de una vez ocultaría riesgos y haría imposible validar el bucle principal.
- **Alternativas:** construir todo en una única ejecución; diseñar primero un vertical slice; comenzar por backend.
- **Decisión:** iniciar con preproducción documentada y luego un prototipo local offline, antes de networking y backend productivo.
- **Motivo:** reduce riesgo, permite pruebas de diversión y respeta el orden de fases solicitado.
- **Consecuencias:** no habrá cuentas reales, multijugador ni monetización en el primer hito; habrá que conservar contratos de dominio que puedan migrarse a online.

## DEC-0002 — Identidad original

- **Fecha:** 2026-07-19
- **Estado:** propuesta
- **Problema:** las referencias evocan convenciones de MMORPG isométrico, pero no deben convertirse en una copia visual, narrativa o técnica.
- **Alternativas:** reproducir una estética de pixel art clásico; usar 3D isométrico genérico; crear pixel art HD con identidad propia.
- **Decisión:** desarrollar fantasía luminosa original en Astreva, con lumbres, rutas de resonancia, siluetas propias y UI móvil propia.
- **Motivo:** preserva la intención de género sin reutilizar nombres, mapas, sprites, textos, código, logos o bases de datos de terceros.
- **Consecuencias:** se requiere una guía de arte y un inventario de assets desde el comienzo; cualquier asset externo debe tener licencia verificable.

## DEC-0003 — Modalidad híbrida del mundo

- **Fecha:** 2026-07-19
- **Estado:** propuesta
- **Problema:** un mundo abierto continuo tiene un coste alto de streaming, memoria, QA y sincronización para un primer producto móvil.
- **Alternativas:** mundo abierto continuo; regiones aisladas; sistema híbrido de regiones persistentes y sectores cargables.
- **Decisión:** usar sistema híbrido.
- **Motivo:** conserva la sensación de mundo conectado y permite controlar rendimiento, población, carga y lanzamiento incremental.
- **Consecuencias:** habrá que diseñar transiciones, persistencia de estados, chunks y herramientas de edición; el MVP solo necesita una región con tres sectores.

## DEC-0004 — Android horizontal como primera plataforma

- **Fecha:** 2026-07-19
- **Estado:** propuesta
- **Problema:** la lectura isométrica y el HUD de combate requieren espacio horizontal, pero el producto debe funcionar en móviles.
- **Alternativas:** vertical; horizontal; ambas orientaciones desde el primer hito.
- **Decisión:** orientar primero a horizontal y diseñar UI adaptable a distintas relaciones de aspecto.
- **Motivo:** mejora el campo de visión y la separación entre navegación, combate y paneles sin reducir botones táctiles.
- **Consecuencias:** se debe probar en teléfonos pequeños y tablets; el soporte vertical queda fuera del MVP salvo que las pruebas lo vuelvan imprescindible.

## DEC-0005 — PvE como eje y PvP opt-in

- **Fecha:** 2026-07-19
- **Estado:** propuesta
- **Problema:** el PvP abierto incrementa complejidad de balance, toxicidad, soporte y riesgo de abuso.
- **Alternativas:** PvP abierto; solo PvE; PvE principal con arena y zonas PvP señalizadas.
- **Decisión:** PvE principal con PvP voluntario y regulado fuera del primer prototipo.
- **Motivo:** permite validar exploración, cooperación y progresión sin comprometer el tono accesible.
- **Consecuencias:** el balance competitivo no será criterio de éxito del MVP; arena y zonas de riesgo se planifican después.

## DEC-0006 — Progresión inicial acotada

- **Fecha:** 2026-07-19
- **Estado:** propuesta
- **Problema:** un RPG con demasiados atributos, ranuras y rarezas es difícil de leer en móvil y de balancear.
- **Alternativas:** sistema profundo desde el día uno; progresión mínima sin decisiones; tres atributos y nivel máximo inicial 30.
- **Decisión:** tres atributos (`Vigor`, `Pulso`, `Puntería`), cuatro niveles de calidad y nivel máximo inicial 30.
- **Motivo:** ofrece dirección de build sin crear una hoja de cálculo obligatoria.
- **Consecuencias:** se puede ampliar después, pero cualquier atributo nuevo debe demostrar valor en decisiones jugables.

## DEC-0007 — Monetización no competitiva y VIP acotado

- **Fecha:** 2026-07-19
- **Estado:** propuesta
- **Problema:** vender poder puede dañar el balance, la confianza y la retención saludable.
- **Alternativas:** venta de poder; suscripción; cosméticos y servicios de conveniencia no competitivos; VIP con aceleración moderada y topes.
- **Decisión:** explorar, después del MVP, cosméticos y un VIP acotado. El VIP podrá ahorrar tiempo mediante pequeños bonus de XP u oro en fuentes controladas, pero no venderá estadísticas, equipo, rareza de botín, acceso exclusivo ni ventaja competitiva.
- **Motivo:** permite ofrecer valor a quienes apoyen el servicio sin convertir el pago en una ruta obligatoria de poder.
- **Consecuencias:** no habrá tienda real en preproducción; primero se valida la progresión gratuita. Los bonos de VIP requieren límites diarios, fuentes equivalentes gratuitas, exclusión de PvP/rankings y telemetría contra inflación y abuso.

## DEC-0008 — Servidor autoritativo como requisito futuro

- **Fecha:** 2026-07-19
- **Estado:** requisito, arquitectura pendiente
- **Problema:** daño, experiencia, monedas y botín son objetivos directos de trampas si el cliente decide sus resultados.
- **Alternativas:** cliente autoritativo; servidor autoritativo; modelo híbrido sin límites claros.
- **Decisión:** el servidor será la autoridad para movimiento validado, combate, inventario, economía, misiones y persistencia.
- **Motivo:** es un requisito de seguridad y de integridad del mundo persistente.
- **Consecuencias:** el coste de infraestructura y pruebas es mayor; el prototipo offline debe separar presentación y reglas para facilitar la migración.

## DEC-0009 — Referencias externas limitadas a observación

- **Fecha:** 2026-07-19
- **Estado:** aceptada
- **Problema:** la captura y la URL compartidas son útiles para entender la categoría, pero no autorizan la reutilización de contenido.
- **Alternativas:** descargar o extraer recursos; replicar interfaz; observar convenciones generales y producir assets propios.
- **Decisión:** usar la captura y la referencia web solo como inspiración de perspectiva, densidad visual, jerarquía y expectativas de género. No se copia contenido.
- **Motivo:** protege la identidad y la propiedad intelectual del proyecto.
- **Consecuencias:** la URL `https://rubinot.com.br/news` no se incorporará como dependencia. El acceso automatizado a la página fue bloqueado por Cloudflare, por lo que el análisis se limita a la captura y a convenciones generales del género.

## DEC-0010 — Estado de los artefactos iniciales

- **Fecha:** 2026-07-19
- **Estado:** propuesta
- **Problema:** el repositorio estaba vacío y no existe todavía un proyecto Unity generado.
- **Alternativas:** generar proyecto Unity inmediatamente; crear documentación y aprobar alcance primero.
- **Decisión:** crear inicialmente solo `README.md`, `docs/GDD_DRAFT.md`, `docs/ROADMAP.md`, `docs/DECISIONS.md` y `docs/ART_GUIDE_DRAFT.md`.
- **Motivo:** coincide con el entregable inicial solicitado y evita inventar decisiones de implementación prematuras.
- **Consecuencias:** el siguiente paso requiere aprobación del usuario antes de crear estructura de proyecto, escenas o código.

## DEC-0011 — Inspiración funcional de las nuevas capturas

- **Fecha:** 2026-07-19
- **Estado:** propuesta
- **Problema:** las nuevas referencias muestran sistemas de servicio, documentación, recompensas, cosméticos, mapas y onboarding que pueden aportar ideas, pero copiarlos literalmente dañaría la identidad del proyecto.
- **Alternativas:** replicar la web y los sistemas observados; ignorar todas las referencias; extraer patrones funcionales y rediseñarlos con reglas propias.
- **Decisión:** adoptar como referencia funcional un portal con noticias, estado, biblioteca, mapa, guía inicial y comunidad; estudiar contratos diarios, drops transparentes, colecciones cosméticas y práctica de oficio como sistemas posteriores.
- **Motivo:** son patrones útiles para un MMORPG persistente y para reducir fricción de aprendizaje, siempre que el contenido, la marca, el arte y las reglas sean originales.
- **Consecuencias:** el portal se separa del cliente y entra después del MVP; toda recompensa aleatoria debe ser transparente; las monturas/outfits serán cosméticos; la monetización no otorgará ventajas de combate.

## DEC-0012 — VIP de conveniencia con límites

- **Fecha:** 2026-07-19
- **Estado:** requisito de producto
- **Problema:** una membresía puede financiar el servicio y reducir fricción, pero los bonos de XP u oro pueden acelerar indirectamente el acceso al poder y alterar la economía.
- **Alternativas:** no ofrecer VIP; vender solo cosméticos; permitir VIP con aceleración moderada, topes, fuentes equivalentes gratuitas y exclusión de actividades competitivas.
- **Decisión:** estudiar un VIP de conveniencia. Podrá aportar un bonus pequeño y limitado de XP y oro en fuentes controladas, además de almacenamiento cosmético, títulos, emotes, decoración y viajes ya desbloqueados. No podrá entregar daño, defensa, equipo, rareza de botín, ventajas de PvP ni control del mercado.
- **Motivo:** respeta la intención de ofrecer valor a una cuenta VIP sin convertir el pago en una ruta obligatoria de poder.
- **Consecuencias:** los topes, las exclusiones y los bonos deben ser configurables en servidor; la economía necesita telemetría y pruebas de inflación. La decisión final queda pendiente de validar primero la progresión gratuita.

### Límites preliminares del VIP

- XP adicional: hasta 10% en fuentes seleccionadas, con tope diario.
- Oro adicional: hasta 5% en recompensas controladas, con tope diario.
- Sin bonus en PvP, rankings, jefes mundiales, mercado entre jugadores ni tablas de botín.
- Los jugadores gratuitos conservan acceso a las mismas actividades y a una aceleración gratuita más pequeña.
- Si un bono cambia de forma relevante la competitividad o la inflación, se retira.

## DEC-0013 — Tratamiento de captura sensible

- **Fecha:** 2026-07-19
- **Estado:** aceptada
- **Problema:** una de las imágenes compartidas contiene detalles de una transferencia bancaria ajena al proyecto.
- **Alternativas:** conservarla como referencia; copiar datos en documentación; descartarla.
- **Decisión:** descartarla del análisis de producto y no guardar, reproducir ni transcribir sus datos.
- **Motivo:** no aporta diseño y contiene información financiera personal.
- **Consecuencias:** la documentación solo incorpora observaciones de las capturas relacionadas con el juego y su portal.

## DEC-0014 — Reducción a vertical slice realista

- **Fecha:** 2026-07-19
- **Estado:** propuesta para aprobación
- **Problema:** el MVP anterior todavía reunía tres regiones, varias criaturas, tienda, sanador, forja, múltiples controles y demasiados sistemas para una primera demo independiente.
- **Alternativas:** mantener el MVP amplio; construir un prototipo técnico sin experiencia completa; reducirlo a una sesión offline de 10–15 minutos.
- **Decisión:** el primer entregable será una escena única con plaza, sendero y cueva; un arquetipo; un NPC; una criatura común; una variante élite; una misión; combate mínimo; recompensa fija; nivel 1→2 e inventario/guardado local.
- **Motivo:** permite validar la fantasía y medir control, combate, lectura, rendimiento y persistencia local antes de pagar la complejidad del online.
- **Consecuencias:** cinco arquetipos, regiones, economía, VIP, drops, web, social y servidores quedan documentados pero no implementados en la demo.

## DEC-0015 — Contratos compartidos, autoridad separada

- **Fecha:** 2026-07-19
- **Estado:** propuesta para aprobación
- **Problema:** empezar offline con reglas acopladas a Unity produciría una reescritura al crear un servidor autoritativo.
- **Alternativas:** hacer toda la lógica en MonoBehaviours; construir networking desde el primer día; separar dominio, sesión y presentación y usar una sesión local reemplazable.
- **Decisión:** mantener dominio y contratos en C# sin `UnityEngine`; la UI habla con `IGameSession`; el slice usa `OfflineSession` y el online futuro usará `RemoteSession` con los mismos comandos/eventos versionados.
- **Motivo:** conserva velocidad de prototipado sin confundir la simulación local con autoridad online.
- **Consecuencias:** el guardado offline no se migra automáticamente a cuentas online; el servidor futuro debe recalcular resultados y el cliente solo muestra estado autorizado.

## DEC-0016 — Puerta de aprobación antes de Unity

- **Fecha:** 2026-07-19
- **Estado:** requisito de proceso
- **Problema:** crear un proyecto y código antes de cerrar alcance puede fijar decisiones equivocadas y crear deuda temprana.
- **Alternativas:** generar el proyecto inmediatamente; generar solo después de aprobar el vertical slice, dispositivo y criterios de aceptación.
- **Decisión:** no crear el proyecto Unity ni escribir código hasta recibir aprobación explícita del alcance reducido y de la arquitectura offline→online.
- **Motivo:** respeta el proceso de fases y evita que el entusiasmo por implementar sustituya la validación del producto.
- **Consecuencias:** esta revisión termina con una solicitud de aprobación; el siguiente hito técnico empieza solo después de esa respuesta.

## DEC-0017 — Unity 6.3 LTS como línea de H1

- **Fecha:** 2026-07-19
- **Estado:** aprobada para H1
- **Problema:** H1 necesita un editor estable, pero el usuario pidió no convertir una revisión concreta en una dependencia innecesaria.
- **Decisión:** usar la versión LTS estable más reciente disponible en Unity Hub al iniciar H1. En este entorno se resolvió Unity 6.3 LTS (`6000.3.20f1`) y se registra la revisión efectiva para que el proyecto sea reproducible.
- **Motivo:** combina estabilidad con una política explícita de actualización; la revisión solo se fija cuando el proyecto de Unity la necesita para abrirse sin migración implícita.
- **Consecuencias:** una actualización futura de Unity será una decisión registrada y deberá repetir la validación de compilación y escenas.

## DEC-0018 — Input System preparado para todas las plataformas objetivo

- **Fecha:** 2026-07-19
- **Estado:** aprobada para H1
- **Decisión:** el proyecto usa el Input System oficial con acciones para touch/joystick virtual, teclado, mouse y gamepad desde el inicio. H1 no implementa el control de gameplay; H3 decidirá qué acciones consume el avatar.
- **Motivo:** evita rehacer bindings y permite probar el slice en Android y editor con la misma capa de intención.
- **Consecuencias:** el asset de acciones es un contrato inicial; ningún binding por sí solo otorga autoridad sobre estado del juego.

## DEC-0019 — Cinemachine como cámara oficial

- **Fecha:** 2026-07-19
- **Estado:** aprobada para H1
- **Decisión:** Cinemachine 3 se incorpora desde H1 y las escenas usan `CinemachineBrain`/cámara Cinemachine como punto oficial de integración. No se crean cámaras paralelas con lógica ad hoc.
- **Motivo:** centraliza seguimiento, composición y futuras transiciones sin acoplar la presentación al dominio.
- **Consecuencias:** H2 definirá la composición isométrica y el objetivo de seguimiento; cualquier excepción requerirá una decisión registrada.

## DEC-0020 — Constantes y configuración separadas

- **Fecha:** 2026-07-19
- **Estado:** aprobada para H2
- **Decisión:** las constantes estructurales compartidas por dominio, tests y herramientas viven en `Assets/Game/Domain/Constants`. `Assets/Game/Config` queda reservado para configuración editable futura y no contiene estado de partida.
- **Motivo:** evita duplicar límites del mundo en scripts de Unity y mantiene una frontera clara entre contrato estructural y datos configurables.
- **Consecuencias:** una constante que afecte reglas del mundo debe permanecer libre de `UnityEngine`; un dato que deba balancearse o editarse en contenido podrá migrar posteriormente a Config.

## DEC-0021 — Greybox con geometría agregada

- **Fecha:** 2026-07-19
- **Estado:** aprobada para H2
- **Decisión:** representar plaza, sendero y cueva con tres meshes agregadas, colisiones 2D estáticas y un preview de navegación; no crear un GameObject por celda.
- **Motivo:** valida escala, lectura, obstáculos y ruta con un presupuesto de objetos pequeño y evita introducir deuda de rendimiento antes del arte.
- **Consecuencias:** H2 no demuestra todavía input o movimiento del jugador. H3 consumirá el mismo grid y añadirá control solo después de una nueva aprobación.

## DEC-0022 — Layers y Gizmos oficiales del greybox

- **Fecha:** 2026-07-19
- **Estado:** aprobada para H3
- **Problema:** el control del jugador necesita una convención estable para colisiones, depuración y UI sin mezclar responsabilidades.
- **Decisión:** reservar `WorldGround` 8, `WorldObstacle` 9, `Player` 10, `PlayerRespawn` 11, `NavigationDebug` 12 y `PlayerUI` 13. `GreyboxDebugGizmos` visualiza grid, obstáculos, ruta BFS y respawn exclusivamente como herramienta de Editor.
- **Motivo:** hace visibles las fronteras del mundo y permite validar el movimiento sin añadir sistemas de gameplay.
- **Consecuencias:** cualquier nueva colisión o herramienta debe respetar estas Layers; no se deben reutilizar para autoridad online.

## DEC-0023 — Movimiento por intención y joystick virtual

- **Fecha:** 2026-07-19
- **Estado:** aprobada para H3
- **Problema:** el teclado de QA y el joystick Android no deben crear dos rutas de movimiento distintas.
- **Decisión:** ambos dispositivos producen `MovementIntent` mediante el Input System. El joystick se implementa con `OnScreenStick` sobre un Canvas y publica `<Gamepad>/leftStick`; el controlador local aplica la intención con `Rigidbody2D`.
- **Motivo:** mantiene el dominio independiente del dispositivo y deja una entrada compatible con una futura sesión remota.
- **Consecuencias:** H3 no valida autoridad de red; la futura sesión autoritativa deberá validar la intención y enviar el estado resultante.

## DEC-0024 — Combat Prototype antes de ampliar habilidades

- **Fecha:** 2026-07-19
- **Estado:** aprobada para H4
- **Problema:** añadir múltiples habilidades, enemigos y recompensas antes de validar el golpe básico dificultaría distinguir problemas de control, lectura, balance y autoridad.
- **Decisión:** H4 se limita a un Mordeluz, un ataque básico, salud, daño, cooldown, IA `Idle → Detect → Follow → Attack → Return`, muerte y feedback visual/sonoro. Se usan `IHealth`, `IDamageable`, `ITargetable` e `IAttacker` como contratos reutilizables.
- **Motivo:** permite comprobar que el bucle de combate es legible y divertido antes de añadir complejidad.
- **Consecuencias:** habilidades adicionales, élite, loot, inventario, misión, progresión y economía quedan bloqueados hasta una nueva aprobación.

## DEC-0025 — El enemigo no bloquea físicamente la navegación

- **Fecha:** 2026-07-19
- **Estado:** aprobada para H4
- **Problema:** un collider de criatura sobre la ruta podía impedir que la regresión de movimiento plaza → cueva → plaza fuese reproducible.
- **Decisión:** `Mordeluz` mantiene collider propio para el mundo, pero ignora la colisión física con el jugador; el combate se valida por objetivo, rango, estado de IA y cooldown.
- **Motivo:** separa navegación de interacción de combate y evita que una criatura provisional introduzca un bloqueo artificial en el greybox.
- **Consecuencias:** el movimiento no prueba todavía empuje, bloqueo de criaturas ni crowd collision; esas reglas pertenecen a una fase posterior.

## DEC-0026 — H4B mantiene las reglas temporales fuera de MonoBehaviour

- **Fecha:** 2026-07-19
- **Estado:** aprobada para H4B
- **Problema:** Calor, duración, cooldown y reducción de daño son reglas sensibles a errores de reloj y difíciles de probar si viven en componentes de escena.
- **Decisión:** `HeatResourceModel`, `DefenseAbilityModel`, `AreaAttackAbilityModel` y `ResonantWaveAttackModel` viven en `Game.Domain/Combat` y reciben `ICombatTimeSource`. Unity solo inyecta `UnityCombatTimeSource` y presenta el resultado.
- **Motivo:** permite pruebas deterministas y deja el mismo contrato listo para una simulación autoritativa futura.
- **Consecuencias:** los valores de balance se mantienen como constantes del proyecto durante el slice; una configuración editable posterior deberá conservar la misma frontera de reglas.

## DEC-0027 — La variante Resonante reutiliza la base común

- **Fecha:** 2026-07-19
- **Estado:** aprobada para H4B
- **Problema:** una segunda IA duplicada habría creado divergencia en seguimiento, leash, colisiones y muerte.
- **Decisión:** `MordeluzResonanteController` hereda `MordeluzController` y reemplaza solo la ejecución del ataque por una onda con anticipación visual y zona de peligro.
- **Motivo:** valida la variante élite sin ampliar el número de sistemas ni introducir una arquitectura de enemigos paralela.
- **Consecuencias:** la futura ampliación de habilidades debe mantener esta separación entre máquina de estados reutilizable, modelo de habilidad y feedback.

## DEC-0028 — H5 progresa por eventos de dominio con entrega idempotente

- **Fecha:** 2026-07-20
- **Estado:** aprobada para H5
- **Problema:** conectar directamente la UI de combate con una misión habría mezclado presentación, identidad de enemigos y estado persistente.
- **Decisión:** cada enemigo publica un `CombatantDefeatedEvent` mediante `DomainEventBus`; `H5MissionModel` escucha esos eventos solo durante `Active`, filtra por tipo y deduplica por `EntityId`. La entrega comprueba estado e inventario antes de publicar la recompensa y pasar a `Completed`.
- **Motivo:** mantiene la misión testeable sin Unity y hace explícita la frontera que más adelante deberá validar un servidor autoritativo.
- **Consecuencias:** los nombres de entidad de la escena son IDs provisionales del slice; antes de persistencia online deberán sustituirse o respaldarse con IDs estables de entidad.

## DEC-0029 — Recompensa fija, inventario pequeño y una ranura de reliquia

- **Fecha:** 2026-07-20
- **Estado:** aprobada para H5
- **Problema:** un inventario genérico y múltiples ranuras aumentarían el coste de UI, balance y persistencia antes de validar el bucle de misión.
- **Decisión:** H5 usa seis espacios fijos, conserva la recompensa `Fragmento de Resonancia` dentro del inventario al equiparla y expone únicamente la ranura `Relic`.
- **Motivo:** permite demostrar recompensa visible y equipable con una superficie pequeña, verificable y sin drops aleatorios.
- **Consecuencias:** no existe todavía reemplazo de equipo, stacks, arrastre, tooltip, economía ni serialización; se decidirán junto con H6/H7.

## DEC-0030 — H6 usa un umbral exacto de XP para el vertical slice

- **Fecha:** 2026-07-20
- **Estado:** aprobada para H6
- **Problema:** una progresión abierta habría impedido validar el recorrido completo y habría introducido balance fuera del alcance.
- **Decisión:** el jugador empieza en nivel 1, el slice termina en nivel 2 y el umbral es 100 XP: cada Mordeluz da 10, el Resonante 30 y completar la misión 40.
- **Motivo:** las cuatro derrotas más la entrega de Nara suman exactamente 100 XP y producen una única subida de nivel verificable.
- **Consecuencias:** no hay nivel 3, XP adicional, atributos, árbol de habilidades ni economía; el sistema queda preparado para ampliar el modelo después de H6.

## DEC-0031 — Persistencia local por contrato y DTOs versionados

- **Fecha:** 2026-07-20
- **Estado:** aprobada para H6
- **Problema:** serializar componentes Unity o hacer que el dominio escriba archivos habría bloqueado la futura migración a autoridad online.
- **Decisión:** `ISaveRepository` vive en `Game.Application`, los datos se expresan como DTOs puros en `Game.Domain.Persistence` y `JsonFileSaveRepository` implementa la persistencia en `Game.Infrastructure.Local`.
- **Motivo:** permite probar modelo, serialización y archivos por separado, con `schemaVersion` explícito y un punto único de sustitución.
- **Consecuencias:** `Game.Client` compone el repositorio local durante el slice; el adaptador remoto futuro debe implementar el mismo contrato sin mover JSON al dominio.

## DEC-0032 — La persistencia guarda IDs ya aplicados y posición segura

- **Fecha:** 2026-07-20
- **Estado:** aprobada para H6
- **Problema:** guardar solo XP y contadores no impediría que enemigos recreados volvieran a conceder recompensas después de cargar.
- **Decisión:** el save conserva `ProcessedDefeatIds` de la misión y `AppliedSourceIds` de experiencia, además de la posición segura del jugador. La carga restaura esos conjuntos antes de aceptar nuevos eventos.
- **Motivo:** evita duplicación de progreso, XP y recompensa sin impedir que los enemigos reaparezcan físicamente al recargar la escena.
- **Consecuencias:** los IDs son estables dentro del vertical slice; la autoridad online futura deberá asignar IDs persistentes del servidor.

## DEC-0033 — Presentación H7 desacoplada y original

- **Fecha:** 2026-07-20
- **Estado:** aprobada durante H7
- **Problema:** el greybox validaba reglas y recorrido, pero no permitía juzgar silueta, ritmo de combate ni lectura del objetivo en Android.
- **Alternativas:** mantener Quads hasta H8; importar assets de las referencias; crear una capa de arte base original que no cambie gameplay.
- **Decisión:** H7 añade sprites originales, Animator, VFX pool, audio procedural, ambientes por zona, HUD e integración de cámara dentro de `Game.Client/Presentation`. El builder conserva colliders, grid, navegación, respawn y contratos de H3–H6.
- **Motivo:** permite validar presentación y feedback sin introducir deuda de autoridad ni copiar contenido de terceros. Los assets se generan con fondo cromático temporal, se limpian a transparencia y se guardan junto a sus fuentes para trazabilidad.
- **Consecuencias:** H7 no es arte final de producción ni perfilado Android. H8 optimiza la importación de texturas y la presentación del slice; la necesidad de atlas y la validación sobre hardware real quedan sujetas a las mediciones de H9. La presentación observa resultados y eventos, pero nunca concede daño, XP, recompensa o equipamiento.

## DEC-0034 — La configuración H8 es local y solo de presentación

- **Fecha:** 2026-07-20
- **Estado:** aprobada durante H8
- **Problema:** pausa, volumen, calidad y diagnósticos necesitan persistir entre ejecuciones, pero no deben contaminar el save de partida ni convertirse en reglas del dominio.
- **Decisión:** `H8LocalSettings` usa claves propias de `PlayerPrefs` para música, FX, vibración, calidad, FPS y debug. `H8PauseController` compone esa configuración en `Game.Client/Presentation` y `H6ProgressionRuntime` conserva su `ISaveRepository` sin cambios.
- **Motivo:** mantiene separadas las preferencias del dispositivo y el estado persistente de personaje; permite sustituir PlayerPrefs por otro almacenamiento local sin mover reglas de gameplay.
- **Consecuencias:** la configuración no se sincroniza, no tiene autoridad online y debe volver a validarse si en el futuro se soportan perfiles o cuentas.

## DEC-0035 — H8 valida el paquete Android y deja el hardware para H9

- **Fecha:** 2026-07-20
- **Estado:** aprobada durante H8
- **Problema:** el módulo Android puede estar instalado aunque no exista un teléfono conectado para medir FPS, temperatura y batería.
- **Decisión:** H8 incorpora `H8OptimizationBuilder.BuildAndroidDevelopment` para generar un APK reproducible con profiler conectado. La aceptación de métricas reales de dispositivo, estabilidad térmica y consumo pertenece a H9.
- **Motivo:** separa un fallo de empaquetado de una medición física que requiere hardware disponible y evita declarar rendimiento no observado.
- **Consecuencias:** el APK H8 confirma compilación/package; no confirma arranque, input, memoria térmica, temperatura, batería ni FPS sostenidos en Android hasta ejecutar H9.

## DEC-0036 — El arranque físico Android es prerrequisito de H9

- **Fecha:** 2026-07-20
- **Estado:** aprobada durante H8.1
- **Problema:** un APK puede compilar correctamente y aun así permanecer en una escena vacía después del splash; los tests que cargan `VerticalSlice` directamente no cubren la ruta real de arranque.
- **Decisión:** H8.1 debe verificar la transición real `Bootstrap` índice 0 → `VerticalSlice` y completar cinco arranques en un Android físico antes de cerrar la puerta Android de H9. La validación Editor/APK no sustituye la prueba de hardware.
- **Motivo:** separa una regresión de arranque de los problemas de rendimiento y temperatura que H9 debe medir, y evita declarar el vertical slice jugable sin evidencia del dispositivo objetivo.
- **Consecuencias:** el pulido de presentación H9 puede implementarse sin hardware, pero su aceptación Android queda bloqueada mientras ADB no detecte un dispositivo y no existan logs de instalación limpia, segundo arranque, save, minimizar/restaurar y force close. No se amplía gameplay para resolver este bloqueo.

## DEC-0037 — H9 es pulido de presentación, no expansión de sistemas

- **Fecha:** 2026-07-21
- **Estado:** aprobada para H9
- **Problema:** el vertical slice ya es jugable, pero la cámara, la densidad del HUD y la ergonomía móvil todavía podían hacer que la demo pareciera un prototipo.
- **Decisión:** H9 modifica únicamente composición Cinemachine, límites de cámara, safe area, layout de controles, HUD, menús y feedback de selección. `H9VerticalSlicePolishBuilder` conserva el scene graph y los contratos de gameplay H3–H8.
- **Motivo:** maximiza sensación, claridad y estabilidad Android sin introducir deuda de backend ni nuevos balances antes de la revisión técnica.
- **Consecuencias:** la validación física, capturas y métricas Android siguen siendo una puerta explícita; no se declara cerrado el rendimiento de hardware sin un dispositivo real.
