using System;
using System.Collections.Generic;
using System.Linq;
using Core.Battle;
using Core.Battle.Commands;
using Core.Battle.Events;
using Core.Character.Model;
using Core.Grid;

namespace MVVM.ViewModels.Battle
{
    public class BattleViewModel : ViewModelBase
    {
        private readonly BattleLoopSystem loopSystem;
        private readonly BattleCommandService commandService;
        private readonly List<GridCellViewModel> gridCells = new List<GridCellViewModel>();
        private readonly List<BattleUnitViewModel> units = new List<BattleUnitViewModel>();

        private BattleState state;
        private GridPosition? selectedCell;
        private string selectedUnitId;
        private BattleInputMode inputMode;
        private string lastError;
        private bool autoRunEnemyTurns = true;

        public BattleState State => state;
        public IReadOnlyList<GridCellViewModel> GridCells => gridCells;
        public IReadOnlyList<BattleUnitViewModel> Units => units;
        public BattleCommandBarViewModel CommandBar { get; }
        public CharacterPanelViewModel CharacterPanel { get; }
        public BattleLogViewModel BattleLog { get; }
        public TurnPreviewViewModel TurnPreview { get; }

        public BattlePhase Phase => state?.Phase ?? BattlePhase.Init;
        public BattleResult Result => state?.Result ?? BattleResult.None;
        public string CurrentActorId => state?.CurrentActorId;
        public bool HasCurrentActor => !string.IsNullOrEmpty(CurrentActorId);
        public bool IsPlayerTurn => GetCurrentActor()?.Team == TeamType.Player;

        public GridPosition? SelectedCell
        {
            get => selectedCell;
            private set => SetProperty(ref selectedCell, value);
        }

        public string SelectedUnitId
        {
            get => selectedUnitId;
            private set => SetProperty(ref selectedUnitId, value);
        }

        public BattleInputMode InputMode
        {
            get => inputMode;
            private set => SetProperty(ref inputMode, value);
        }

        public string LastError
        {
            get => lastError;
            private set => SetProperty(ref lastError, value);
        }

        public bool AutoRunEnemyTurns
        {
            get => autoRunEnemyTurns;
            set => SetProperty(ref autoRunEnemyTurns, value);
        }

        public BattleViewModel()
            : this(new BattleLoopSystem(), new BattleCommandService())
        {
        }

        public BattleViewModel(BattleLoopSystem loopSystem, BattleCommandService commandService)
        {
            if (loopSystem == null) throw new ArgumentNullException(nameof(loopSystem));
            if (commandService == null) throw new ArgumentNullException(nameof(commandService));

            this.loopSystem = loopSystem;
            this.commandService = commandService;

            CommandBar = new BattleCommandBarViewModel();
            CharacterPanel = new CharacterPanelViewModel();
            BattleLog = new BattleLogViewModel();
            TurnPreview = new TurnPreviewViewModel();
        }

        public void StartSampleBattle()
        {
            SetBattleState(BattleVerticalSliceScenario.CreateSampleBattle());
            StartBattle();
        }

        public void SetBattleState(BattleState battleState)
        {
            if (battleState == null) throw new ArgumentNullException(nameof(battleState));

            state = battleState;
            SelectedCell = null;
            SelectedUnitId = null;
            InputMode = BattleInputMode.None;
            LastError = string.Empty;

            BuildGridCells();
            BuildUnits();
            BattleLog.Clear();
            RefreshAll();

            OnPropertyChanged(nameof(State));
            OnPropertyChanged(nameof(GridCells));
            OnPropertyChanged(nameof(Units));
        }

        public void StartBattle()
        {
            if (state == null) return;

            LastError = string.Empty;
            var events = loopSystem.StartBattle(state);
            BattleLog.AddEvents(state, events);
            RefreshAll();
            RunEnemyTurnsIfNeeded();
        }

        public void SelectCell(GridPosition position)
        {
            if (state == null || !state.Grid.IsInside(position)) return;

            var occupant = state.GetCharacterAt(position);
            SelectedCell = position;
            SelectedUnitId = occupant?.InstanceId;

            if (InputMode == BattleInputMode.Move)
            {
                TryMove(position);
                return;
            }

            if (InputMode == BattleInputMode.Attack && occupant != null)
            {
                TryBasicAttack(occupant.InstanceId);
                return;
            }

            RefreshAll();
        }

        public void SelectUnit(string unitId)
        {
            if (state == null) return;

            var unit = state.GetCharacter(unitId);
            if (unit == null) return;

            SelectedUnitId = unit.InstanceId;
            SelectedCell = unit.Position;

            if (InputMode == BattleInputMode.Attack)
            {
                TryBasicAttack(unit.InstanceId);
                return;
            }

            RefreshAll();
        }

        public void ClearSelection()
        {
            SelectedCell = null;
            SelectedUnitId = null;
            InputMode = BattleInputMode.None;
            LastError = string.Empty;
            RefreshAll();
        }

        public void EnterMoveMode()
        {
            CommandBar.Refresh(state);
            if (!CommandBar.CanMove)
            {
                SetCommandError("Move is not available.");
                return;
            }

            InputMode = BattleInputMode.Move;
            LastError = string.Empty;
            RefreshAll();
        }

        public void EnterAttackMode()
        {
            CommandBar.Refresh(state);
            if (!CommandBar.CanBasicAttack)
            {
                SetCommandError("Basic attack is not available.");
                return;
            }

            InputMode = BattleInputMode.Attack;
            LastError = string.Empty;
            RefreshAll();
        }

        public bool TryMove(GridPosition targetPosition)
        {
            var actor = GetCurrentActor();
            if (actor == null) return false;

            bool success = ExecuteCommand(new MoveCommand(actor.InstanceId, targetPosition), false);
            if (success)
            {
                SelectedUnitId = actor.InstanceId;
                SelectedCell = actor.Position;
                InputMode = BattleInputMode.None;
            }

            RefreshAll();
            return success;
        }

        public bool TryBasicAttack(string targetId)
        {
            var actor = GetCurrentActor();
            if (actor == null) return false;

            bool success = ExecuteCommand(new BasicAttackCommand(actor.InstanceId, targetId), false);
            if (success)
            {
                SelectedUnitId = targetId;
                var target = state.GetCharacter(targetId);
                SelectedCell = target?.Position;
                InputMode = BattleInputMode.None;
            }

            RefreshAll();
            return success;
        }

        public bool EndTurn()
        {
            var actor = GetCurrentActor();
            if (actor == null) return false;

            bool success = ExecuteCommand(new EndTurnCommand(actor.InstanceId), false);
            if (!success)
            {
                RefreshAll();
                return false;
            }

            InputMode = BattleInputMode.None;
            SelectedCell = null;
            SelectedUnitId = null;
            AdvanceToNextTurnIfNeeded();
            RefreshAll();
            RunEnemyTurnsIfNeeded();
            return true;
        }

        private void AdvanceToNextTurnIfNeeded()
        {
            if (state == null || state.Result != BattleResult.None) return;

            var events = loopSystem.AdvanceToNextTurn(state);
            BattleLog.AddEvents(state, events);
        }

        private void RunEnemyTurnsIfNeeded()
        {
            if (!AutoRunEnemyTurns || state == null) return;

            int safety = 10;
            while (state.Result == BattleResult.None && GetCurrentActor()?.Team == TeamType.Enemy)
            {
                if (safety-- <= 0)
                {
                    SetCommandError("Enemy turn safety limit reached.");
                    break;
                }

                RunEnemyTurn();
            }

            RefreshAll();
        }

        private void RunEnemyTurn()
        {
            var actor = GetCurrentActor();
            if (actor == null || actor.Team != TeamType.Enemy) return;

            var target = state.GetAliveCharacters(TeamType.Player)
                .OrderBy(player => GridDistance.Manhattan(actor.Position, player.Position))
                .ThenBy(player => player.InstanceId, StringComparer.Ordinal)
                .FirstOrDefault();

            if (target != null)
            {
                if (GridDistance.Manhattan(actor.Position, target.Position) > actor.AttackRange)
                {
                    TryEnemyMoveToward(actor, target);
                }

                if (!target.IsDead
                    && !actor.TurnState.HasUsedBasicAttack
                    && GridDistance.Manhattan(actor.Position, target.Position) <= actor.AttackRange)
                {
                    ExecuteCommand(new BasicAttackCommand(actor.InstanceId, target.InstanceId), false);
                }
            }

            ExecuteCommand(new EndTurnCommand(actor.InstanceId), false);
            AdvanceToNextTurnIfNeeded();
        }

        private void TryEnemyMoveToward(CharacterModel actor, CharacterModel target)
        {
            GridPosition bestPosition = actor.Position;
            int bestTargetDistance = GridDistance.Manhattan(actor.Position, target.Position);
            int bestMoveCost = 0;

            foreach (var position in EnumerateGridPositions())
            {
                if (position == actor.Position) continue;
                if (!state.IsCellAvailable(position)) continue;

                int moveCost = GridDistance.Manhattan(actor.Position, position);
                if (moveCost > actor.TurnState.RemainingMove) continue;

                int targetDistance = GridDistance.Manhattan(position, target.Position);
                if (targetDistance < bestTargetDistance
                    || targetDistance == bestTargetDistance && moveCost < bestMoveCost)
                {
                    bestPosition = position;
                    bestTargetDistance = targetDistance;
                    bestMoveCost = moveCost;
                }
            }

            if (bestPosition != actor.Position)
            {
                ExecuteCommand(new MoveCommand(actor.InstanceId, bestPosition), false);
            }
        }

        private IEnumerable<GridPosition> EnumerateGridPositions()
        {
            for (int x = 0; x < state.Grid.Width; x++)
            {
                for (int y = 0; y < state.Grid.Height; y++)
                {
                    yield return new GridPosition(x, y);
                }
            }
        }

        private bool ExecuteCommand(IBattleCommand command, bool refreshAfter)
        {
            if (state == null) return false;

            var result = commandService.TryExecute(state, command);
            if (!result.IsSuccess)
            {
                SetCommandError(result.ErrorMessage);
                if (refreshAfter) RefreshAll();
                return false;
            }

            LastError = string.Empty;
            BattleLog.AddEvents(state, result.Events);

            if (refreshAfter)
            {
                RefreshAll();
            }

            return true;
        }

        private void SetCommandError(string message)
        {
            LastError = message ?? string.Empty;
            BattleLog.AddMessage($"Command failed: {LastError}");
        }

        private CharacterModel GetCurrentActor()
        {
            return state?.GetCharacter(state.CurrentActorId);
        }

        private void BuildGridCells()
        {
            gridCells.Clear();
            if (state == null) return;

            for (int y = 0; y < state.Grid.Height; y++)
            {
                for (int x = 0; x < state.Grid.Width; x++)
                {
                    gridCells.Add(new GridCellViewModel(new GridPosition(x, y)));
                }
            }
        }

        private void BuildUnits()
        {
            units.Clear();
            if (state == null) return;

            foreach (var character in state.Characters)
            {
                units.Add(new BattleUnitViewModel(character));
            }
        }

        private void RefreshAll()
        {
            CommandBar.Refresh(state);
            CharacterPanel.Refresh(state, SelectedUnitId);
            TurnPreview.Refresh(state);

            foreach (var cell in gridCells)
            {
                cell.Refresh(state, SelectedCell, InputMode);
            }

            foreach (var unit in units)
            {
                unit.Refresh(state, SelectedUnitId, InputMode);
            }

            OnPropertyChanged(nameof(Phase));
            OnPropertyChanged(nameof(Result));
            OnPropertyChanged(nameof(CurrentActorId));
            OnPropertyChanged(nameof(HasCurrentActor));
            OnPropertyChanged(nameof(IsPlayerTurn));
        }
    }
}
