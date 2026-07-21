# Lumbre de Nácar — Android y build del vertical slice

## Requisitos locales

- Unity 6.3 LTS instalado desde Unity Hub.
- Android Build Support, Android SDK & NDK Tools, OpenJDK y CMake instalados desde el mismo editor.
- Un dispositivo Android con depuración USB para la validación posterior.
- No guardar keystores, contraseñas ni certificados dentro del repositorio.

## Configuración aplicada en H3

- Orientación por defecto: `LandscapeLeft`.
- Rotación vertical desactivada.
- Input System activo (`activeInputHandler: 1`).
- Resolución de referencia de desarrollo: 1920×1080; la UI final deberá probar relaciones de aspecto menores.
- Joystick virtual basado en `OnScreenStick`, con `EventSystem` e `InputSystemUIInputModule`.
- Botón de ataque `ATK` basado en `OnScreenButton`, enlazado a `<Gamepad>/buttonSouth`.
- Botón `HABLAR` de H5 enlazado a `<Gamepad>/buttonNorth`; teclado de QA `F`.
- Botón `EQUIP` de H5 enlazado a `<Gamepad>/leftShoulder`; teclado de QA `G`.
- La UI móvil usa la Layer `PlayerUI`; el avatar usa la misma acción `Move` que el teclado y el gamepad.
- H7 importa sprites base sin mipmaps, limita las texturas a 2048 px y usa compresión de alta calidad compatible; el arte final y los atlas se decidirán después de medir.
- H7 mantiene un pool de 18 VFX prewarmados y 24 activos como máximo, tres fuentes ambientales y un solo backdrop de mundo.
- H7 no incorpora postprocesado ni iluminación dinámica; la cámara Cinemachine solo añade shake y zoom temporales.
- H8 mantiene el Canvas con canales adicionales desactivados, texturas sin mipmaps/readable, Animator con culling, VSync desactivado y objetivo de 60 FPS.
- H8 ofrece pausa, opciones y configuración local de música, FX, vibración, calidad, FPS y debug. Estas preferencias no modifican el save de partida H6.
- H8 incorpora un overlay de QA con FPS, frame time, memoria, GC y draw calls; permanece oculto salvo que se active desde opciones.
- Backend, firma, iconos y permisos de producción quedan fuera de H1.

## Abrir y ejecutar

1. Abrir `/Users/migueltroncoso/Documents/Juego 2D` con Unity 6000.3.20f1.
2. Abrir `Assets/Game/Scenes/Bootstrap.unity`.
3. Ejecutar Play para validar que el proyecto carga la escena, la cámara oficial y el joystick sin errores.
4. Repetir con `Assets/Game/Scenes/VerticalSlice.unity`.

## Build de prueba

En Unity 6 usar **File → Build Profiles**, seleccionar Android, añadir las dos escenas desde Build Settings/Scene List y crear un APK de desarrollo para un dispositivo local. H1 solo exige documentar y dejar lista esta ruta; no se distribuye un APK ni se configura firma de tienda.

Antes de aceptar un build Android se debe comprobar:

- la aplicación arranca en landscape;
- no aparecen errores de paquete o shader;
- las dos escenas de H1 cargan;
- el joystick desplaza el avatar desde la plaza hasta la cueva y de regreso;
- el botón `ATK` permite derrotar a Mordeluz respetando el cooldown;
- Nara permite aceptar y entregar la misión dentro del rango de proximidad;
- la recompensa fija aparece en seis espacios y puede equiparse en la ranura de reliquia;
- el HUD de vida, Calor, XP, misión, inventario y reliquia conserva legibilidad en el teléfono de referencia;
- los sprites, animaciones, VFX y audio de H7 no producen errores de shader ni spikes visibles durante el recorrido;
- no se atraviesan los obstáculos ni los límites del greybox;
- la salida no incluye credenciales ni material de firma;
- el dispositivo y su versión Android quedan registrados en la prueba.

La automatización de builds reproducibles y la firma AAB pertenecen a H10, después de validar el vertical slice.

## Build Android H8

El módulo Android está instalado en Unity 6000.3.20f1. H8 expone un builder reproducible para comprobar compilación y empaquetado:

```bash
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics -quit \
  -projectPath "/Users/migueltroncoso/Documents/Juego 2D" \
  -executeMethod Lumbre.Game.Editor.H8OptimizationBuilder.BuildAndroidDevelopment \
  -logFile /tmp/lumbre-h8-android-build.log
```

La ejecución H8 generó `/tmp/lumbre-h8-android.apk` sin errores de compilación del proyecto. No había un teléfono conectado: `adb devices` solo mostró el encabezado de la lista. El APK confirma el paquete y la compilación; no confirma todavía el arranque ni las métricas en hardware.

## Perfilado pendiente de H9

H8 deja preparado el overlay y el build de desarrollo, pero no sustituye una medición en hardware. H9 debe registrar FPS sostenidos, frame time, memoria, draw calls, temperatura, batería y tiempos de carga en al menos un Android de gama media durante 15 minutos. Si aparecen hitches por transparencia, audio, Canvas o partículas, se corregirá la presentación antes de ampliar contenido.
