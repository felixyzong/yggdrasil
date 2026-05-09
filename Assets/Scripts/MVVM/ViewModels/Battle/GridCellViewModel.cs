using Core.Battle;
using Core.Grid;

namespace MVVM.ViewModels.Battle
{
    public class GridCellViewModel : ViewModelBase
    {
        private bool isWalkable;
        private bool isOccupied;
        private string occupantId;
        private bool isSelected;
        private bool isMoveCandidate;
        private bool isAttackCandidate;
        private bool isBlocked;
        private GridCellHighlightType highlightType;

        public GridPosition Position { get; }
        public int X => Position.X;
        public int Y => Position.Y;

        public bool IsWalkable
        {
            get => isWalkable;
            private set => SetProperty(ref isWalkable, value);
        }

        public bool IsOccupied
        {
            get => isOccupied;
            private set => SetProperty(ref isOccupied, value);
        }

        public string OccupantId
        {
            get => occupantId;
            private set => SetProperty(ref occupantId, value);
        }

        public bool IsSelected
        {
            get => isSelected;
            private set => SetProperty(ref isSelected, value);
        }

        public bool IsMoveCandidate
        {
            get => isMoveCandidate;
            private set => SetProperty(ref isMoveCandidate, value);
        }

        public bool IsAttackCandidate
        {
            get => isAttackCandidate;
            private set => SetProperty(ref isAttackCandidate, value);
        }

        public bool IsBlocked
        {
            get => isBlocked;
            private set => SetProperty(ref isBlocked, value);
        }

        public GridCellHighlightType HighlightType
        {
            get => highlightType;
            private set => SetProperty(ref highlightType, value);
        }

        public GridCellViewModel(GridPosition position)
        {
            Position = position;
        }

        public void Refresh(BattleState state, GridPosition? selectedCell, BattleInputMode inputMode)
        {
            if (state == null) return;

            var occupant = state.GetCharacterAt(Position);
            var actor = state.GetCharacter(state.CurrentActorId);

            bool walkable = state.Grid.IsWalkable(Position);
            bool occupied = occupant != null;
            bool selected = selectedCell.HasValue && selectedCell.Value == Position;
            bool moveCandidate = false;
            bool attackCandidate = false;

            if (actor != null && state.Phase == BattlePhase.WaitingForCommand && !actor.IsDead)
            {
                int distance = GridDistance.Manhattan(actor.Position, Position);
                moveCandidate = inputMode == BattleInputMode.Move
                    && walkable
                    && !occupied
                    && distance <= actor.TurnState.RemainingMove;

                attackCandidate = inputMode == BattleInputMode.Attack
                    && occupied
                    && occupant.InstanceId != actor.InstanceId
                    && occupant.Team != actor.Team
                    && !occupant.IsDead
                    && !actor.TurnState.HasUsedBasicAttack
                    && distance <= actor.AttackRange;
            }

            IsWalkable = walkable;
            IsOccupied = occupied;
            OccupantId = occupant?.InstanceId;
            IsSelected = selected;
            IsMoveCandidate = moveCandidate;
            IsAttackCandidate = attackCandidate;
            IsBlocked = !walkable || (occupied && occupant?.InstanceId != state.CurrentActorId);
            HighlightType = GetHighlightType();
        }

        private GridCellHighlightType GetHighlightType()
        {
            if (IsSelected) return GridCellHighlightType.Selected;
            if (IsAttackCandidate) return GridCellHighlightType.AttackCandidate;
            if (IsMoveCandidate) return GridCellHighlightType.MoveCandidate;
            if (IsBlocked) return GridCellHighlightType.Blocked;
            if (IsOccupied) return GridCellHighlightType.Occupied;
            return GridCellHighlightType.None;
        }
    }
}
