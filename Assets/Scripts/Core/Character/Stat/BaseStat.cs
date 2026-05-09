using System;

namespace Core.Character.Stat
{
    public enum BaseStatType
    {
        MaxHealth,
        MaxMana,
        Strength,
        Speed,
        Intelligence,
        Luck,
        MovementRange
    }

    public class BaseStat
    {
        public int Value { get; private set; }
        public event Action<int> ValueChanged;

        public BaseStat(int baseValue)
        {
            SetValue(baseValue);
        }

        public void SetValue(int newBaseValue)
        {
            if (newBaseValue < 0) throw new ArgumentOutOfRangeException(nameof(newBaseValue));
            if (Value == newBaseValue) return;
            Value = newBaseValue;
            ValueChanged?.Invoke(Value);
        }

        public void Increase(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            SetValue(Value + amount);
        }

        public void Decrease(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            SetValue(Math.Max(0, Value - amount));
        }
    }
}
