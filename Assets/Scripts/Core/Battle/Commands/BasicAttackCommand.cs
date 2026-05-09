using System;
using System.Collections.Generic;
using Core.Battle.Events;
using Core.Grid;

namespace Core.Battle.Commands
{
    public class BasicAttackCommand : IBattleCommand
    {
        public string AttackerId { get; }
        public string TargetId { get; }

        public BasicAttackCommand(string attackerId, string targetId)
        {
            AttackerId = attackerId;
            TargetId = targetId;
        }

        public CommandResult Validate(BattleState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Phase != BattlePhase.WaitingForCommand) return CommandResult.Failure("Battle is not waiting for a command.");
            if (AttackerId != state.CurrentActorId) return CommandResult.Failure("Attacker is not the current actor.");

            var attacker = state.GetCharacter(AttackerId);
            var target = state.GetCharacter(TargetId);

            if (attacker == null) return CommandResult.Failure("Attacker does not exist.");
            if (target == null) return CommandResult.Failure("Target does not exist.");
            if (attacker.IsDead) return CommandResult.Failure("Attacker is dead.");
            if (target.IsDead) return CommandResult.Failure("Target is dead.");
            if (attacker.Team == target.Team) return CommandResult.Failure("Cannot attack a character on the same team.");
            if (attacker.TurnState.HasUsedBasicAttack) return CommandResult.Failure("Basic attack has already been used this turn.");

            int distance = GridDistance.Manhattan(attacker.Position, target.Position);
            if (distance > attacker.AttackRange) return CommandResult.Failure("Target is outside attack range.");

            return CommandResult.Success();
        }

        public CommandResult Execute(BattleState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            var attacker = state.GetCharacter(AttackerId);
            var target = state.GetCharacter(TargetId);
            int damage = Math.Max(1, attacker.Strength);

            target.TakeDamage(damage);
            attacker.TurnState.MarkBasicAttackUsed();

            var events = new List<IBattleEvent>
            {
                new CharacterDamagedEvent(attacker.InstanceId, target.InstanceId, damage)
            };

            if (target.CurrentHealth <= 0)
            {
                target.MarkDead();
                events.Add(new CharacterDiedEvent(target.InstanceId));
            }

            return CommandResult.Success(events);
        }
    }
}
