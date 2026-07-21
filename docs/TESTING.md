# Lumbre de Nácar — Estrategia de pruebas

## Compilación

La prueba mínima de H1 es de compilación y estructura:

```bash
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics -quit \
  -projectPath "/Users/migueltroncoso/Documents/Juego 2D" \
  -logFile -
```

El resultado debe terminar correctamente, importar los paquetes oficiales y no registrar errores de compilación. Los warnings de terceros o del editor se anotan y no se confunden con fallos del proyecto.

## EditMode

`ArchitectureSmokeTests` comprueba que:

- `Game.Domain` no referencia `UnityEngine`;
- `LocalGameSession` cambia a `Running` y publica un evento;
- la frontera de sesión existe sin gameplay.

Se ejecuta desde **Window → General → Test Runner → EditMode**. En CI se añadirá un comando de test dedicado cuando el proyecto tenga un agente Unity disponible.

## PlayMode H3

`H3PlayerMovementPlayModeTests` carga `VerticalSlice`, crea un gamepad virtual y publica valores en el stick izquierdo. Comprueba el recorrido observable plaza → cueva → plaza, la ausencia de bloqueo por colisiones, el retorno al punto de respawn y la Layer oficial del jugador.

Comando reproducible:

```bash
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -projectPath "/Users/migueltroncoso/Documents/Juego 2D" \
  -runTests -testPlatform playmode \
  -testResults /tmp/lumbre-h3-playmode.xml \
  -testFilter H3PlayerMovementPlayModeTests \
  -logFile /tmp/lumbre-h3-playmode.log
```

## Resultado H3

- EditMode: 7 tests pasados, 0 fallidos.
- PlayMode: 1 test de recorrido pasado, 0 fallidos.
- Builder de escena H3: terminó en batchmode con código 0.
- El warning de referencia HDRP opcional de una muestra de Cinemachine no corresponde al proyecto y no bloquea la compilación.

## H4 Combat Prototype

`CombatPrototypeTests` comprueba los modelos puros de salud, daño, ataque, cooldown y transiciones de Mordeluz. `H4CombatPrototypePlayModeTests` carga la escena, acerca el jugador al enemigo, comprueba la transición a `Attack`, verifica daño recibido y derrota a Mordeluz con tres ataques básicos mediante `buttonSouth`.

Comandos reproducibles:

```bash
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -projectPath "/Users/migueltroncoso/Documents/Juego 2D" \
  -runTests -testPlatform editmode \
  -testResults /tmp/lumbre-h4-editmode.xml \
  -testFilter Lumbre.Game.Tests \
  -logFile /tmp/lumbre-h4-editmode.log
```

```bash
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -projectPath "/Users/migueltroncoso/Documents/Juego 2D" \
  -runTests -testPlatform playmode \
  -testResults /tmp/lumbre-h4-playmode.xml \
  -testFilter Lumbre.Game.Tests \
  -logFile /tmp/lumbre-h4-playmode.log
```

## Resultado H4

- EditMode: 10 tests pasados, 0 fallidos.
- PlayMode: 2 tests pasados, 0 fallidos: regresión de movimiento H3 y combate H4.
- Builder H4: terminó en batchmode con código 0.
- El feedback sonoro usa `AudioSource` y tonos procedurales locales; no requiere assets de audio externos.

## H4B habilidades y élite

`H4BAbilityTests` comprueba Calor, reducción temporal, duración, cooldown, coste de área y ventana de resolución de la onda usando un `ICombatTimeSource` manual. `H4BAbilitiesElitePlayModeTests` carga la escena, valida defensa, agrupa tres Mordeluz para el área, los derrota y comprueba el telegraph, daño y muerte del Resonante.

Comandos reproducibles:

```bash
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -projectPath "/Users/migueltroncoso/Documents/Juego 2D" \
  -runTests -testPlatform editmode \
  -testResults /tmp/lumbre-h4b-editmode-final.xml \
  -testFilter Lumbre.Game.Tests \
  -logFile /tmp/lumbre-h4b-editmode-final.log
```

```bash
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -projectPath "/Users/migueltroncoso/Documents/Juego 2D" \
  -runTests -testPlatform playmode \
  -testResults /tmp/lumbre-h4b-playmode-final.xml \
  -testFilter Lumbre.Game.Tests \
  -logFile /tmp/lumbre-h4b-playmode-final.log
```

## Resultado H4B

- Builder H4B: terminó en batchmode con código 0.
- EditMode: 14 tests pasados, 0 fallidos.
- PlayMode: 3 tests pasados, 0 fallidos: recorrido H3, combate H4 y demo H4B.
- No se requieren assets de audio o arte finales; el feedback es provisional y procedural.

## H5 misión y recompensa

`H5MissionInventoryTests` comprueba los estados de misión, progreso por `CombatantDefeatedEvent`, deduplicación por `EntityId`, capacidad fija de seis espacios, recompensa única y equipamiento en la ranura `Relic`. `H5MissionRewardPlayModeTests` carga la escena, acepta la misión cerca de Nara, derrota tres Mordeluz con área y ataque básico, derrota al Resonante, entrega la recompensa, rechaza una segunda entrega y equipa el objeto.

Builder reproducible:

```bash
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -quit \
  -projectPath "/Users/migueltroncoso/Documents/Juego 2D" \
  -executeMethod Lumbre.Game.Editor.H5MissionBuilder.Build \
  -logFile /tmp/lumbre-h5-build.log
```

Pruebas reproducibles:

```bash
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -projectPath "/Users/migueltroncoso/Documents/Juego 2D" \
  -runTests -testPlatform editmode \
  -testResults /tmp/lumbre-h5-editmode.xml \
  -testFilter Lumbre.Game.Tests \
  -logFile /tmp/lumbre-h5-editmode.log
```

```bash
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -projectPath "/Users/migueltroncoso/Documents/Juego 2D" \
  -runTests -testPlatform playmode \
  -testResults /tmp/lumbre-h5-playmode.xml \
  -testFilter Lumbre.Game.Tests \
  -logFile /tmp/lumbre-h5-playmode.log
```

## Resultado H5

- Builder H5: terminó en batchmode con código de salida 0.
- EditMode: 17 tests pasados, 0 fallidos.
- PlayMode: 4 tests pasados, 0 fallidos; incluye recorrido H3, combate H4, demo H4B y misión completa H5.
- No se requieren assets finales: Nara, botones y HUD usan presentación greybox/provisional.

## H6 progresión y persistencia

`H6ProgressionPersistenceTests` cubre acumulación de XP, nivel máximo, eventos de XP/nivel, deduplicación, snapshots DTO, misión/inventario/equipamiento restaurados, archivo inexistente, JSON corrupto, versión incompatible y replay bloqueado. `H6ProgressionPersistencePlayModeTests` completa el vertical slice, alcanza nivel 2, equipa la reliquia, guarda, recarga `VerticalSlice` y valida todo el estado persistente, incluida la posición segura.

Builder reproducible:

```bash
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -quit \
  -projectPath "/Users/migueltroncoso/Documents/Juego 2D" \
  -executeMethod Lumbre.Game.Editor.H6ProgressionBuilder.Build \
  -logFile /tmp/lumbre-h6-build.log
```

Pruebas reproducibles:

```bash
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -projectPath "/Users/migueltroncoso/Documents/Juego 2D" \
  -runTests -testPlatform editmode \
  -testResults /tmp/lumbre-h6-editmode.xml \
  -testFilter Lumbre.Game.Tests \
  -logFile /tmp/lumbre-h6-editmode.log
```

```bash
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -projectPath "/Users/migueltroncoso/Documents/Juego 2D" \
  -runTests -testPlatform playmode \
  -testResults /tmp/lumbre-h6-playmode.xml \
  -testFilter Lumbre.Game.Tests \
  -logFile /tmp/lumbre-h6-playmode.log
```

## Resultado H6

- Builder H6: terminó en batchmode con código de salida 0.
- EditMode: 23 tests pasados, 0 fallidos.
- PlayMode: 5 tests pasados, 0 fallidos; incluye H3, H4, H4B, H5 y restauración H6 después de recargar la escena.
- El repositorio local no deja archivos temporales tras una escritura correcta.

## H7 arte, audio y presentación

`H7PresentationPlayModeTests` carga el Vertical Slice y comprueba que el runtime de presentación, el pool de VFX, los ambientes, la cámara Cinemachine, el mundo, el HUD y los seis personajes tengan configuración válida. También guarda, recarga la escena y verifica que la presentación siga presente junto al estado cargado.

Builder reproducible:

```bash
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics \
  -projectPath "/Users/migueltroncoso/Documents/Juego 2D" \
  -executeMethod Lumbre.Game.Editor.H7PresentationBuilder.Build \
  -quit -logFile /tmp/lumbre-h7-builder.log
```

Pruebas reproducibles:

```bash
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -projectPath "/Users/migueltroncoso/Documents/Juego 2D" \
  -runTests -testPlatform editmode \
  -testResults /tmp/lumbre-h7-editmode.xml \
  -testFilter Lumbre.Game.Tests \
  -logFile /tmp/lumbre-h7-editmode.log
```

```bash
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -projectPath "/Users/migueltroncoso/Documents/Juego 2D" \
  -runTests -testPlatform playmode \
  -testResults /tmp/lumbre-h7-playmode.xml \
  -testFilter Lumbre.Game.Tests \
  -logFile /tmp/lumbre-h7-playmode.log
```

## Resultado H7

- Builder H7 final: terminó en batchmode con código de salida 0.
- EditMode: 23 tests pasados, 0 fallidos.
- PlayMode: 6 tests pasados, 0 fallidos; incluye la nueva prueba H7 y las regresiones H3–H6.
- La primera ejecución detectó un log no esperado por un `ParticleSystem` que nacía reproduciéndose; el pool ahora lo detiene antes de configurarlo y la segunda ejecución quedó limpia.
- El perfilado sobre hardware Android, temperatura, batería y draw calls sigue siendo responsabilidad de H9.

## H8 optimización, UX y preparación Android

`H8UxSettingsPlayModeTests` carga el Vertical Slice y comprueba que el controlador de pausa/opciones, el overlay de rendimiento, el HUD y los tooltips estén configurados. Cambia música, FX, vibración, FPS y debug, verifica persistencia local, pausa, apertura/cierre de opciones, continuación y salida solicitada sin dejar el tiempo global detenido.

Builder reproducible:

```bash
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics -quit \
  -projectPath "/Users/migueltroncoso/Documents/Juego 2D" \
  -executeMethod Lumbre.Game.Editor.H8OptimizationBuilder.Build \
  -logFile /tmp/lumbre-h8-builder.log
```

Build Android de desarrollo reproducible:

```bash
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics -quit \
  -projectPath "/Users/migueltroncoso/Documents/Juego 2D" \
  -executeMethod Lumbre.Game.Editor.H8OptimizationBuilder.BuildAndroidDevelopment \
  -logFile /tmp/lumbre-h8-android-build.log
```

Pruebas finales:

```bash
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics -projectPath "/Users/migueltroncoso/Documents/Juego 2D" \
  -runTests -testPlatform editmode \
  -testResults /tmp/lumbre-h8-editmode-final.xml \
  -testFilter Lumbre.Game.Tests \
  -logFile /tmp/lumbre-h8-editmode-final.log
```

```bash
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics -projectPath "/Users/migueltroncoso/Documents/Juego 2D" \
  -runTests -testPlatform playmode \
  -testResults /tmp/lumbre-h8-playmode-final.xml \
  -testFilter Lumbre.Game.Tests \
  -logFile /tmp/lumbre-h8-playmode-final.log
```

## Resultado H8

- Builder H8 ejecutado dos veces: código de salida 0 en ambas ejecuciones y scene marker `H8OptimizationUx` persistente.
- EditMode: 23 tests pasados, 0 fallidos.
- PlayMode: 7 tests pasados, 0 fallidos; incluye las regresiones H3–H7 y la prueba H8 de UX/configuración.
- No aparecen errores de compilación, `NullReferenceException`, `MissingComponent`, `AssertionException` ni logs no esperados del proyecto en la ejecución final.
- El APK Android de desarrollo se generó en `/tmp/lumbre-h8-android.apk`; el módulo Android/SDK/NDK/OpenJDK está instalado.
- No había dispositivo conectado: `adb devices` no mostró entradas. El arranque y las métricas de hardware quedan pendientes de H9.

## H8.1 diagnóstico de arranque Android

`H8_1BootstrapPlayModeTests` carga la escena real `Bootstrap`, espera la transición a `VerticalSlice` y comprueba Build Settings, jugador, cámara principal/Cinemachine, Canvas, `InputSystemUIInputModule`, AudioListener, HUD, paneles cerrados, `Time.timeScale` y fallback para preferencias locales inválidas. El builder valida Bootstrap en índice 0 y VerticalSlice habilitada antes de cualquier APK.

Resultado reproducible:

- Builder H8.1: código de salida 0.
- EditMode: 23/23 pasados.
- PlayMode: 8/8 pasados.
- APK: `/tmp/lumbre-h8-1-black-screen-debug.apk`.
- Logcat físico: pendiente; el ADB de Unity mostró lista de dispositivos vacía.

Checklist físico pendiente:

1. Instalación limpia y captura de logcat.
2. Segundo arranque.
3. Arranque con save local.
4. Arranque después de minimizar y restaurar.
5. Arranque después de forzar cierre.

En cada caso deben aparecer `[BOOT] Bootstrap Awake`, `[BOOT] Bootstrap Start`, `[BOOT] Loading VerticalSlice`, `[BOOT] VerticalSlice activated`, jugador, cámara y HUD, sin pantalla negra, crash ni `AndroidRuntime` fatal. H9 no se inicia hasta cerrar este checklist.

## H9 pulido del vertical slice

`H9VerticalSlicePolishPlayModeTests` valida la escena y el builder: la raíz `H9_SafeAreaRoot` usa `Screen.safeArea`, el Canvas mantiene la referencia 1920×1080, la cámara oficial conserva seguimiento Cinemachine, damping, composición, zoom y confiner, y el joystick, botones y HUD respetan el layout compacto. `H9VerticalSlicePolishBuilder.Validate` ejecuta la comprobación estructural sin reconstruir gameplay.

Builder y validación reproducibles:

```bash
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics -quit \
  -projectPath "/Users/migueltroncoso/Documents/Juego 2D" \
  -executeMethod Lumbre.Game.Editor.H9VerticalSlicePolishBuilder.Build \
  -logFile /tmp/lumbre-h9-builder.log
```

```bash
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics -quit \
  -projectPath "/Users/migueltroncoso/Documents/Juego 2D" \
  -executeMethod Lumbre.Game.Editor.H9VerticalSlicePolishBuilder.Validate \
  -logFile /tmp/lumbre-h9-validate.log
```

Suite automática prevista:

```bash
"/Applications/Unity/Hub/Editor/6000.3.20f1/Unity.app/Contents/MacOS/Unity" \
  -batchmode -nographics -projectPath "/Users/migueltroncoso/Documents/Juego 2D" \
  -runTests -testPlatform playmode \
  -testResults /tmp/lumbre-h9-playmode.xml \
  -testFilter Lumbre.Game.Tests \
  -logFile /tmp/lumbre-h9-playmode.log
```

No se debe añadir `-quit` a una ejecución `-runTests`: Unity cerraría antes de que el Test Framework guarde el XML. Con el comando anterior, el runner completó correctamente.

Resultado H9.1 de esta estación:

- Builder H9/Validate: cuatro ejecuciones, todas con código 0; dos ciclos Build → Validate.
- EditMode: 23/23 pasados, 0 fallidos, 0 ignorados. XML: `/tmp/lumbre-h9-editmode-final.qZqdTZ/results.xml`.
- PlayMode: 9/9 pasados, 0 fallidos, 0 ignorados. XML: `/tmp/lumbre-h9-playmode-final.lMz5Vs/results.xml`.
- Las regresiones H3, H4, H4B, H5, H6, H7, H8.1 y H8 están incluidas en PlayMode.
- APK H9: generado correctamente en `/tmp/lumbre-h9-android.apk`.
- Validación física, capturas y métricas: bloqueadas porque el ADB incluido en Unity no detectó un dispositivo.

La corrección H9.1 sincroniza el offset de composición serializado del binding H3 con el `CinemachineFollow` H9; no modifica gameplay ni contratos de dominio.

## Criterio de calidad H4–H8

- cero errores de compilación;
- Bootstrap y VerticalSlice aparecen en la lista de escenas;
- Cinemachine se resuelve y sus componentes están presentes en las escenas;
- el asset de acciones importa sin error;
- los tests EditMode y PlayMode pasan;
- la escena contiene Layers y Gizmos de depuración verificables;
- el joystick y la entrada de QA comparten `MovementIntent`;
- Mordeluz detecta, sigue, ataca, retorna y muere correctamente;
- el ataque básico respeta cooldown y el daño no puede reducir la salud por debajo de cero;
- defensa respeta reducción, duración y cooldown;
- área respeta Calor, radio y cooldown;
- la onda muestra anticipación y solo daña dentro de su zona;
- la misión solo progresa por eventos de dominio y no cuenta dos veces el mismo `EntityId`;
- la entrega fija es idempotente: una segunda interacción no aumenta el inventario;
- el inventario rechaza duplicados y respeta seis espacios;
- la recompensa equipa en la única ranura de reliquia sin salir del inventario;
- el recorrido suma exactamente 100 XP y alcanza nivel 2 una sola vez;
- una derrota repetida no vuelve a conceder XP ni progreso;
- el archivo inexistente, corrupto o incompatible inicia una partida limpia y registra el diagnóstico;
- el save usa DTOs y no serializa componentes Unity;
- la recarga restaura posición segura, misión, progreso, XP, nivel, inventario y equipamiento;
- cada personaje y enemigo del slice tiene SpriteRenderer y Animator configurados;
- el pool de VFX respeta prewarm y máximo activo;
- ataque, habilidades, daño, muerte, misión, recompensa, equipamiento y nivel disparan feedback visual/sonoro sin cambiar el dominio;
- HUD y ambientes siguen configurados después de recargar la escena;
- el HUD refresca el texto a una frecuencia limitada y conserva feedback animado;
- la pausa detiene y reanuda el tiempo sin alterar el estado de gameplay;
- música, FX, vibración, calidad, FPS y debug persisten como configuración local independiente del save;
- los tooltips comparten un panel y se configuran en los controles de opciones;
- el overlay de QA expone FPS, frame time, memoria, GC, draw calls y lecturas CPU/GPU opcionales sin estar activo por defecto;
- el builder H8 es idempotente y el APK Android se empaqueta en batchmode;
- no se introducen tienda, economía, monedas, crafting, profesiones, más niveles, nube, cuentas, multiplayer ni networking.

El paquete oficial Cinemachine 3.1.7 puede emitir un warning de referencia de una muestra HDRP opcional (`ExposeHDRPInternals`) al importar sus archivos de editor; no corresponde a código del proyecto, no bloquea la compilación ni aparece como error de script. Se conserva visible para no ocultar señales del entorno.
