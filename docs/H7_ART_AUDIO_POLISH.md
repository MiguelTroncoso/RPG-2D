# Lumbre de Nácar — H7: arte base, audio y pulido

> Estado: completado · v0.7.0 Alpha · Fecha: 2026-07-20

## Objetivo

Dar identidad visual y sonora al vertical slice sin cambiar las reglas de H3, H4, H4B, H5 o H6. H7 es una capa de presentación: el dominio, la intención de movimiento, el combate, la misión, la progresión y el guardado siguen siendo los mismos.

## Entregables

- Assets originales con transparencia para Bastión de Brasa, Nara Velaquieta, Mordeluz, Mordeluz Resonante y un backdrop de plaza, sendero y cueva.
- Animaciones Animator generadas por builder para estados de movimiento, combate, daño, muerte, habilidades y telegraph.
- VFX runtime con pool prewarmado: 18 elementos iniciales y máximo de 24 activos.
- Feedback de audio procedural: pasos, ataque, impacto, muerte, misión, equipamiento, subida de nivel, UI y tres capas ambientales con crossfade por zona.
- HUD horizontal de vida, Calor, XP, misión, inventario y equipamiento.
- Indicador de misión y orientación de Nara hacia el jugador.
- Pulido de Cinemachine: shake corto en acciones críticas y zoom breve en subida de nivel.

## Arquitectura

La presentación vive en `Game.Client/Presentation` y se compone en la escena por `H7PresentationRuntime`. Las reglas siguen fuera de `MonoBehaviour`; la presentación escucha resultados y eventos existentes. El builder conserva colliders, grid, navegación, respawn y controladores de H3–H6, y solo reemplaza los visuales de greybox y añade presentación.

`H7PresentationBuilder` es idempotente: puede ejecutarse varias veces, importa/configura texturas, recrea un visual inválido de una pasada anterior, genera clips/controladores y guarda `VerticalSlice` sin duplicar componentes funcionales.

## Rendimiento Android

- Texturas importadas sin mipmaps, con compresión de alta calidad compatible y límite de 2048 px.
- VFX limitado mediante pooling; no se hace `Instantiate/Destroy` por golpe.
- Un solo backdrop por escena y tres fuentes ambientales reutilizadas.
- La UI sigue siendo Canvas horizontal; no se añaden postprocesado ni iluminación dinámica.
- El presupuesto de draw calls, FPS, memoria, temperatura y batería debe medirse en H9 sobre dispositivo real. H7 no declara todavía un resultado de perfilado Android.

## Criterios de aceptación

- El avatar, Nara, Mordeluz y el Resonante muestran sprite y Animator configurados.
- Los estados críticos producen señal visual y/o sonora sin alterar el resultado de dominio.
- La misión, el inventario, la reliquia, XP y nivel aparecen en HUD.
- El pool, audio por zonas, HUD y cámara se mantienen configurados tras recargar `VerticalSlice`.
- Builder, EditMode y PlayMode terminan sin fallos del proyecto.

## Fuera de H7

No se implementan nuevas reglas de juego, enemigos, mapas, NPC, misiones, inventario, loot, economía, VIP, crafting, tienda, networking, backend, autenticación, chat, multiplayer ni arte final de producción.
