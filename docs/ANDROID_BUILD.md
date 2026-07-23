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
- H9 añade una raíz de safe area para `PlayerUI`, reespacia joystick/botones, compacta HUD/menús y configura la cámara Cinemachine con límites de presentación.
- H9 conserva landscape como orientación de la demo; no añade una ruta portrait ni cambia la simulación de gameplay.
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

## Build de diagnóstico H8.1 — pantalla negra

H8.1 configura temporalmente un APK Development ARM64 con Script Debugging, Autoconnect Profiler, símbolos y OpenGLES3 como primera API gráfica. El builder restaura la configuración de proyecto después del build:

```bash
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics -quit \
  -projectPath "/Users/migueltroncoso/Documents/Juego 2D" \
  -executeMethod Lumbre.Game.Editor.H8OptimizationBuilder.BuildAndroidBlackScreenDebug \
  -logFile /tmp/lumbre-h8-1-android-build.log
```

Salida verificada: `/tmp/lumbre-h8-1-black-screen-debug.apk` (82 MB, build Android exitoso). El APK no se versiona por `.gitignore`.

Con un dispositivo conectado, usar el ADB incluido con Unity:

```bash
ADB="/Applications/Unity/Hub/Editor/6000.3.20f1/PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb"
"$ADB" devices -l
"$ADB" install -r /tmp/lumbre-h8-1-black-screen-debug.apk
"$ADB" logcat -c
"$ADB" shell am force-stop com.LumbredeNacarStudio.LumbredeNcar
"$ADB" shell monkey -p com.LumbredeNacarStudio.LumbredeNcar 1
"$ADB" logcat -v time -s Unity ActivityManager AndroidRuntime libc DEBUG
```

El package id del APK debug es `com.LumbredeNacarStudio.LumbredeNcar`; debe confirmarse si se modifica la configuración Android antes de instalar. Guardar el logcat limpio y filtrado fuera del repositorio si contiene datos del dispositivo. H8.1 exige cinco arranques físicos y no se considera cerrado mientras `adb devices -l` esté vacío.

## Build Android H9

El builder H9 prepara la escena existente sin reconstruir gameplay y genera un APK ARM64 de desarrollo:

```bash
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics -quit \
  -projectPath "/Users/migueltroncoso/Documents/Juego 2D" \
  -executeMethod Lumbre.Game.Editor.H9VerticalSlicePolishBuilder.BuildAndroidDevelopment \
  -logFile /tmp/lumbre-h9-android-build.log
```

Salida esperada: `/tmp/lumbre-h9-android.apk`. El APK no se versiona por `.gitignore`. La ejecución física debe registrar capturas del arranque, recorrido, HUD/controles, pausa y safe area en el dispositivo de referencia.

## Validación pendiente de hardware H9

El entorno local no tiene un Android conectado: `adb devices -l` solo muestra el encabezado. Por tanto, no se afirma todavía un resultado físico ni se agregan capturas simuladas como si fueran de dispositivo. H9 debe registrar FPS sostenidos, frame time, memoria, draw calls, temperatura, batería y tiempos de carga en al menos un Android de gama media durante 15 minutos. Si aparecen hitches por transparencia, audio, Canvas o partículas, se corrige la presentación antes de ampliar contenido.

## H9.1 — Runner, APK y estado de validación

Para que Unity termine las suites y genere el XML, no se debe pasar `-quit` junto con `-runTests`; el Test Framework cierra el proceso cuando termina:

```bash
UNITY="/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity"
PROJECT="/Users/migueltroncoso/Documents/Juego 2D"

"$UNITY" -batchmode -projectPath "$PROJECT" \
  -runTests -testPlatform editmode \
  -testResults /tmp/lumbre-h9-editmode.xml \
  -logFile /tmp/lumbre-h9-editmode.log

"$UNITY" -batchmode -projectPath "$PROJECT" \
  -runTests -testPlatform playmode \
  -testResults /tmp/lumbre-h9-playmode.xml \
  -logFile /tmp/lumbre-h9-playmode.log
```

El resultado verificado en esta estación fue EditMode 23/23 y PlayMode 9/9. El APK H9 se generó en `/tmp/lumbre-h9-android.apk` con ARM64, OpenGLES3, Development Build, Allow Debugging y ConnectWithProfiler. El ADB incluido en Unity está en `PlaybackEngines/AndroidPlayer/SDK/platform-tools/adb`; actualmente `adb devices -l` devuelve una lista vacía, así que H9.1 permanece bloqueado para instalación, arranques, capturas y métricas físicas.

## H9.2 — Corrección de layout y APK de validación

H9.2 mantiene landscape, ARM64, OpenGLES3, Development Build, Allow Debugging y ConnectWithProfiler. El builder conserva la configuración de Android y deja la salida intermedia en `/tmp/lumbre-h9-android.apk`; para revisión del hito se copia al destino ignorado del proyecto:

```text
Builds/Android/LumbreDeNacar-v0.8.1-H9.2.apk
```

El layout se valida en 1920×1080, 2400×1080, 2340×1080, 2560×1080 y 1280×720. El joystick mantiene un área táctil amplia mientras su representación visual ocupa menos espacio; los controles superiores se anclan a la safe area y el HUD se resuelve después del arranque de la escena.

El APK y cualquier carpeta `Builds/` quedan excluidos por `.gitignore`; no se suben al repositorio. Un APK generado correctamente no sustituye la prueba física: instalación, arranques, capturas y métricas Android siguen pendientes de un dispositivo conectado.

## H10 — locomoción y combate táctil

H10 mantiene la ejecución horizontal y prepara un APK Development ARM64 con OpenGLES3, Allow Debugging y ConnectWithProfiler. `H10PlayerControlBuilder.BuildAndroidDevelopment` ejecuta primero el builder idempotente de H10 y restaura las APIs gráficas y arquitecturas originales al terminar:

```bash
UNITY="/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity"
PROJECT="/Users/migueltroncoso/Documents/Juego 2D"

"$UNITY" -batchmode -nographics -quit -projectPath "$PROJECT" \
  -executeMethod Lumbre.Game.Editor.H10PlayerControlBuilder.BuildAndroidDevelopment \
  -logFile /tmp/h10-android-build.log
```

Salida verificada en esta estación:

```text
Builds/Android/LumbreDeNacar-v0.8.1-H10.apk
76,139,085 bytes
SHA-256: a78cf8d9aaefc9703032860023cc690cea19ddeceb641640125a3e236ff3004f
```

La escena incluye Bootstrap y VerticalSlice habilitadas, el Input System unificado y Cinemachine oficial. El APK queda ignorado por `.gitignore` y no se publica en GitHub. Todavía falta instalarlo en un Android físico para validar joystick, ATK, DEF, AOE, safe area, cámara, pausa, interacción, equipamiento, arranques y métricas; un build correcto no se presenta como validación de hardware.

## H10.2 — game feel y feedback de combate

El builder H10.2 configura y valida la secuencia visual sin persistir estado runtime. La generación reproducible se ejecutó con:

```bash
"$UNITY" -batchmode -nographics -quit -projectPath "$PROJECT" \
  -executeMethod Lumbre.Game.Editor.H10_2GameFeelBuilder.BuildAndroidDevelopment \
  -logFile /tmp/lumbre-h10-2-android-build.log
```

Configuración y resultado:

```text
Builds/Android/LumbreDeNacar-v0.8.1-H10.2.apk
76,184,173 bytes
SHA-256: 3e2e9fa13d8b2100e3d6250ad5db54f4dc2ebeb3da5313fe35bf3c42b9c2393b
ARM64 · OpenGLES3 · Development · AllowDebugging · ConnectWithProfiler · CleanBuildCache
```

El APK se instaló manualmente mediante ADB en Xiaomi 24090RA29G, Android 16/API 36, resolución renderizada landscape 2712×1220. La variante anterior con tiempos de secuencia serializados reprodujo `CachedReader::OutOfBoundsError`; la salida indicada arriba reconstruye esos valores en runtime y arrancó correctamente. Bootstrap, VerticalSlice, cámara, jugador y HUD se verificaron después de la carga. La evidencia visual está en `docs/captures/android-h10.2-physical/`. Android 16 bloqueó la inyección de eventos `adb shell input`, y el Profiler no pudo conectar por UDP; la validación táctil interactiva y el perfilado siguen pendientes. El APK y `Builds/` permanecen ignorados.
