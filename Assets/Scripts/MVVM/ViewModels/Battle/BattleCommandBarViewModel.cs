using System.Linq;
using Core.Battle;
using Core.Character.Model;
using Core.Grid;

namespace MVVM.ViewModels.Battle
{
    public class BattleCommandBarViewModel : ViewModelBase
    {
        private bool canMove;
        private bool canBasicAttack;
        private bool canEndTurn;
        private int remainingMove;
        private bool hasUsedBasicAttack;

        public bool CanMove
        {
            get => canMove;
            private set => SetProperty(ref canMove, value);
        }

        public bool CanBasicAttack
        {
            get => canBasicAttack;
            private set => SetProperty(ref canBasicAttack, value);
        }

        public bool CanEndTurn
        {
            get => canEndTurn;
            private set => SetProperty(ref canEndTurn, value);
        }

        public int RemainingMove
        {
            get => remainingMove;
            private set
            {
                if (SetProperty(ref remainingMove, value))
                {
                    OnPropertyChanged(nameof(MoveButtonText));
                }
            }
        }

        public bool HasUsedBasicAttack
        {
            get => hasUsedBasicAttack;
            private set
            {
                if (SetProperty(ref hasUsedBasicAttack, value))
                {
                    OnPropertyChanged(nameof(AttackButtonText));
                }
            }
        }

        public string MoveButtonText => $"Move ({RemainingMove})";
        public string AttackButtonText => HasUsedBasicAttack ? "Attack Used" : "Attack";
        public string EndTurnButtonText => "End Turn";

        public void Refresh(BattleState state)
        {
            var actor = state?.GetCharacter(state.CurrentActorId);
            bool playerCanAct = actor != null
                && actor.Team == TeamType.Player
                && !actor.IsDead
                && state.Phase == BattlePhase.WaitingForCommand;

            RemainingMove = actor?.TurnState.RemainingMove ?? 0;
            HasUsedBasicAttack = actor?.TurnState.HasUsedBasicAttack ?? false;
            CanMove = playerCanAct && RemainingMove > 0;
            CanBasicAttack = playerCanAct && !HasUsedBasicAttack && HasTargetInAttackRange(state, actor);
            CanEndTurn = playerCanAct;
        }

        private static bool HasTargetInAttackRange(BattleState state, CharacterModel actor)
        {
            if (state == null || actor == null) return false;

            return state.GetAliveCharacters()
                .Any(target => target.Team != actor.Team
                    && GridDistance.Manhattan(actor.Position, target.Position) <= actor.AttackRange);
        }
    }
}
