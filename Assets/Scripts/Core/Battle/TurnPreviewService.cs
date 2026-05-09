using System;
using System.Collections.Generic;
using System.Linq;
using Core.Character.Model;

namespace Core.Battle
{
    public class TurnPreviewEntry
    {
        public string CharacterId { get; }
        public string Name { get; }
        public TeamType Team { get; }

        public TurnPreviewEntry(string characterId, string name, TeamType team)
        {
            CharacterId = characterId;
            Name = name;
            Team = team;
        }
    }

    public class TurnPreviewService
    {
        public IReadOnlyList<TurnPreviewEntry> Preview(BattleState state, int count)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (count <= 0) return Array.Empty<TurnPreviewEntry>();
            if (state.Result != BattleResult.None) return Array.Empty<TurnPreviewEntry>();

            var snapshots = state.GetAliveCharacters()
                .Select(character => new CharacterGaugeSnapshot(character))
                .ToList();

            var result = new List<TurnPreviewEntry>(count);

            var current = state.GetCharacter(state.CurrentActorId);
            if (current != null && !current.IsDead && state.Phase == BattlePhase.WaitingForCommand)
            {
                result.Add(new TurnPreviewEntry(current.InstanceId, current.Name, current.Team));
                var currentSnapshot = snapshots.FirstOrDefault(snapshot => snapshot.Character.InstanceId == current.InstanceId);
                if (currentSnapshot != null)
                {
                    currentSnapshot.ActionGauge = 0;
                }
            }

            while (result.Count < count)
            {
                var next = PickNextReady(snapshots, state.ActionGaugeThreshold);
                if (next == null)
                {
                    AdvanceUntilReady(snapshots, state.ActionGaugeThreshold);
                    next = PickNextReady(snapshots, state.ActionGaugeThreshold);
                }

                if (next == null)
                {
                    break;
                }

                result.Add(new TurnPreviewEntry(next.Character.InstanceId, next.Character.Name, next.Character.Team));
                next.ActionGauge = 0;
            }

            return result.AsReadOnly();
        }

        private static void AdvanceUntilReady(List<CharacterGaugeSnapshot> snapshots, int threshold)
        {
            var candidates = snapshots
                .Where(snapshot => snapshot.Character.Speed > 0)
                .ToList();

            if (candidates.Count == 0)
            {
                return;
            }

            float minTime = float.MaxValue;
            foreach (var candidate in candidates)
            {
                int remaining = threshold - candidate.ActionGauge;
                float time = remaining / (float)candidate.Character.Speed;
                minTime = Math.Min(minTime, time);
            }

            foreach (var candidate in candidates)
            {
                int gain = Math.Max(1, (int)Math.Ceiling(candidate.Character.Speed * minTime));
                candidate.ActionGauge += gain;
            }
        }

        private static CharacterGaugeSnapshot PickNextReady(List<CharacterGaugeSnapshot> snapshots, int threshold)
        {
            return snapshots
                .Where(snapshot => snapshot.ActionGauge >= threshold)
                .OrderByDescending(snapshot => snapshot.ActionGauge)
                .ThenByDescending(snapshot => snapshot.Character.Speed)
                .ThenBy(snapshot => GetTeamPriority(snapshot.Character.Team))
                .ThenBy(snapshot => snapshot.Character.InstanceId, StringComparer.Ordinal)
                .FirstOrDefault();
        }

        private static int GetTeamPriority(TeamType team)
        {
            switch (team)
            {
                case TeamType.Player:
                    return 0;
                case TeamType.Ally:
                    return 1;
                case TeamType.Enemy:
                    return 2;
                default:
                    return 3;
            }
        }

        private class CharacterGaugeSnapshot
        {
            public CharacterModel Character { get; }
            public int ActionGauge { get; set; }

            public CharacterGaugeSnapshot(CharacterModel character)
            {
                Character = character;
                ActionGauge = character.ActionGauge;
            }
        }
    }
}
