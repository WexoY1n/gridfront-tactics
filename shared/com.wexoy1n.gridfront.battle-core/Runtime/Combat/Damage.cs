using System;

namespace Gridfront.BattleCore.Combat
{
    public static class Damage
    {
        public static int Physical(int attack, int defense, int minDamage)
        {
            if (minDamage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minDamage), minDamage, "Minimum damage must be non-negative.");
            }

            var raw = attack - defense;
            return raw > minDamage ? raw : minDamage;
        }
    }
}
