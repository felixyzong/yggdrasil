using System;
using Core.Character.Stat;
using Core.Grid;

namespace Core.Character.Model
{
    public class CharacterModel
    {
        public string InstanceId { get; }
        public string Name { get; private set; }
        public TeamType Team { get; private set; }
        public GridPosition Position { get; private set; }
        public CharacterStats Stats { get; }
        
        public int AttackRange { get; private set; }
        public int ActionGauge { get; private set; }
        public bool IsDead { get; private set; }
        public TurnActionState TurnState { get; }
        
        public int MaxHealth => Stats.BaseStats[BaseStatType.MaxHealth].Value;
        public int MaxMana => Stats.BaseStats[BaseStatType.MaxMana].Value;
        public int Strength => Stats.BaseStats[BaseStatType.Strength].Value;
        public int Speed => Stats.BaseStats[BaseStatType.Speed].Value;
        public int Intelligence => Stats.BaseStats[BaseStatType.Intelligence].Value;
        public int Luck => Stats.BaseStats[BaseStatType.Luck].Value;
        public int MovementRange => Stats.BaseStats[BaseStatType.MovementRange].Value;
        public int CurrentHealth => Stats.DerivedStats[DerivedStatType.CurrentHealth].Value;
        public int CurrentMana => Stats.DerivedStats[DerivedStatType.CurrentMana].Value;
        public int ManaRestore => (int)(MaxMana * 0.4);

        public CharacterModel(
            string name,
            int maxHealth,
            int maxMana,
            int strength,
            int speed,
            int intelligence,
            int luck,
            int movementRange,
            int attackRange)
            : this(
                Guid.NewGuid().ToString("N"),
                name,
                TeamType.Player,
                new GridPosition(0, 0),
                maxHealth,
                maxMana,
                strength,
                speed,
                intelligence,
                luck,
                movementRange,
                attackRange)
        {
        }

        public CharacterModel(
            string instanceId,
            string name,
            TeamType team,
            GridPosition position,
            int maxHealth,
            int maxMana,
            int strength,
            int speed,
            int intelligence,
            int luck,
            int movementRange,
            int attackRange)
        {
            if (string.IsNullOrWhiteSpace(instanceId)) throw new ArgumentException("Instance id cannot be null or whitespace.", nameof(instanceId));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name cannot be null or whitespace.", nameof(name));
            InstanceId = instanceId;
            Name = name;
            Team = team;
            Position = position;
            Stats = new CharacterStats(maxHealth, maxMana, strength, speed, intelligence, luck, movementRange);
            if (attackRange < 1) throw new ArgumentOutOfRangeException(nameof(attackRange), "Attack range must be at least 1.");
            AttackRange = attackRange;
            TurnState = new TurnActionState();
        }

        public void SetPosition(GridPosition position)
        {
            Position = position;
        }

        public void AddActionGauge(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (IsDead) return;
            ActionGauge += amount;
        }

        public void ResetActionGauge()
        {
            ActionGauge = 0;
        }

        public void TakeDamage(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            DecreaseStat(DerivedStatType.CurrentHealth, amount);
        }

        public void MarkDead()
        {
            IsDead = true;
            SetStat(DerivedStatType.CurrentHealth, 0);
            ResetActionGauge();
        }

        public void SetStat(BaseStatType statType, int newValue)
        {
            if (!Stats.BaseStats.ContainsKey(statType)) throw new ArgumentException($"Invalid stat type: {statType}", nameof(statType));
            Stats.BaseStats[statType].SetValue(newValue);
        }

        public void SetStat(DerivedStatType statType, int newValue)
        {
            if (!Stats.DerivedStats.ContainsKey(statType)) throw new ArgumentException($"Invalid stat type: {statType}", nameof(statType));
            Stats.DerivedStats[statType].SetValue(newValue);
        }

        public void IncreaseStat(BaseStatType statType, int amount)
        {
            if (!Stats.BaseStats.ContainsKey(statType)) throw new ArgumentException($"Invalid stat type: {statType}", nameof(statType));
            Stats.BaseStats[statType].Increase(amount);
        }

        public void DecreaseStat(BaseStatType statType, int amount)
        {
            if (!Stats.BaseStats.ContainsKey(statType)) throw new ArgumentException($"Invalid stat type: {statType}", nameof(statType));
            Stats.BaseStats[statType].Decrease(amount);
        }

        public void IncreaseStat(DerivedStatType statType, int amount)
        {
            if (!Stats.DerivedStats.ContainsKey(statType)) throw new ArgumentException($"Invalid stat type: {statType}", nameof(statType));
            Stats.DerivedStats[statType].Increase(amount);
        }

        public void DecreaseStat(DerivedStatType statType, int amount)
        {
            if (!Stats.DerivedStats.ContainsKey(statType)) throw new ArgumentException($"Invalid stat type: {statType}", nameof(statType));
            Stats.DerivedStats[statType].Decrease(amount);
        }

        public void RestoreMana()
        {
            int manaToRestore = ManaRestore;
            IncreaseStat(DerivedStatType.CurrentMana, manaToRestore);
        }
    }
}
