using System;
using System.Linq;
using Core.Character.Model;

namespace Core.Battle
{
    public class ActionGaugeSystem
    {
        public void AdvanceUntilReady(BattleState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (HasReadyActor(state)) return;

            var candidates = state.GetAliveCharacters()
                .Where(character => character.Speed > 0)
                .ToList();

            if (candidates.Count == 0)
            {
                throw new InvalidOperationException("No living character can gain action gauge.");
            }

            float minTime = float.MaxValue;

            foreach (var character in candidates)
            {
                int remaining = state.ActionGaugeThreshold - character.ActionGauge;
                float time = remaining / (float)character.Speed;
                minTime = Math.Min(minTime, time);
            }

            foreach (var character in candidates)
            {
                int gain = Math.Max(1, (int)Math.Ceiling(character.Speed * minTime));
                character.AddActionGauge(gain);
            }
        }

        public CharacterModel PickNextActor(BattleState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            return state.GetAliveCharacters()
                .Where(character => character.ActionGauge >= state.ActionGaugeThreshold)
                .OrderByDescending(character => character.ActionGauge)
                .ThenByDescending(character => character.Speed)
                .ThenBy(character => GetTeamPriority(character.Team))
                .ThenBy(character => character.InstanceId, StringComparer.Ordinal)
                .FirstOrDefault();
        }

        private static bool HasReadyActor(BattleState state)
        {
            return state.GetAliveCharacters().Any(character => character.ActionGauge >= state.ActionGaugeThreshold);
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
    }
}
