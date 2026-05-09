using Core.Battle;
using Core.Character.Model;

namespace MVVM.ViewModels.Battle
{
    public class CharacterPanelViewModel : ViewModelBase
    {
        private bool hasCharacter;
        private string instanceId;
        private string displayName;
        private string teamText;
        private string hpText;
        private float healthRatio;
        private string positionText;
        private string moveText;
        private string attackStateText;
        private string actionGaugeText;
        private bool isDead;

        public bool HasCharacter
        {
            get => hasCharacter;
            private set => SetProperty(ref hasCharacter, value);
        }

        public string InstanceId
        {
            get => instanceId;
            private set => SetProperty(ref instanceId, value);
        }

        public string DisplayName
        {
            get => displayName;
            private set => SetProperty(ref displayName, value);
        }

        public string TeamText
        {
            get => teamText;
            private set => SetProperty(ref teamText, value);
        }

        public string HpText
        {
            get => hpText;
            private set => SetProperty(ref hpText, value);
        }

        public float HealthRatio
        {
            get => healthRatio;
            private set => SetProperty(ref healthRatio, value);
        }

        public string PositionText
        {
            get => positionText;
            private set => SetProperty(ref positionText, value);
        }

        public string MoveText
        {
            get => moveText;
            private set => SetProperty(ref moveText, value);
        }

        public string AttackStateText
        {
            get => attackStateText;
            private set => SetProperty(ref attackStateText, value);
        }

        public string ActionGaugeText
        {
            get => actionGaugeText;
            private set => SetProperty(ref actionGaugeText, value);
        }

        public bool IsDead
        {
            get => isDead;
            private set => SetProperty(ref isDead, value);
        }

        public void Refresh(BattleState state, string selectedUnitId)
        {
            CharacterModel character = null;
            if (state != null)
            {
                character = state.GetCharacter(selectedUnitId) ?? state.GetCharacter(state.CurrentActorId);
            }

            if (character == null || state == null)
            {
                HasCharacter = false;
                InstanceId = null;
                DisplayName = string.Empty;
                TeamText = string.Empty;
                HpText = string.Empty;
                HealthRatio = 0f;
                PositionText = string.Empty;
                MoveText = string.Empty;
                AttackStateText = string.Empty;
                ActionGaugeText = string.Empty;
                IsDead = false;
                return;
            }

            HasCharacter = true;
            InstanceId = character.InstanceId;
            DisplayName = character.Name;
            TeamText = character.Team.ToString();
            HpText = $"{character.CurrentHealth}/{character.MaxHealth}";
            HealthRatio = character.MaxHealth <= 0 ? 0f : character.CurrentHealth / (float)character.MaxHealth;
            PositionText = $"{character.Position.X}, {character.Position.Y}";
            MoveText = $"Move {character.TurnState.RemainingMove}/{character.MovementRange}";
            AttackStateText = character.TurnState.HasUsedBasicAttack ? "Attack used" : "Attack ready";
            ActionGaugeText = $"{character.ActionGauge}/{state.ActionGaugeThreshold}";
            IsDead = character.IsDead;
        }
    }
}
