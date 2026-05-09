using System;
using System.Collections.Generic;
using System.Linq;
using Core.Battle.Events;
using Core.Character.Model;

namespace Core.Battle.Commands
{
    public class EndTurnCommand : IBattleCommand
    {
        public string ActorId { get; }

        public EndTurnCommand(string actorId)
        {
            ActorId = actorId;
        }

        public CommandResult Validate(BattleState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (ActorId != state.CurrentActorId) return CommandResult.Failure("Actor is not the current actor.");

            var actor = state.GetCharacter(ActorId);
            if (actor == null) return CommandResult.Failure("Actor does not exist.");
            if (actor.IsDead) return CommandResult.Failure("Actor is dead.");
            if (state.Phase != BattlePhase.WaitingForCommand) return CommandResult.Failure("Battle is not waiting for a command.");

            return CommandResult.Success();
        }

        public CommandResult Execute(BattleState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            var actor = state.GetCharacter(ActorId);
            var events = new List<IBattleEvent>
            {
                new TurnEndedEvent(actor.InstanceId, state.TurnIndex)
            };

            actor.ResetActionGauge();
            state.ClearCurrentActor();
            state.IncrementTurnIndex();

            BattleResult result = EvaluateBattleResult(state);
            if (result != BattleResult.None)
            {
                state.SetResult(result);
                events.Add(new BattleEndedEvent(result));
            }
            else
            {
                state.SetPhase(BattlePhase.WaitingForGauge);
            }

            return CommandResult.Success(events);
        }

        private static BattleResult EvaluateBattleResult(BattleState state)
        {
            bool playerAlive = state.GetAliveCharacters(TeamType.Player).Any();
            bool enemyAlive = state.GetAliveCharacters(TeamType.Enemy).Any();

            if (!playerAlive) return BattleResult.EnemyWin;
            if (!enemyAlive) return BattleResult.PlayerWin;
            return BattleResult.None;
        }
    }
}
