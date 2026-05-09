using System.ComponentModel;
using Core.Character.Model;
using MVVM.ViewModels.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace MVVM.Views.Battle
{
    public class TurnPreviewItemView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Image background;
        [SerializeField] private Text label;
        [SerializeField] private Color playerColor = new Color(0.18f, 0.45f, 0.95f, 0.85f);
        [SerializeField] private Color enemyColor = new Color(0.9f, 0.22f, 0.18f, 0.85f);
        [SerializeField] private Color allyColor = new Color(0.2f, 0.75f, 0.35f, 0.85f);
        [SerializeField] private Color emptyColor = new Color(0.15f, 0.15f, 0.15f, 0.35f);

        private TurnPreviewItemViewModel viewModel;
        private CharacterSpriteCatalog spriteCatalog;

        public void Bind(TurnPreviewItemViewModel itemViewModel, CharacterSpriteCatalog catalog)
        {
            if (viewModel != null)
            {
                viewModel.PropertyChanged -= OnViewModelChanged;
            }

            viewModel = itemViewModel;
            spriteCatalog = catalog;

            if (viewModel != null)
            {
                viewModel.PropertyChanged += OnViewModelChanged;
            }

            Refresh();
        }

        private void OnDestroy()
        {
            if (viewModel != null)
            {
                viewModel.PropertyChanged -= OnViewModelChanged;
            }
        }

        private void OnViewModelChanged(object sender, PropertyChangedEventArgs e)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (viewModel == null) return;

            bool hasCharacter = !string.IsNullOrEmpty(viewModel.CharacterId);
            if (label != null)
            {
                label.text = hasCharacter ? viewModel.Label : $"{viewModel.Order}.";
            }

            if (icon != null)
            {
                icon.sprite = spriteCatalog != null ? spriteCatalog.GetSprite(viewModel.SpriteKey) : null;
                icon.enabled = hasCharacter && icon.sprite != null;
            }

            if (background != null)
            {
                background.color = hasCharacter ? GetTeamColor(viewModel.Team) : emptyColor;
            }
        }

        private Color GetTeamColor(TeamType team)
        {
            switch (team)
            {
                case TeamType.Player:
                    return playerColor;
                case TeamType.Ally:
                    return allyColor;
                case TeamType.Enemy:
                    return enemyColor;
                default:
                    return emptyColor;
            }
        }
    }
}
