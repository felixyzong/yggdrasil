using System;
using System.Collections.Generic;
using Core.Battle;

namespace MVVM.ViewModels.Battle
{
    public class TurnPreviewViewModel : ViewModelBase
    {
        private readonly TurnPreviewService previewService;
        private readonly List<TurnPreviewItemViewModel> items = new List<TurnPreviewItemViewModel>();

        public IReadOnlyList<TurnPreviewItemViewModel> Items => items;
        public int PreviewCount { get; }

        public TurnPreviewViewModel()
            : this(new TurnPreviewService(), 10)
        {
        }

        public TurnPreviewViewModel(TurnPreviewService previewService, int previewCount)
        {
            if (previewService == null) throw new ArgumentNullException(nameof(previewService));
            if (previewCount <= 0) throw new ArgumentOutOfRangeException(nameof(previewCount));

            this.previewService = previewService;
            PreviewCount = previewCount;

            for (int i = 0; i < previewCount; i++)
            {
                items.Add(new TurnPreviewItemViewModel());
            }
        }

        public void Refresh(BattleState state)
        {
            var entries = state == null
                ? Array.Empty<TurnPreviewEntry>()
                : previewService.Preview(state, PreviewCount);

            for (int i = 0; i < items.Count; i++)
            {
                if (i < entries.Count)
                {
                    var entry = entries[i];
                    items[i].Set(i + 1, entry.CharacterId, entry.Name, entry.Team);
                }
                else
                {
                    items[i].Set(i + 1, string.Empty, string.Empty, 0);
                }
            }

            OnPropertyChanged(nameof(Items));
        }
    }
}
