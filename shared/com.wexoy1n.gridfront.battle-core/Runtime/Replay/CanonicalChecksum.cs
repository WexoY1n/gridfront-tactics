using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Gridfront.BattleCore.Domain;

namespace Gridfront.BattleCore.Replay
{
    /// <summary>
    /// Builds a deterministic SHA-256 digest over canonical battle fields.
    /// </summary>
    public static class CanonicalChecksum
    {
        public static string Compute(
            ulong seed,
            int tick,
            BattlePhase phase,
            IReadOnlyList<AppliedCommandRecord> appliedCommands)
        {
            if (appliedCommands == null)
            {
                throw new ArgumentNullException(nameof(appliedCommands));
            }

            var builder = new StringBuilder(128 + (appliedCommands.Count * 32));
            builder.Append("seed=").Append(seed).Append('\n');
            builder.Append("tick=").Append(tick).Append('\n');
            builder.Append("phase=").Append((byte)phase).Append('\n');
            builder.Append("commands=").Append(appliedCommands.Count).Append('\n');

            for (var i = 0; i < appliedCommands.Count; i++)
            {
                var record = appliedCommands[i];
                builder
                    .Append(record.Sequence)
                    .Append('|')
                    .Append(record.ExecuteAtTick)
                    .Append('|')
                    .Append(record.TypeName)
                    .Append('\n');
            }

            var bytes = Encoding.UTF8.GetBytes(builder.ToString());
            using (var sha = SHA256.Create())
            {
                return ToHex(sha.ComputeHash(bytes));
            }
        }

        private static string ToHex(byte[] hash)
        {
            var chars = new char[hash.Length * 2];
            for (var i = 0; i < hash.Length; i++)
            {
                var value = hash[i];
                chars[i * 2] = GetHexNibble(value >> 4);
                chars[(i * 2) + 1] = GetHexNibble(value & 0xF);
            }

            return new string(chars);
        }

        private static char GetHexNibble(int value)
        {
            return (char)(value < 10 ? ('0' + value) : ('a' + (value - 10)));
        }
    }

    public readonly struct AppliedCommandRecord
    {
        public AppliedCommandRecord(long sequence, int executeAtTick, string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentException("Command type name is required.", nameof(typeName));
            }

            Sequence = sequence;
            ExecuteAtTick = executeAtTick;
            TypeName = typeName;
        }

        public long Sequence { get; }

        public int ExecuteAtTick { get; }

        public string TypeName { get; }
    }
}
