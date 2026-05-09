using System;
using Core.Battle.Events;
using Core.Grid;

namespace Core.Battle.Commands
{
    public class MoveCommand : IBattleCommand
    {
        public string ActorId { get; }
        public GridPosition TargetPosition { get; }

        public MoveCommand(string actorId, GridPosition targetPosition)
        {
            ActorId = actorId;
            TargetPosition = targetPosition;
        }

        public CommandResult Validate(BattleState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Phase != BattlePhase.WaitingForCommand) return CommandResult.Failure("Battle is not waiting for a command.");
            if (ActorId != state.CurrentActorId) return CommandResult.Failure("Actor is not the current actor.");

            var actor = state.GetCharacter(ActorId);
            if (actor == null) return CommandResult.Failure("Actor does not exist.");
            if (actor.IsDead) return CommandResult.Failure("Actor is dead.");
            if (!state.Grid.IsInside(TargetPosition)) return CommandResult.Failure("Target position is outside the grid.");
            if (!state.Grid.IsWalkable(TargetPosition)) return CommandResult.Failure("Target position is not walkable.");
            if (state.IsCellOccupied(TargetPosition)) return CommandResult.Failure("Target position is occupied.");

            int distance = GridDistance.Manhattan(actor.Position, TargetPosition);
            if (distance > actor.TurnState.RemainingMove) return CommandResult.Failure("Target position is beyond remaining move.");

            return CommandResult.Success();
        }

        public CommandResult Execute(BattleState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            var actor = state.GetCharacter(ActorId);
            var from = actor.Position;
            int moveCost = GridDistance.Manhattan(from, TargetPosition);

            actor.SetPosition(TargetPosition);
            actor.TurnState.ConsumeMove(moveCost);

            return CommandResult.Success(new CharacterMovedEvent(actor.InstanceId, from, TargetPosition));
        }
    }
}
