# H10.3 — Matriz de Aceptación

> Proyecto: Lumbre de Nácar · Versión: v0.8.1 Alpha · Rama: `claude/h10-3-body-animation-combat-80s54o`

## Nota sobre el entorno de verificación

Este entorno **no dispone de Unity Editor ni de .NET SDK** (el host de descarga
de dotnet está bloqueado por política de red). En consecuencia:

- **EditMode / PlayMode / Builder / Validate no pudieron ejecutarse aquí.**
- La lógica de dominio (secuencia de ataque, reacción de golpe, poses de
  caminata, mezcla de poses) se verificó con un **arnés de puerto a Python** que
  reproduce la matemática exacta de los modelos C#: todas las aserciones pasaron
  (impacto único en el instante correcto, knockback acotado, zancada
  independiente de framerate, idle sin avance de zancada, amplitud proporcional
  a la intensidad, mezcla clamped).
- La integridad de la escena (referencias del rig, GUIDs de sprites, parentesco,
  renderer base deshabilitado) se validó por inspección estructural del YAML.

Por diseño y por mandato del hito, **no se declara H10.3 aprobado**. La
aprobación requiere ejecutar las suites en Unity y una prueba física Android.

Leyenda de estado:
- ✅ Implementado y verificado por lógica/estructura (pendiente de ejecución en Unity).
- ⏳ Implementado; requiere ejecución en Unity para confirmar.
- 📱 Requiere validación física.

## 1. Locomoción

| Requisito | Implementación | Verificación | Estado |
| --- | --- | --- | --- |
| Idle claro | Amplitud decae a ~0 + respiración sutil | EditMode `IdleKeepsStrideFrozenAndAmplitudeLow` | ✅⏳ |
| Walk real (movimiento corporal) | Rig cutout: piernas en oposición, torso, cadera, capa | EditMode `MovingLegsSwingInOpposition`; preview | ✅⏳ |
| Transición Idle ↔ Walk | Amplitud suavizada (sube 10/s, baja 6/s) | Revisión + preview | ✅⏳ |
| Sincronía física ↔ Animator | Zancada ligada a distancia; `animator.speed` ∝ velocidad | EditMode `StridePhaseIsFrameRateIndependent`; PlayMode `WalkingAdvancesTheStrideWhileIdleDoesNot` | ✅⏳ |
| Facing estable | Histéresis sobre componente X del look | PlayMode `FacingStaysStableAcrossDiagonalInput` | ✅⏳ |
| Ocho direcciones | Locomoción física y facing heredados de H3/H10 (8 dir.) | PlayMode H10 (N/S/E/O + diagonal) existentes | ✅⏳ |
| Respuesta proporcional al joystick | Amplitud ∝ `speed^0.7` | EditMode `WalkAmplitudeScalesWithJoystickIntensity` | ✅⏳ |
| No depende solo de escala/tilt/partículas/cámara | Movimiento por huesos reales del rig | Diseño (rig cutout) | ✅ |

## 2. Ataque por fases

| Requisito | Implementación | Verificación | Estado |
| --- | --- | --- | --- |
| Fases Idle→Prep→Golpe→Impacto→Recuperación→Idle | `AttackSequenceModel` + `PlayerAttackPoseCurve` | EditMode `AttackSequenceVisitsEveryPhaseInOrder`; PlayMode `AttackProgressesThroughVisiblePhases` | ✅⏳ |
| Daño exactamente en el impacto (no antes) | `ImpactStarted` tras windup+strike | EditMode `AttackSequenceFiresImpactExactlyOnceAtImpactDelay`; PlayMode `DamageLandsAtImpactNeverAtButtonPress` | ✅⏳ |
| Daño exactamente una vez (no después/duplicado) | Latch `_impactFired` | EditMode `AttackImpactNeverDropsOnLargeFrameSkip` | ✅⏳ |
| Recuperación y retorno a Idle | Secuencia completa libera estado | EditMode `AttackSequenceCompletesBackToIdle`; PlayMode `AttackReleasesTheActionStateAfterRecovery` | ✅⏳ |
| Legible sin partículas | Poses de cuerpo (windup atrás, impacto extendido) | Preview `attack-phases-preview.png` | ✅⏳📱 |

## 3. Reacción del enemigo

| Requisito | Implementación | Verificación | Estado |
| --- | --- | --- | --- |
| Reacción visible al recibir daño | Stagger + knockback + flash + estado Damage | PlayMode `EnemyStaggersVisiblyWhenItTakesDamage` | ✅⏳ |
| Knockback pequeño | 0.15 u con easing, acotado | EditMode `HitReactionKnockbackNeverExceedsConfiguredDistance`; PlayMode `EnemyIsPushedAwayFromThePlayerOnHit` | ✅⏳ |
| Interrupción breve | Stagger 0.28 s interrumpe IA | EditMode `HitReactionStaggersForConfiguredWindow` | ✅⏳ |
| Se limpia (no congela) | Stagger expira; `Reset` en muerte | EditMode `HitReactionResetClearsStagger`; PlayMode confirma `IsStaggered` vuelve a false | ✅⏳ |

## 4. DEF y HAB

| Requisito | Implementación | Verificación | Estado |
| --- | --- | --- | --- |
| Mantener efectos actuales | Sin cambios en `H4BAbilityFeedback` / VFX | Revisión | ✅ |
| No dejar estados bloqueados | `try/finally` libera la compuerta | PlayMode `DefenseInputReleasesTheActionStateForFollowUpActions`, `AreaAttackReleasesTheActionState` | ✅⏳ |
| Sincronizados con animación | Efectos ligados a eventos de activación | Revisión | ✅⏳ |
| Se limpian correctamente | Modificador de defensa se retira al expirar | PlayMode `DefenseCleansUpAndDoesNotLockTheActionState` | ✅⏳ |

## 5. Arquitectura

| Requisito | Estado |
| --- | --- |
| Unity 6 LTS / Input System / Cinemachine sin cambios | ✅ |
| Clean Architecture (lógica en Domain puro) | ✅ |
| Offline First | ✅ |
| No romper H1–H10 | ⏳ (requiere correr regresiones en Unity) |
| No modificar networking | ✅ |
| No iniciar H11 | ✅ |
| Nunca serializar estado runtime | ✅ (modelos transitorios, reconstruidos en Awake) |

## 6. Pruebas — estado

| Suite | Estado |
| --- | --- |
| EditMode (existentes + `H103BodyCombatTests`, 20 casos nuevos) | ⏳ escritos; no ejecutados aquí (sin Unity) |
| PlayMode (existentes ajustados + `H103BodyCombatPlayModeTests`, 11 casos) | ⏳ escritos; no ejecutados aquí |
| Builder (`H103BodyCombatBuilder.Build`) | ⏳ no ejecutado |
| Validate (`H103BodyCombatBuilder.Validate`) | ⏳ no ejecutado |
| Puerto de lógica de dominio a Python | ✅ todas las aserciones pasan |

### Tests existentes ajustados (y por qué)

Tres tests PlayMode de H10 asumían **daño instantáneo al pulsar**, que es
exactamente el "ataque fantasma" rechazado. Se ajustaron para esperar al impacto
(`WaitForAttackImpact`) manteniendo lo que verifican (el ataque táctil/gamepad
daña al enemigo) pero con la temporización correcta del hito:
`KeyboardAndGamepadAttackDamagesTheTarget`, `TouchAttackUsesTheSameActionAsGamepadAttack`,
`OneTouchPressProducesOneBasicAttack`. El primero además ahora **afirma que no
hay daño antes del impacto**.

## 7. Pendiente para aprobación

1. Ejecutar en Unity: EditMode, PlayMode, `Build`, `Validate` — todos en verde.
2. Generar APK y realizar **prueba física Android** de la sensación (peso de la
   caminata, legibilidad del ataque, reacción del enemigo) — el criterio que
   rechazó H10.2.
3. Revisión del Director Técnico y del propietario del proyecto.

## Veredicto

**No aprobado automáticamente.** La implementación está completa y la lógica de
dominio verificada, pero la ejecución de suites en Unity y la prueba física
Android son condición necesaria. Ver la sección de recomendación en la entrega.
