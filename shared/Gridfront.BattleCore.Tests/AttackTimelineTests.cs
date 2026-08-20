using Gridfront.BattleCore.Combat;

namespace Gridfront.BattleCore.Tests
{
    public sealed class AttackTimelineTests
    {
        [Fact]
        public void WindupReachesHitExactlyOnce()
        {
            var clock = new AttackTimeline(windupTicks: 2, recoveryTicks: 1);
            var hits = 0;

            var start = clock.Step(acquiredTargetId: 7, lockedTargetStillValid: true);
            Assert.Equal(AttackPhase.Windup, start.Phase);
            Assert.False(start.Hit);

            var winding = clock.Step(7, true);
            Assert.Equal(AttackPhase.Windup, winding.Phase);
            Assert.False(winding.Hit);

            var hit = clock.Step(7, true);
            Assert.True(hit.Hit);
            Assert.Equal(AttackPhase.Recovery, hit.Phase);
            hits++;

            var recovered = clock.Step(7, true);
            Assert.False(recovered.Hit);
            Assert.Equal(AttackPhase.Idle, recovered.Phase);

            Assert.Equal(1, hits);
        }

        [Fact]
        public void CancelsWindupWithoutHitWhenTargetBecomesInvalid()
        {
            var clock = new AttackTimeline(windupTicks: 3, recoveryTicks: 1);
            clock.Step(acquiredTargetId: 4, lockedTargetStillValid: true);
            clock.Step(4, true);

            var cancelled = clock.Step(4, lockedTargetStillValid: false);

            Assert.False(cancelled.Hit);
            Assert.Equal(AttackPhase.Idle, cancelled.Phase);
            Assert.Null(clock.LockedTargetId);
        }
    }

    public sealed class DamageTests
    {
        [Fact]
        public void PhysicalUsesMaxOfRawAndMinimum()
        {
            Assert.Equal(7, Damage.Physical(attack: 12, defense: 5, minDamage: 1));
            Assert.Equal(3, Damage.Physical(attack: 4, defense: 10, minDamage: 3));
        }
    }
}
