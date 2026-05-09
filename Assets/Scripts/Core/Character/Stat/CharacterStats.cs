using System;
using System.Collections.Generic;

namespace Core.Character.Stat
{
    public class CharacterStats
    {
        public Dictionary<BaseStatType, BaseStat> BaseStats { get; }
        public Dictionary<DerivedStatType, DerivedStat> DerivedStats { get; }

        public CharacterStats(
            int maxHealth,
            int maxMana,
            int strength,
            int speed,
            int intelligence,
            int luck,
            int movementRange)
        {
            BaseStats = new Dictionary<BaseStatType, BaseStat>
            {
                { BaseStatType.MaxHealth, new BaseStat(maxHealth) },
                { BaseStatType.MaxMana, new BaseStat(maxMana) },
                { BaseStatType.Strength, new BaseStat(strength) },
                { BaseStatType.Speed, new BaseStat(speed) },
                { BaseStatType.Intelligence, new BaseStat(intelligence) },
                { BaseStatType.Luck, new BaseStat(luck) },
                { BaseStatType.MovementRange, new BaseStat(movementRange) }
            };

            DerivedStats = new Dictionary<DerivedStatType, DerivedStat>
            {
                { DerivedStatType.CurrentHealth, new DerivedStat(BaseStats[BaseStatType.MaxHealth]) },
                { DerivedStatType.CurrentMana, new DerivedStat(BaseStats[BaseStatType.MaxMana]) }
            };
        }
    }
}