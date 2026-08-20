using System;

namespace Gridfront.BattleCore.Combat
{
    public enum AttackPhase : byte
    {
        Idle = 0,
        Windup = 1,
        Recovery = 2
    }

    public readonly struct AttackStepResult
    {
        public AttackStepResult(AttackPhase phase, bool hit)
        {
            Phase = phase;
            Hit = hit;
        }

        public AttackPhase Phase { get; }

        public bool Hit { get; }
    }

    public sealed class AttackTimeline
    {
        public AttackTimeline(int windupTicks, int recoveryTicks)
        {
            if (windupTicks < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(windupTicks), windupTicks, "Windup must be at least 1 tick.");
            }

            if (recoveryTicks < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(recoveryTicks), recoveryTicks, "Recovery must be at least 1 tick.");
            }

            WindupTicks = windupTicks;
            RecoveryTicks = recoveryTicks;
            Phase = AttackPhase.Idle;
        }

        public int WindupTicks { get; }

        public int RecoveryTicks { get; }

        public AttackPhase Phase { get; private set; }

        public int ElapsedInPhase { get; private set; }

        public int? LockedTargetId { get; private set; }

        public AttackStepResult Step(int? acquiredTargetId, bool lockedTargetStillValid)
        {
            switch (Phase)
            {
                case AttackPhase.Idle:
                    return StepIdle(acquiredTargetId);
                case AttackPhase.Windup:
                    return StepWindup(lockedTargetStillValid);
                case AttackPhase.Recovery:
                    return StepRecovery();
                default:
                    throw new InvalidOperationException("Unknown attack phase.");
            }
        }

        private AttackStepResult StepIdle(int? acquiredTargetId)
        {
            if (!acquiredTargetId.HasValue)
            {
                return new AttackStepResult(AttackPhase.Idle, false);
            }

            LockedTargetId = acquiredTargetId;
            Phase = AttackPhase.Windup;
            ElapsedInPhase = 0;
            return new AttackStepResult(AttackPhase.Windup, false);
        }

        private AttackStepResult StepWindup(bool lockedTargetStillValid)
        {
            if (!lockedTargetStillValid)
            {
                LockedTargetId = null;
                Phase = AttackPhase.Idle;
                ElapsedInPhase = 0;
                return new AttackStepResult(AttackPhase.Idle, false);
            }

            ElapsedInPhase += 1;
            if (ElapsedInPhase < WindupTicks)
            {
                return new AttackStepResult(AttackPhase.Windup, false);
            }

            Phase = AttackPhase.Recovery;
            ElapsedInPhase = 0;
            return new AttackStepResult(AttackPhase.Recovery, true);
        }

        private AttackStepResult StepRecovery()
        {
            ElapsedInPhase += 1;
            if (ElapsedInPhase < RecoveryTicks)
            {
                return new AttackStepResult(AttackPhase.Recovery, false);
            }

            LockedTargetId = null;
            Phase = AttackPhase.Idle;
            ElapsedInPhase = 0;
            return new AttackStepResult(AttackPhase.Idle, false);
        }
    }
}
