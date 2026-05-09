namespace Core.Battle.Events
{
    public class CharacterDiedEvent : IBattleEvent
    {
        public string CharacterId { get; }

        public CharacterDiedEvent(string characterId)
        {
            CharacterId = characterId;
        }
    }
}
