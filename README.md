# Lumbre de Nácar

Documento de orientación inicial para un MMORPG 2D isométrico, original y orientado primero a Android.

> Estado: v0.8.1 Alpha · H9 — pulido del vertical slice para Android; validación física de Android pendiente. H0–H8.1 estables.

## Resumen

**Lumbre de Nácar** es un MMORPG de fantasía luminosa con perspectiva isométrica, exploración cooperativa y progresión legible en sesiones cortas o largas. El mundo de **Astreva** está atravesado por rutas de resonancia que conectan ciudades, minas, bosques, ruinas y costas. Los jugadores exploran, combaten, recolectan, fabrican, comercian y participan en eventos públicos que modifican temporalmente las condiciones de una región.

La propuesta no intenta ganar por tamaño desde el primer día. El objetivo inicial es validar un pequeño fragmento del juego: un asentamiento, una zona exterior, una mazmorra corta, combate contra criaturas, una misión y una economía mínima. Todo el contenido será original; la captura adjunta y `https://rubinot.com.br/news` se usan solo como referencias de convenciones generales del género.

## Decisiones iniciales para aprobación

- **Modalidad del mundo:** híbrida: regiones persistentes conectadas por rutas y transiciones breves.
- **Orientación:** horizontal, con UI adaptable a teléfonos y tablets.
- **Combate:** PvE como eje; PvP opt-in en arena y zonas señalizadas.
- **Progresión:** nivel máximo inicial 30, equipo con pocas capas de complejidad y poder de jugador controlado.
- **Monetización:** cosméticos y servicios de conveniencia no competitivos; sin venta directa de poder.
- **Primer hito jugable:** prototipo local offline en Unity con recursos provisionales propios.
- **Editor:** Unity 6.3 LTS, usando la LTS estable más reciente disponible en Unity Hub al iniciar H1. La revisión resuelta localmente es `6000.3.20f1`; se documenta para reproducibilidad, no como compromiso de permanecer en una revisión antigua.
- **Input:** Input System oficial preparado para Android/touch, teclado, mouse y gamepad; el vertical slice usará primero joystick virtual.
- **Cámara:** Cinemachine 3 como sistema oficial desde H1.
- **Constantes:** contrato estructural centralizado en `Assets/Game/Domain/Constants`.
- **Configuración futura:** `Assets/Game/Config` queda reservado para definiciones editables posteriores.

## Documentación

- [GDD draft](docs/GDD_DRAFT.md): visión, sistemas, mundo, personajes, criaturas, NPC, economía y MVP.
- [Roadmap](docs/ROADMAP.md): fases, dependencias, complejidad y criterios de salida.
- [Decisions](docs/DECISIONS.md): registro de decisiones de preproducción.
- [Art guide draft](docs/ART_GUIDE_DRAFT.md): dirección visual, legibilidad, UI y política de assets.
- [Critical review](docs/CRITICAL_REVIEW.md): reducción del alcance, riesgos, arquitectura de migración y criterios de la primera demo.
- [TDD](docs/TDD.md): diseño técnico de H6 y límites de implementación.
- [Architecture](docs/ARCHITECTURE.md): módulos, dependencias y frontera offline/online.
- [Android build](docs/ANDROID_BUILD.md): configuración horizontal y procedimiento de build.
- [Testing](docs/TESTING.md): estrategia y comandos de verificación.
- [Changelog](docs/CHANGELOG.md): bitácora reproducible de H1–H9.
- [H2 greybox](docs/H2_GREYBOX.md): zonas, grid, colisiones, navegación y presupuesto estructural.
- [H3 player control](docs/H3_PLAYER_CONTROL.md): Layers, Gizmos, input, movimiento, cámara, colisiones y respawn.
- [H4 combat prototype](docs/H4_COMBAT_PROTOTYPE.md): Mordeluz, salud, daño, cooldown, IA, muerte y feedback.
- [H4B abilities and elite](docs/H4B_ABILITIES_ELITE.md): Calor, defensa, área, telegraph y Mordeluz Resonante.
- [H5 mission and reward](docs/H5_MISSION_REWARD.md): Nara, eventos de progreso, recompensa fija, inventario y equipamiento.
- [H6 progression and persistence](docs/H6_PROGRESSION_PERSISTENCE.md): XP, nivel 1→2, JSON versionado y restauración local.
- [H7 art, audio and presentation](docs/H7_ART_AUDIO_POLISH.md): arte base original, animaciones, VFX, audio, HUD y cámara.
- [H8 optimization and UX](docs/H8_OPTIMIZATION_UX.md): rendimiento de presentación, pausa, opciones, configuración local, tooltips y overlay de QA.
- [H8.1 Android black screen](docs/H8_1_ANDROID_BLACK_SCREEN.md): diagnóstico, corrección de Bootstrap y checklist físico pendiente.
- [H9 vertical slice polish](docs/H9_VERTICAL_SLICE_POLISH.md): cámara, safe area, controles, HUD, menús y validación Android.

## Qué no se implementa todavía

H9 conserva la optimización de presentación y pule exclusivamente la experiencia del vertical slice: cámara, safe area, controles, HUD, menús, feedback y preparación Android. Todavía no implementa crafting, tienda, economía, monedas, profesiones, atributos avanzados, más niveles, varias partidas, nube, cuentas, login, backend online, networking, servidor, multiplayer ni arte final de producción. El slice actual permite recorrer el mundo, completar la misión de Nara, alcanzar nivel 2, equipar la recompensa, recargar el progreso local y configurar la experiencia sin cambiar las reglas de H3–H8.

## Próximo paso propuesto

La siguiente acción es la revisión técnica del repositorio y la validación física del APK H9 en un Android de referencia. El objetivo de versión posterior es v0.9.0 Alpha; no se inicia H10 ni se añaden sistemas nuevos antes de esa revisión.
