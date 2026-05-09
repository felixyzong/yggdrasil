using System.ComponentModel;
using MVVM.ViewModels.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace MVVM.Views.Battle
{
    public class GridCellView : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Button button;
        [SerializeField] private Text label;
        [SerializeField] private Color normalColor = new Color(0.18f, 0.2f, 0.22f, 0.9f);
        [SerializeField] private Color blockedColor = new Color(0.08f, 0.08f, 0.08f, 0.9f);
        [SerializeField] private Color moveColor = new Color(0.1f, 0.55f, 0.95f, 0.85f);
        [SerializeField] private Color attackColor = new Color(0.95f, 0.25f, 0.18f, 0.85f);
        [SerializeField] private Color selectedColor = new Color(1f, 0.85f, 0.25f, 0.95f);

        private BattleGridView gridView;

        public GridCellViewModel ViewModel { get; private set; }

        public void Bind(GridCellViewModel viewModel, BattleGridView owner)
        {
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged -= OnViewModelChanged;
            }

            ViewModel = viewModel;
            gridView = owner;

            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
                button.onClick.AddListener(HandleClick);
            }

            if (ViewModel != null)
            {
                ViewModel.PropertyChanged += OnViewModelChanged;
            }

            Refresh();
        }

        private void OnDestroy()
        {
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged -= OnViewModelChanged;
            }

            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
            }
        }

        private void OnMouseDown()
        {
            HandleClick();
        }

        private void OnViewModelChanged(object sender, PropertyChangedEventArgs e)
        {
            Refresh();
        }

        private void HandleClick()
        {
            if (ViewModel == null) return;
            gridView?.SelectCell(ViewModel);
        }

        private void Refresh()
        {
            if (ViewModel == null) return;

            Color color = GetColor(ViewModel.HighlightType);
            if (image != null)
            {
                image.color = color;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }

            if (label != null)
            {
                label.text = $"{ViewModel.X},{ViewModel.Y}";
            }
        }

        private Color GetColor(GridCellHighlightType highlightType)
        {
            switch (highlightType)
            {
                case GridCellHighlightType.Selected:
                    return selectedColor;
                case GridCellHighlightType.AttackCandidate:
                    return attackColor;
                case GridCellHighlightType.MoveCandidate:
                    return moveColor;
                case GridCellHighlightType.Blocked:
                    return blockedColor;
                default:
                    return normalColor;
            }
        }
    }
}
