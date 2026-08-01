using Gridfront.BattleCore.Domain;
using Xunit;

namespace Gridfront.BattleCore.Tests
{
    public sealed class ChecksumDeterminismTests
    {
        [Fact]
        public void IdenticalSeed_EmptyCommands_1000Ticks_ProduceSameChecksum()
        {
            const ulong seed = 92133741UL;

            var first = BattleRunner.Create(seed);
            var second = BattleRunner.Create(seed);

            first.Step(1000);
            second.Step(1000);

            Assert.Equal(1000, first.Tick);
            Assert.Equal(1000, second.Tick);
            Assert.Equal(first.Checksum, second.Checksum);
            Assert.False(string.IsNullOrWhiteSpace(first.Checksum));
        }

        [Fact]
        public void DifferentSeeds_ProduceDifferentChecksums()
        {
            var first = BattleRunner.Create(1UL);
            var second = BattleRunner.Create(2UL);

            first.Step(1000);
            second.Step(1000);

            Assert.NotEqual(first.Checksum, second.Checksum);
        }

        [Fact]
        public void ChangingOneCommand_DivergesChecksum()
        {
            const ulong seed = 42UL;

            var baseline = BattleRunner.Create(seed);
            baseline.Enqueue(new ProbeCommand(executeAtTick: 10, sequence: 1));
            baseline.Step(1000);

            var mutated = BattleRunner.Create(seed);
            mutated.Enqueue(new ProbeCommand(executeAtTick: 11, sequence: 1));
            mutated.Step(1000);

            Assert.NotEqual(baseline.Checksum, mutated.Checksum);
        }

        [Fact]
        public void EnqueueAfterEnd_Throws()
        {
            var runner = BattleRunner.Create(7UL);
            runner.EndBattle();

            Assert.Throws<InvalidOperationException>(() =>
                runner.Enqueue(new ProbeCommand(executeAtTick: 0, sequence: 1)));
        }

        private sealed class ProbeCommand : IBattleCommand
        {
            public ProbeCommand(int executeAtTick, long sequence)
            {
                ExecuteAtTick = executeAtTick;
                Sequence = sequence;
            }

            public int ExecuteAtTick { get; }

            public long Sequence { get; }
        }
    }
}
