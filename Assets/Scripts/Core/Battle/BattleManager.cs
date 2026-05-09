using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Battle
{
    public class BattleManager
    {
        private List<Core.Character.Model.CharacterModel> PlayerCharacters;
        private List<Core.Character.Model.CharacterModel> EnemyCharacters;
        private List<Core.Character.Model.CharacterModel> AssistCharacters;
        

        public BattleManager(List<Core.Character.Model.CharacterModel> characters)
        {
            if (characters == null || characters.Count == 0) throw new ArgumentException("Characters list cannot be null or empty.", nameof(characters));
            this.PlayerCharacters = characters;
        }

        public void StartBattle()
        {
            // Implement battle logic here
            Console.WriteLine("Battle started with the following characters:");
            foreach (var character in PlayerCharacters)
            {
                Console.WriteLine($"- {character.Name}");
            }
        }
    }
}