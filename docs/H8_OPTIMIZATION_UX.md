# Lumbre de Nácar — H8 optimización, UX y preparación del vertical slice

> Estado: completado · v0.8.0 Alpha · 2026-07-20

## Objetivo

H8 mejora la estabilidad y la lectura del vertical slice sin añadir reglas de gameplay. La arquitectura, el dominio, la aplicación, el save H6 y los contratos H3–H7 permanecen sin cambios.

## Entregables

### Optimización de presentación

- Texturas finales sin mipmaps ni lectura CPU, con compresión y límite de tamaño móvil.
- Canvas con resolución de referencia 1920×1080, sin canales adicionales y sin pixel-perfect innecesario.
- Animator con `CullUpdateTransforms` y root motion desactivado.
- VSync desactivado y frame rate objetivo de 60 para el slice.
- HUD H7 con refresco textual limitado a 10 Hz; las animaciones de pulso siguen siendo independientes.
- Pool de VFX H7 conservado y reutilizado; no se introducen `Instantiate/Destroy` por evento.
- Audio H7 conserva fuentes y ambientes locales; H8 expone volúmenes configurables sin mover audio al dominio.

La escena usa pocos sprites, un backdrop y una UI pequeña. Por eso H8 no agrega un paquete de Sprite Atlas ni Addressables sin una medición que justifique su coste; la decisión queda abierta para H9 si el perfilado detecta draw calls o memoria problemáticos.

### Pausa y opciones

`H8PauseController` ofrece:

- pausar y continuar;
- abrir y cerrar opciones;
- volumen de música y efectos;
- vibración preparada para el futuro feedback háptico;
- mostrar FPS y debug para QA;
- ciclo de calidad gráfica;
- salida y versión visible `v0.8.0 Alpha`.

`H8LocalSettings` persiste estas preferencias con `PlayerPrefs`. Se trata de configuración de presentación local, independiente de `ISaveRepository` y del estado de partida.

### Diagnóstico y ayuda contextual

`H8PerformanceOverlay` muestra, cuando QA lo solicita, FPS, frame time, memoria de sistema, memoria GC, draw calls, CPU frame time, GPU frame time y calidad activa. Los contadores CPU/GPU son opcionales por plataforma: cuando Unity no expone uno, el valor se presenta como `--`. `H8Tooltip` usa un único panel compartido para orientar controles de volumen, calidad, vibración y diagnóstico sin crear ventanas duplicadas.

## Validación reproducible

- `H8OptimizationBuilder.Build` ejecutado dos veces en batchmode, sin cambiar el resultado de la escena.
- EditMode: 23/23 tests pasados.
- PlayMode: 7/7 tests pasados, incluyendo regresiones H3, H4, H4B, H5, H6 y H7.
- APK Android de desarrollo generado en `/tmp/lumbre-h8-android.apk` con el módulo Android de Unity 6000.3.20f1.
- `adb devices` no mostró dispositivos conectados. Por tanto, el arranque en hardware, FPS sostenidos, temperatura, batería, memoria térmica y tiempos de carga siguen pendientes de H9.

## Fuera de alcance

H8 no añade NPC, enemigos, misiones, habilidades, XP, niveles, loot, inventario, equipamiento, economía, VIP, tienda, crafting, networking, login, cuentas, servidor, multiplayer ni arte final de producción.

## Criterio de salida

H8 queda cerrado cuando el builder y las pruebas están verdes, las opciones funcionan y persisten, la pausa conserva el slice, el HUD se mantiene legible, no hay errores críticos del proyecto y existe un build Android reproducible. Las métricas de dispositivo físico son una puerta separada de H9.
