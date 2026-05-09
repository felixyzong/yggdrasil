using System;
using System.Collections.Generic;
using System.Linq;
using Core.Battle.Events;
using Core.Character.Model;

namespace Core.Battle
{
    public class BattleLoopSystem
    {
        private readonly ActionGaugeSystem actionGaugeSystem;

        public BattleLoopSystem()
            : this(new ActionGaugeSystem())
        {
        }

        public BattleLoopSystem(ActionGaugeSystem actionGaugeSystem)
        {
            if (actionGaugeSystem == null) throw new ArgumentNullException(nameof(actionGaugeSystem));
            this.actionGaugeSystem = actionGaugeSystem;
        }

        public IReadOnlyList<IBattleEvent> StartBattle(BattleState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            state.SetPhase(BattlePhase.WaitingForGauge);

            BattleResult result = CheckBattleResult(state);
            if (result != BattleResult.None)
            {
                return new List<IBattleEvent> { new BattleEndedEvent(result) }.AsReadOnly();
            }

            return AdvanceToNextTurn(state);
        }

        public IReadOnlyList<IBattleEvent> AdvanceToNextTurn(BattleState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Result != BattleResult.None) return Array.Empty<IBattleEvent>();

            BattleResult result = CheckBattleResult(state);
            if (result != BattleResult.None)
            {
                return new List<IBattleEvent> { new BattleEndedEvent(result) }.AsReadOnly();
            }

            state.SetPhase(BattlePhase.WaitingForGauge);
            actionGaugeSystem.AdvanceUntilReady(state);

            var actor = actionGaugeSystem.PickNextActor(state);
            if (actor == null)
            {
                throw new InvalidOperationException("No actor is ready after advancing action gauge.");
            }

            state.SetCurrentActor(actor.InstanceId);
            actor.TurnState.Reset(actor);
            state.SetPhase(BattlePhase.WaitingForCommand);

            return new List<IBattleEvent>
            {
                new TurnStartedEvent(actor.InstanceId, state.TurnIndex)
            }.AsReadOnly();
        }

        public BattleResult CheckBattleResult(BattleState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Result != BattleResult.None) return state.Result;

            bool playerAlive = state.GetAliveCharacters(TeamType.Player).Any();
            bool enemyAlive = state.GetAliveCharacters(TeamType.Enemy).Any();

            if (!playerAlive)
            {
                state.SetResult(BattleResult.EnemyWin);
                return BattleResult.EnemyWin;
            }

            if (!enemyAlive)
            {
                state.SetResult(BattleResult.PlayerWin);
                return BattleResult.PlayerWin;
            }

            return BattleResult.None;
        }
    }
}
