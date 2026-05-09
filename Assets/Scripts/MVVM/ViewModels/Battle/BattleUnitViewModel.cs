using Core.Battle;
using Core.Character.Model;
using Core.Grid;

namespace MVVM.ViewModels.Battle
{
    public class BattleUnitViewModel : ViewModelBase
    {
        private readonly CharacterModel model;
        private bool isCurrentActor;
        private bool isSelected;
        private bool canBeSelected;
        private bool canBeTargeted;

        public string InstanceId => model.InstanceId;
        public string Name => model.Name;
        public TeamType Team => model.Team;
        public string SpriteKey => model.InstanceId;
        public GridPosition Position => model.Position;
        public int X => model.Position.X;
        public int Y => model.Position.Y;
        public int CurrentHealth => model.CurrentHealth;
        public int MaxHealth => model.MaxHealth;
        public float HealthRatio => model.MaxHealth <= 0 ? 0f : model.CurrentHealth / (float)model.MaxHealth;
        public int ActionGauge => model.ActionGauge;
        public float ActionGaugeRatio => battleGaugeThreshold <= 0 ? 0f : model.ActionGauge / (float)battleGaugeThreshold;
        public bool IsDead => model.IsDead;
        public bool IsShadeVisible => !model.IsDead;

        private int battleGaugeThreshold = 1000;

        public bool IsCurrentActor
        {
            get => isCurrentActor;
            private set => SetProperty(ref isCurrentActor, value);
        }

        public bool IsSelected
        {
            get => isSelected;
            private set => SetProperty(ref isSelected, value);
        }

        public bool CanBeSelected
        {
            get => canBeSelected;
            private set => SetProperty(ref canBeSelected, value);
        }

        public bool CanBeTargeted
        {
            get => canBeTargeted;
            private set => SetProperty(ref canBeTargeted, value);
        }

        public BattleUnitViewModel(CharacterModel model)
        {
            this.model = model;
        }

        public void Refresh(BattleState state, string selectedUnitId, BattleInputMode inputMode)
        {
            if (state == null) return;

            battleGaugeThreshold = state.ActionGaugeThreshold;
            var actor = state.GetCharacter(state.CurrentActorId);

            IsCurrentActor = state.CurrentActorId == model.InstanceId;
            IsSelected = selectedUnitId == model.InstanceId;
            CanBeSelected = !model.IsDead;
            CanBeTargeted = actor != null
                && inputMode == BattleInputMode.Attack
                && actor.InstanceId != model.InstanceId
                && actor.Team != model.Team
                && !actor.TurnState.HasUsedBasicAttack
                && GridDistance.Manhattan(actor.Position, model.Position) <= actor.AttackRange
                && !model.IsDead;

            OnPropertyChanged(nameof(Position));
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
            OnPropertyChanged(nameof(CurrentHealth));
            OnPropertyChanged(nameof(MaxHealth));
            OnPropertyChanged(nameof(HealthRatio));
            OnPropertyChanged(nameof(ActionGauge));
            OnPropertyChanged(nameof(ActionGaugeRatio));
            OnPropertyChanged(nameof(IsDead));
            OnPropertyChanged(nameof(IsShadeVisible));
        }
    }
}
