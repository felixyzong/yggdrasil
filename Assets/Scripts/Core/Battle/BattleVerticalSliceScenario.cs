using System;
using System.Collections.Generic;
using System.Linq;
using Core.Battle.Commands;
using Core.Battle.Events;
using Core.Character.Model;
using Core.Grid;

namespace Core.Battle
{
    public class BattleVerticalSliceReport
    {
        public BattleState State { get; }
        public IReadOnlyList<IBattleEvent> Events { get; }

        public BattleVerticalSliceReport(BattleState state, IReadOnlyList<IBattleEvent> events)
        {
            State = state;
            Events = events;
        }
    }

    public static class BattleVerticalSliceScenario
    {
        public static BattleState CreateSampleBattle()
        {
            var grid = new GridMap(5, 5);
            var hero = new CharacterModel(
                "hero",
                "Hero",
                TeamType.Player,
                new GridPosition(0, 0),
                maxHealth: 30,
                maxMana: 0,
                strength: 10,
                speed: 10,
                intelligence: 0,
                luck: 0,
                movementRange: 3,
                attackRange: 1);

            var slime = new CharacterModel(
                "slime",
                "Slime",
                TeamType.Enemy,
                new GridPosition(2, 0),
                maxHealth: 20,
                maxMana: 0,
                strength: 5,
                speed: 5,
                intelligence: 0,
                luck: 0,
                movementRange: 2,
                attackRange: 1);

            return new BattleState(grid, new[] { hero, slime });
        }

        public static BattleVerticalSliceReport RunSampleBattle()
        {
            var state = CreateSampleBattle();
            var loopSystem = new BattleLoopSystem();
            var commandService = new BattleCommandService();
            var events = new List<IBattleEvent>();

            events.AddRange(loopSystem.StartBattle(state));

            int safety = 20;
            while (state.Result == BattleResult.None)
            {
                if (safety-- <= 0)
                {
                    throw new InvalidOperationException("Sample battle did not finish within the safety limit.");
                }

                var actor = state.GetCharacter(state.CurrentActorId);
                if (actor == null)
                {
                    throw new InvalidOperationException("No current actor is available.");
                }

                if (actor.Team == TeamType.Player)
                {
                    RunPlayerSampleAction(state, commandService, events, actor);
                }
                else if (actor.Team == TeamType.Enemy)
                {
                    RunEnemySampleAction(state, commandService, events, actor);
                }

                ExecuteOrThrow(commandService, state, new EndTurnCommand(actor.InstanceId), events);

                if (state.Result == BattleResult.None)
                {
                    events.AddRange(loopSystem.AdvanceToNextTurn(state));
                }
            }

            return new BattleVerticalSliceReport(state, events.AsReadOnly());
        }

        private static void RunPlayerSampleAction(
            BattleState state,
            BattleCommandService commandService,
            List<IBattleEvent> events,
            CharacterModel actor)
        {
            var target = state.GetAliveCharacters(TeamType.Enemy).FirstOrDefault();
            if (target == null) return;

            var preferredPosition = new GridPosition(1, 0);
            if (actor.Position != preferredPosition && state.IsCellAvailable(preferredPosition))
            {
                ExecuteOrThrow(commandService, state, new MoveCommand(actor.InstanceId, preferredPosition), events);
            }

            if (GridDistance.Manhattan(actor.Position, target.Position) <= actor.AttackRange)
            {
                ExecuteOrThrow(commandService, state, new BasicAttackCommand(actor.InstanceId, target.InstanceId), events);
            }
        }

        private static void RunEnemySampleAction(
            BattleState state,
            BattleCommandService commandService,
            List<IBattleEvent> events,
            CharacterModel actor)
        {
            var target = state.GetAliveCharacters(TeamType.Player).FirstOrDefault();
            if (target == null) return;

            if (GridDistance.Manhattan(actor.Position, target.Position) <= actor.AttackRange)
            {
                ExecuteOrThrow(commandService, state, new BasicAttackCommand(actor.InstanceId, target.InstanceId), events);
            }
        }

        private static void ExecuteOrThrow(
            BattleCommandService commandService,
            BattleState state,
            IBattleCommand command,
            List<IBattleEvent> events)
        {
            var result = commandService.TryExecute(state, command);
            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.ErrorMessage);
            }

            events.AddRange(result.Events);
        }
    }
}
