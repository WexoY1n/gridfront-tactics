using System;
using System.Collections.Generic;
using Gridfront.BattleCore.Domain;
using Gridfront.BattleCore.Replay;

namespace Gridfront.BattleCore
{
    /// <summary>
    /// Fixed-tick battle driver. Systems may be empty; determinism still holds.
    /// </summary>
    public sealed class BattleRunner
    {
        private readonly List<IBattleCommand> _pending = new List<IBattleCommand>();
        private readonly List<AppliedCommandRecord> _applied = new List<AppliedCommandRecord>();
        private readonly HashSet<long> _seenSequences = new HashSet<long>();

        private BattleRunner(ulong seed)
        {
            Seed = seed;
            Phase = BattlePhase.Running;
            Tick = 0;
            Checksum = string.Empty;
            RefreshChecksum();
        }

        public ulong Seed { get; }

        public int Tick { get; private set; }

        public BattlePhase Phase { get; private set; }

        public string Checksum { get; private set; }

        public int AppliedCommandCount => _applied.Count;

        public static BattleRunner Create(ulong seed)
        {
            return new BattleRunner(seed);
        }

        public void Enqueue(IBattleCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            if (Phase != BattlePhase.Running)
            {
                throw new InvalidOperationException("Cannot enqueue commands after the battle has ended.");
            }

            if (command.ExecuteAtTick < Tick)
            {
                throw new InvalidOperationException(
                    $"Command sequence {command.Sequence} targets tick {command.ExecuteAtTick}, which is already past (current tick {Tick}).");
            }

            if (!_seenSequences.Add(command.Sequence))
            {
                throw new InvalidOperationException($"Duplicate command sequence: {command.Sequence}.");
            }

            _pending.Add(command);
        }

        public void Step()
        {
            if (Phase != BattlePhase.Running)
            {
                throw new InvalidOperationException("Cannot step after the battle has ended.");
            }

            ApplyDueCommands(Tick);
            Tick += 1;
            RefreshChecksum();
        }

        public void Step(int times)
        {
            if (times < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(times), times, "Step count must be non-negative.");
            }

            for (var i = 0; i < times; i++)
            {
                Step();
            }
        }

        public void EndBattle()
        {
            if (Phase == BattlePhase.Ended)
            {
                throw new InvalidOperationException("Battle is already ended.");
            }

            Phase = BattlePhase.Ended;
            RefreshChecksum();
        }

        private void ApplyDueCommands(int tick)
        {
            if (_pending.Count == 0)
            {
                return;
            }

            _pending.Sort(CompareCommands);

            var index = 0;
            while (index < _pending.Count)
            {
                var command = _pending[index];
                if (command.ExecuteAtTick > tick)
                {
                    break;
                }

                if (command.ExecuteAtTick < tick)
                {
                    throw new InvalidOperationException(
                        $"Pending command sequence {command.Sequence} missed its tick {command.ExecuteAtTick} at tick {tick}.");
                }

                _applied.Add(new AppliedCommandRecord(
                    command.Sequence,
                    command.ExecuteAtTick,
                    command.GetType().FullName ?? command.GetType().Name));
                _pending.RemoveAt(index);
            }
        }

        private static int CompareCommands(IBattleCommand left, IBattleCommand right)
        {
            var tickCompare = left.ExecuteAtTick.CompareTo(right.ExecuteAtTick);
            if (tickCompare != 0)
            {
                return tickCompare;
            }

            return left.Sequence.CompareTo(right.Sequence);
        }

        private void RefreshChecksum()
        {
            Checksum = CanonicalChecksum.Compute(Seed, Tick, Phase, _applied);
        }
    }
}
