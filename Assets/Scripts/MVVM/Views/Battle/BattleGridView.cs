using System.Collections.Generic;
using System.ComponentModel;
using MVVM.ViewModels.Battle;
using UnityEngine;

namespace MVVM.Views.Battle
{
    public class BattleGridView : MonoBehaviour
    {
        [SerializeField] private GridCellView cellPrefab;
        [SerializeField] private BattleUnitView unitPrefab;
        [SerializeField] private Transform cellRoot;
        [SerializeField] private Transform unitRoot;
        [SerializeField] private CharacterSpriteCatalog spriteCatalog;
        [SerializeField] private Vector2 cellSize = Vector2.one;
        [SerializeField] private bool centerGrid = true;

        private readonly List<GridCellView> cellViews = new List<GridCellView>();
        private readonly List<BattleUnitView> unitViews = new List<BattleUnitView>();
        private BattleViewModel viewModel;

        public CharacterSpriteCatalog SpriteCatalog => spriteCatalog;

        public void Bind(BattleViewModel battleViewModel)
        {
            if (viewModel != null)
            {
                viewModel.PropertyChanged -= OnViewModelChanged;
            }

            viewModel = battleViewModel;
            if (viewModel != null)
            {
                viewModel.PropertyChanged += OnViewModelChanged;
            }

            Rebuild();
        }

        public void SelectCell(GridCellViewModel cellViewModel)
        {
            viewModel?.SelectCell(cellViewModel.Position);
        }

        public void SelectUnit(BattleUnitViewModel unitViewModel)
        {
            viewModel?.SelectUnit(unitViewModel.InstanceId);
        }

        public Vector3 GridToLocalPosition(int x, int y)
        {
            float offsetX = centerGrid && viewModel?.State != null
                ? (viewModel.State.Grid.Width - 1) * cellSize.x * 0.5f
                : 0f;
            float offsetY = centerGrid && viewModel?.State != null
                ? (viewModel.State.Grid.Height - 1) * cellSize.y * 0.5f
                : 0f;

            return new Vector3(x * cellSize.x - offsetX, y * cellSize.y - offsetY, 0f);
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
            if (e.PropertyName == nameof(BattleViewModel.GridCells)
                || e.PropertyName == nameof(BattleViewModel.Units))
            {
                Rebuild();
                return;
            }

            RefreshLayout();
        }

        private void Rebuild()
        {
            ClearViews();
            if (viewModel == null) return;

            Transform resolvedCellRoot = cellRoot != null ? cellRoot : transform;
            Transform resolvedUnitRoot = unitRoot != null ? unitRoot : transform;

            foreach (var cell in viewModel.GridCells)
            {
                if (cellPrefab == null) break;

                var cellView = Instantiate(cellPrefab, resolvedCellRoot);
                cellView.Bind(cell, this);
                SetLocalPosition(cellView.transform, GridToLocalPosition(cell.X, cell.Y));
                cellViews.Add(cellView);
            }

            foreach (var unit in viewModel.Units)
            {
                if (unitPrefab == null) break;

                var unitView = Instantiate(unitPrefab, resolvedUnitRoot);
                unitView.Bind(unit, this, spriteCatalog);
                SetLocalPosition(unitView.transform, GridToLocalPosition(unit.X, unit.Y));
                unitViews.Add(unitView);
            }
        }

        private void RefreshLayout()
        {
            foreach (var cellView in cellViews)
            {
                if (cellView.ViewModel == null) continue;
                SetLocalPosition(cellView.transform, GridToLocalPosition(cellView.ViewModel.X, cellView.ViewModel.Y));
            }

            foreach (var unitView in unitViews)
            {
                if (unitView.ViewModel == null) continue;
                SetLocalPosition(unitView.transform, GridToLocalPosition(unitView.ViewModel.X, unitView.ViewModel.Y));
            }
        }

        private void ClearViews()
        {
            foreach (var view in cellViews)
            {
                if (view != null) Destroy(view.gameObject);
            }

            foreach (var view in unitViews)
            {
                if (view != null) Destroy(view.gameObject);
            }

            cellViews.Clear();
            unitViews.Clear();
        }

        private static void SetLocalPosition(Transform target, Vector3 localPosition)
        {
            if (target is RectTransform rectTransform)
            {
                rectTransform.anchoredPosition = new Vector2(localPosition.x, localPosition.y);
            }
            else
            {
                target.localPosition = localPosition;
            }
        }
    }
}
