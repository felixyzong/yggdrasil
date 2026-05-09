using System;
using System.Collections.Generic;
using System.Linq;
using Core.Character.Model;
using Core.Grid;

namespace Core.Battle
{
    public class BattleState
    {
        private readonly List<CharacterModel> characters;

        public GridMap Grid { get; }
        public IReadOnlyList<CharacterModel> Characters => characters;
        public string CurrentActorId { get; private set; }
        public int TurnIndex { get; private set; }
        public int ActionGaugeThreshold { get; }
        public BattlePhase Phase { get; private set; }
        public BattleResult Result { get; private set; }

        public BattleState(GridMap grid, IEnumerable<CharacterModel> characters, int actionGaugeThreshold = 1000)
        {
            if (grid == null) throw new ArgumentNullException(nameof(grid));
            if (characters == null) throw new ArgumentNullException(nameof(characters));
            if (actionGaugeThreshold <= 0) throw new ArgumentOutOfRangeException(nameof(actionGaugeThreshold));

            Grid = grid;
            this.characters = characters.ToList();
            if (this.characters.Count == 0) throw new ArgumentException("Battle needs at least one character.", nameof(characters));
            if (this.characters.Any(character => character == null)) throw new ArgumentException("Characters cannot contain null.", nameof(characters));

            var duplicateId = this.characters
                .GroupBy(character => character.InstanceId)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicateId != null) throw new ArgumentException($"Duplicate character instance id: {duplicateId.Key}", nameof(characters));

            ActionGaugeThreshold = actionGaugeThreshold;
            Phase = BattlePhase.Init;
            Result = BattleResult.None;
        }

        public CharacterModel GetCharacter(string instanceId)
        {
            if (string.IsNullOrWhiteSpace(instanceId)) return null;
            return characters.FirstOrDefault(character => character.InstanceId == instanceId);
        }

        public CharacterModel GetCharacterAt(GridPosition position)
        {
            return characters.FirstOrDefault(character => !character.IsDead && character.Position == position);
        }

        public bool IsCellOccupied(GridPosition position)
        {
            return GetCharacterAt(position) != null;
        }

        public bool IsCellAvailable(GridPosition position)
        {
            return Grid.IsInside(position) && Grid.IsWalkable(position) && !IsCellOccupied(position);
        }

        public IEnumerable<CharacterModel> GetAliveCharacters()
        {
            return characters.Where(character => !character.IsDead);
        }

        public IEnumerable<CharacterModel> GetAliveCharacters(TeamType team)
        {
            return characters.Where(character => !character.IsDead && character.Team == team);
        }

        public void SetCurrentActor(string actorId)
        {
            if (string.IsNullOrWhiteSpace(actorId)) throw new ArgumentException("Actor id cannot be null or whitespace.", nameof(actorId));
            if (GetCharacter(actorId) == null) throw new ArgumentException($"Unknown actor id: {actorId}", nameof(actorId));
            CurrentActorId = actorId;
        }

        public void ClearCurrentActor()
        {
            CurrentActorId = null;
        }

        public void IncrementTurnIndex()
        {
            TurnIndex++;
        }

        public void SetPhase(BattlePhase phase)
        {
            Phase = phase;
        }

        public void SetResult(BattleResult result)
        {
            Result = result;
            if (result != BattleResult.None)
            {
                Phase = BattlePhase.BattleEnd;
            }
        }
    }
}
