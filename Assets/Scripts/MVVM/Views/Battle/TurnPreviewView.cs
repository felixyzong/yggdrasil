using System.Collections.Generic;
using System.ComponentModel;
using MVVM.ViewModels.Battle;
using UnityEngine;

namespace MVVM.Views.Battle
{
    public class TurnPreviewView : MonoBehaviour
    {
        [SerializeField] private TurnPreviewItemView itemPrefab;
        [SerializeField] private Transform itemRoot;
        [SerializeField] private CharacterSpriteCatalog spriteCatalog;

        private readonly List<TurnPreviewItemView> itemViews = new List<TurnPreviewItemView>();
        private TurnPreviewViewModel viewModel;

        public void Bind(TurnPreviewViewModel turnPreviewViewModel)
        {
            if (viewModel != null)
            {
                viewModel.PropertyChanged -= OnViewModelChanged;
            }

            viewModel = turnPreviewViewModel;

            if (viewModel != null)
            {
                viewModel.PropertyChanged += OnViewModelChanged;
            }

            Rebuild();
        }

        private void OnDestroy()
        {
            if (viewModel != null)
            {
                viewModel.PropertyChanged -= OnViewModelChanged;
            }

            Clear();
        }

        private void OnViewModelChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TurnPreviewViewModel.Items))
            {
                EnsureItemViews();
            }
        }

        private void Rebuild()
        {
            Clear();
            EnsureItemViews();
        }

        private void EnsureItemViews()
        {
            if (viewModel == null || itemPrefab == null) return;
            if (itemViews.Count == viewModel.Items.Count) return;

            Transform root = itemRoot != null ? itemRoot : transform;
            for (int i = itemViews.Count; i < viewModel.Items.Count; i++)
            {
                var itemView = Instantiate(itemPrefab, root);
                itemView.Bind(viewModel.Items[i], spriteCatalog);
                itemViews.Add(itemView);
            }
        }

        private void Clear()
        {
            foreach (var itemView in itemViews)
            {
                if (itemView != null) Destroy(itemView.gameObject);
            }

            itemViews.Clear();
        }
    }
}
