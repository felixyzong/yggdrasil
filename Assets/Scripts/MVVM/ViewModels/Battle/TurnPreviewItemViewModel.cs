using Core.Character.Model;

namespace MVVM.ViewModels.Battle
{
    public class TurnPreviewItemViewModel : ViewModelBase
    {
        private int order;
        private string characterId;
        private string displayName;
        private TeamType team;

        public int Order
        {
            get => order;
            private set => SetProperty(ref order, value);
        }

        public string CharacterId
        {
            get => characterId;
            private set => SetProperty(ref characterId, value);
        }

        public string DisplayName
        {
            get => displayName;
            private set => SetProperty(ref displayName, value);
        }

        public TeamType Team
        {
            get => team;
            private set => SetProperty(ref team, value);
        }

        public string SpriteKey => CharacterId;
        public string Label => $"{Order}. {DisplayName}";

        public void Set(int order, string characterId, string displayName, TeamType team)
        {
            Order = order;
            CharacterId = characterId;
            DisplayName = displayName;
            Team = team;
            OnPropertyChanged(nameof(SpriteKey));
            OnPropertyChanged(nameof(Label));
        }
    }
}
