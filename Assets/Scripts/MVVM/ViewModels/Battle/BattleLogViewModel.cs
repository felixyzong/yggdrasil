using System.Collections.Generic;
using Core.Battle;
using Core.Battle.Events;

namespace MVVM.ViewModels.Battle
{
    public class BattleLogViewModel : ViewModelBase
    {
        private readonly List<string> entries = new List<string>();

        public IReadOnlyList<string> Entries => entries;
        public int MaxEntries { get; set; } = 50;

        public void AddMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            entries.Add(message);
            while (entries.Count > MaxEntries)
            {
                entries.RemoveAt(0);
            }

            OnPropertyChanged(nameof(Entries));
        }

        public void AddEvents(BattleState state, IEnumerable<IBattleEvent> events)
        {
            if (events == null) return;

            foreach (var battleEvent in events)
            {
                AddMessage(FormatEvent(state, battleEvent));
            }
        }

        public void Clear()
        {
            if (entries.Count == 0) return;

            entries.Clear();
            OnPropertyChanged(nameof(Entries));
        }

        private static string FormatEvent(BattleState state, IBattleEvent battleEvent)
        {
            if (battleEvent is TurnStartedEvent turnStarted)
            {
                return $"Turn {turnStarted.TurnIndex}: {GetName(state, turnStarted.CharacterId)} starts.";
            }

            if (battleEvent is TurnEndedEvent turnEnded)
            {
                return $"Turn {turnEnded.TurnIndex}: {GetName(state, turnEnded.CharacterId)} ends.";
            }

            if (battleEvent is CharacterMovedEvent moved)
            {
                return $"{GetName(state, moved.CharacterId)} moves {moved.From} -> {moved.To}.";
            }

            if (battleEvent is CharacterDamagedEvent damaged)
            {
                return $"{GetName(state, damaged.SourceId)} hits {GetName(state, damaged.TargetId)} for {damaged.Amount}.";
            }

            if (battleEvent is CharacterDiedEvent died)
            {
                return $"{GetName(state, died.CharacterId)} dies.";
            }

            if (battleEvent is BattleEndedEvent ended)
            {
                return $"Battle ended: {ended.Result}.";
            }

            return battleEvent?.GetType().Name ?? "Unknown battle event.";
        }

        private static string GetName(BattleState state, string characterId)
        {
            if (state == null) return characterId;
            return state.GetCharacter(characterId)?.Name ?? characterId;
        }
    }
}
