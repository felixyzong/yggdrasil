using System;

namespace Core.Character.Model
{
    public class TurnActionState
    {
        public int RemainingMove { get; private set; }
        public bool HasUsedBasicAttack { get; private set; }

        public void Reset(CharacterModel character)
        {
            if (character == null) throw new ArgumentNullException(nameof(character));
            RemainingMove = character.MovementRange;
            HasUsedBasicAttack = false;
        }

        public void ConsumeMove(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (amount > RemainingMove) throw new InvalidOperationException("Cannot consume more move than remains.");
            RemainingMove -= amount;
        }

        public void MarkBasicAttackUsed()
        {
            HasUsedBasicAttack = true;
        }
    }
}
