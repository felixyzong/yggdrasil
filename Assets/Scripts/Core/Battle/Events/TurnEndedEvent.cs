namespace Core.Battle.Events
{
    public class TurnEndedEvent : IBattleEvent
    {
        public string CharacterId { get; }
        public int TurnIndex { get; }

        public TurnEndedEvent(string characterId, int turnIndex)
        {
            CharacterId = characterId;
            TurnIndex = turnIndex;
        }
    }
}
