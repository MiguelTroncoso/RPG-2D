using System;
using Lumbre.Game.Domain.Animation;
using Lumbre.Game.Domain.Combat;
using NUnit.Framework;

namespace Lumbre.Game.Tests
{
    public sealed class H103BodyCombatTests
    {
        // ---- Attack sequence: damage exactly at the visual impact ----

        [Test]
        public void AttackSequenceVisitsEveryPhaseInOrder()
        {
            var sequence = new AttackSequenceModel(0.14f, 0.1f, 0.08f, 0.16f);
            Assert.IsTrue(sequence.TryStart(0f));
            Assert.AreEqual(AttackPhase.Windup, sequence.Phase);

            Assert.AreEqual(AttackPhase.Windup, PhaseAt(sequence, 0.05f));
            Assert.AreEqual(AttackPhase.Strike, PhaseAt(sequence, 0.2f));
            Assert.AreEqual(AttackPhase.Impact, PhaseAt(sequence, 0.27f));
            Assert.AreEqual(AttackPhase.Recovery, PhaseAt(sequence, 0.4f));
        }

        [Test]
        public void AttackSequenceFiresImpactExactlyOnceAtImpactDelay()
        {
            var sequence = new AttackSequenceModel(0.14f, 0.1f, 0.08f, 0.16f);
            sequence.TryStart(0f);
            Assert.AreEqual(0.24f, sequence.ImpactDelaySeconds, 1e-4f);

            var impactCount = 0;
            var impactTime = -1f;
            for (var t = 0f; t <= sequence.TotalSeconds + 0.1f; t += 1f / 60f)
            {
                var tick = sequence.Tick(t);
                if (tick.ImpactStarted)
                {
                    impactCount++;
                    impactTime = t;
                }
            }

            Assert.AreEqual(1, impactCount, "impact must fire exactly once");
            // Impact fires no earlier than windup+strike, and within one frame.
            Assert.GreaterOrEqual(impactTime, 0.24f - 1e-4f);
            Assert.LessOrEqual(impactTime, 0.24f + 1f / 60f + 1e-4f);
        }

        [Test]
        public void AttackImpactNeverDropsOnLargeFrameSkip()
        {
            var sequence = new AttackSequenceModel(0.14f, 0.1f, 0.08f, 0.16f);
            sequence.TryStart(0f);

            // A single huge delta jumps past the whole impact window.
            var tick = sequence.Tick(5f);

            Assert.IsTrue(tick.ImpactStarted, "impact must still fire once");
            Assert.IsTrue(tick.Completed);
            Assert.AreEqual(AttackPhase.Idle, sequence.Phase);
        }

        [Test]
        public void AttackSequenceCannotRestartWhileActive()
        {
            var sequence = new AttackSequenceModel(0.14f, 0.1f, 0.08f, 0.16f);
            Assert.IsTrue(sequence.TryStart(0f));
            Assert.IsFalse(sequence.TryStart(0.05f));
        }

        [Test]
        public void AttackSequenceCompletesBackToIdle()
        {
            var sequence = new AttackSequenceModel(0.14f, 0.1f, 0.08f, 0.16f);
            sequence.TryStart(0f);
            var completed = false;
            for (var t = 0f; t <= sequence.TotalSeconds + 0.05f; t += 1f / 120f)
            {
                completed |= sequence.Tick(t).Completed;
            }

            Assert.IsTrue(completed);
            Assert.AreEqual(AttackPhase.Idle, sequence.Phase);
            Assert.IsFalse(sequence.IsActive);
        }

        [Test]
        public void AttackSequenceCancelClearsState()
        {
            var sequence = new AttackSequenceModel(0.14f, 0.1f, 0.08f, 0.16f);
            sequence.TryStart(0f);
            sequence.Cancel();
            Assert.AreEqual(AttackPhase.Idle, sequence.Phase);
            Assert.IsFalse(sequence.IsActive);
            // After cancel it can start again and re-fire its impact.
            sequence.TryStart(1f);
            var tick = sequence.Tick(1f + sequence.TotalSeconds);
            Assert.IsTrue(tick.ImpactStarted);
        }

        // ---- Hit reaction: visible stagger + bounded knockback ----

        [Test]
        public void HitReactionStaggersForConfiguredWindow()
        {
            var reaction = new HitReactionModel(0.28f, 0.3f, 0.2f);
            Assert.IsFalse(reaction.IsStaggered(0f));

            reaction.ApplyHit(1f, 1f, 0f);
            Assert.IsTrue(reaction.IsStaggered(1f));
            Assert.IsTrue(reaction.IsStaggered(1.27f));
            Assert.IsFalse(reaction.IsStaggered(1.29f));
        }

        [Test]
        public void HitReactionKnockbackNeverExceedsConfiguredDistance()
        {
            var reaction = new HitReactionModel(0.28f, 0.3f, 0.2f);
            reaction.ApplyHit(0f, 1f, 0f);

            var totalX = 0f;
            var totalY = 0f;
            for (var t = 0f; t <= 0.5f; t += 1f / 60f)
            {
                var (x, y) = reaction.TickDisplacement(t);
                totalX += x;
                totalY += y;
            }

            Assert.AreEqual(0.3f, totalX, 1e-3f);
            Assert.AreEqual(0f, totalY, 1e-6f);
        }

        [Test]
        public void HitReactionPushesAwayFromSource()
        {
            var reaction = new HitReactionModel(0.28f, 0.3f, 0.2f);
            // Source to the right of the enemy -> push left.
            reaction.ApplyHit(0f, -1f, 0f);
            var (x, _) = reaction.TickDisplacement(0.05f);
            Assert.Less(x, 0f);
        }

        [Test]
        public void HitReactionZeroDirectionFallsBackWithoutNaN()
        {
            var reaction = new HitReactionModel(0.28f, 0.3f, 0.2f);
            reaction.ApplyHit(0f, 0f, 0f);
            var (x, y) = reaction.TickDisplacement(0.05f);
            Assert.IsFalse(float.IsNaN(x));
            Assert.IsFalse(float.IsNaN(y));
            Assert.Greater(x, 0f);
        }

        [Test]
        public void HitReactionResetClearsStagger()
        {
            var reaction = new HitReactionModel(0.28f, 0.3f, 0.2f);
            reaction.ApplyHit(1f, 1f, 0f);
            reaction.Reset();
            Assert.IsFalse(reaction.IsStaggered(1f));
        }

        // ---- Body pose model: stride synced to physical travel ----

        [Test]
        public void StridePhaseIsFrameRateIndependent()
        {
            var high = new PlayerBodyPoseModel();
            var low = new PlayerBodyPoseModel();
            // Same distance travelled, different frame rates.
            for (var i = 0; i < 60; i++)
            {
                high.Tick(1f, 0.92f / 60f, 1f / 60f);
            }

            for (var i = 0; i < 30; i++)
            {
                low.Tick(1f, 0.92f / 30f, 1f / 30f);
            }

            // Compare stride phases as angles on the unit circle (phase wraps
            // at 2*pi, so a raw subtraction would falsely flag equal poses).
            var delta = high.StridePhase - low.StridePhase;
            var wrapped = Math.Abs(Math.Atan2(Math.Sin(delta), Math.Cos(delta)));
            Assert.Less(wrapped, 0.01f);
        }

        [Test]
        public void IdleKeepsStrideFrozenAndAmplitudeLow()
        {
            var model = new PlayerBodyPoseModel();
            for (var i = 0; i < 60; i++)
            {
                model.Tick(0f, 0f, 1f / 60f);
            }

            Assert.AreEqual(0f, model.StridePhase, 1e-6f);
            Assert.Less(model.Amplitude, 0.01f);
        }

        [Test]
        public void WalkAmplitudeScalesWithJoystickIntensity()
        {
            var slow = new PlayerBodyPoseModel();
            var fast = new PlayerBodyPoseModel();
            for (var i = 0; i < 120; i++)
            {
                slow.Tick(0.3f, 0.3f * 2.4f / 60f, 1f / 60f);
                fast.Tick(1f, 2.4f / 60f, 1f / 60f);
            }

            Assert.Less(slow.Amplitude, fast.Amplitude);
        }

        [Test]
        public void MovingLegsSwingInOpposition()
        {
            var model = new PlayerBodyPoseModel();
            PlayerBodyPose pose = default;
            for (var i = 0; i < 20; i++)
            {
                pose = model.Tick(1f, 2.4f / 60f, 1f / 60f);
            }

            // Legs must move opposite each other for a believable gait.
            Assert.AreEqual(-pose.FrontLegAngle, pose.BackLegAngle, 1e-3f);
        }

        [Test]
        public void PoseResetReturnsToRest()
        {
            var model = new PlayerBodyPoseModel();
            for (var i = 0; i < 30; i++)
            {
                model.Tick(1f, 2.4f / 60f, 1f / 60f);
            }

            model.Reset();
            Assert.AreEqual(0f, model.StridePhase, 1e-6f);
            Assert.AreEqual(0f, model.Amplitude, 1e-6f);
        }

        // ---- Attack pose curve: neutral at idle, blends between key poses ----

        [Test]
        public void AttackPoseCurveReturnsNeutralWhenIdle()
        {
            var pose = PlayerAttackPoseCurve.Sample(AttackPhase.Idle, 0.5f);
            Assert.AreEqual(0f, pose.ArmAngle, 1e-6f);
            Assert.AreEqual(0f, pose.TorsoAngle, 1e-6f);
        }

        [Test]
        public void AttackPoseCurveSwingsSwordArmForwardByImpact()
        {
            var windup = PlayerAttackPoseCurve.Sample(AttackPhase.Windup, 1f);
            var impact = PlayerAttackPoseCurve.Sample(AttackPhase.Impact, 1f);
            // Windup pulls the arm back (positive), impact drives it forward
            // (strongly negative): the swing must reverse direction.
            Assert.Greater(windup.ArmAngle, 0f);
            Assert.Less(impact.ArmAngle, windup.ArmAngle);
        }

        [Test]
        public void BodyPoseBlendClampsWeight()
        {
            var a = PlayerBodyPose.Neutral;
            var b = new PlayerBodyPose(10f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f);
            Assert.AreEqual(10f, PlayerBodyPose.Blend(a, b, 2f).TorsoAngle, 1e-4f);
            Assert.AreEqual(0f, PlayerBodyPose.Blend(a, b, -1f).TorsoAngle, 1e-4f);
            Assert.AreEqual(5f, PlayerBodyPose.Blend(a, b, 0.5f).TorsoAngle, 1e-4f);
        }

        private static AttackPhase PhaseAt(AttackSequenceModel sequence, float time)
        {
            sequence.Tick(time);
            return sequence.Phase;
        }
    }
}
