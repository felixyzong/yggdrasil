using System;

namespace Core.Character.Stat
{
    public enum DerivedStatType
    {
        CurrentHealth,
        CurrentMana
    }
    
    public class DerivedStat
    {
        private readonly BaseStat baseStat;
        public int Value { get; private set; }

        public event Action<int> ValueChanged;

        public DerivedStat(BaseStat baseStat)
        {
            if (baseStat == null) throw new ArgumentNullException(nameof(baseStat));
            this.baseStat = baseStat;
            Value = baseStat.Value;
            this.baseStat.ValueChanged += OnBaseStatValueChanged;
        }

        private void OnBaseStatValueChanged(int newBaseValue)
        {
            // If the base stat decreases and the current value exceeds the new base value, adjust it down to the new base value.
            if (Value > newBaseValue)
            {
                SetValue(newBaseValue);
            }
        }

        public void SetValue(int newValue)
        {
            if (newValue < 0) throw new ArgumentOutOfRangeException(nameof(newValue));
            if (Value == Math.Min(newValue, baseStat.Value)) return;
            Value = Math.Min(newValue, baseStat.Value);
            ValueChanged?.Invoke(Value);
        }

        public void Increase(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            SetValue(Math.Min(baseStat.Value, Value + amount));
        }

        public void Decrease(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            SetValue(Math.Max(0, Value - amount));
        }

        public void Reset()
        {
            SetValue(baseStat.Value);
        }
    }
}
