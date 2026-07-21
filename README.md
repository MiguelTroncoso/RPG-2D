# Lumbre de Nácar

Documento de orientación inicial para un MMORPG 2D isométrico, original y orientado primero a Android.

> Estado: v0.8.0 Alpha · H8 completado — optimización, UX, configuración local y preparación del vertical slice validadas; H0–H7 estables. El perfilado sobre dispositivo Android queda en H9.

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
- [Changelog](docs/CHANGELOG.md): bitácora reproducible de H1–H8.
- [H2 greybox](docs/H2_GREYBOX.md): zonas, grid, colisiones, navegación y presupuesto estructural.
- [H3 player control](docs/H3_PLAYER_CONTROL.md): Layers, Gizmos, input, movimiento, cámara, colisiones y respawn.
- [H4 combat prototype](docs/H4_COMBAT_PROTOTYPE.md): Mordeluz, salud, daño, cooldown, IA, muerte y feedback.
- [H4B abilities and elite](docs/H4B_ABILITIES_ELITE.md): Calor, defensa, área, telegraph y Mordeluz Resonante.
- [H5 mission and reward](docs/H5_MISSION_REWARD.md): Nara, eventos de progreso, recompensa fija, inventario y equipamiento.
- [H6 progression and persistence](docs/H6_PROGRESSION_PERSISTENCE.md): XP, nivel 1→2, JSON versionado y restauración local.
- [H7 art, audio and presentation](docs/H7_ART_AUDIO_POLISH.md): arte base original, animaciones, VFX, audio, HUD y cámara.
- [H8 optimization and UX](docs/H8_OPTIMIZATION_UX.md): rendimiento de presentación, pausa, opciones, configuración local, tooltips y overlay de QA.

## Qué no se implementa todavía

H8 incorpora optimización de presentación, HUD refinado, pausa, opciones locales y herramientas de QA, pero todavía no implementa crafting, tienda, economía, monedas, profesiones, atributos avanzados, más niveles, varias partidas, nube, cuentas, login, backend online, networking, servidor, multiplayer ni arte final de producción. El slice actual permite recorrer el mundo, completar la misión de Nara, alcanzar nivel 2, equipar la recompensa, recargar el progreso local y configurar la experiencia sin cambiar las reglas de H3–H7.

## Próximo paso propuesto

El siguiente hito propuesto es H9 — perfilado en un Android de gama media con métricas reales de FPS, frame time, memoria, temperatura, batería y carga. No se añadirá tienda, economía, VIP ni multiplayer antes de esa puerta.
