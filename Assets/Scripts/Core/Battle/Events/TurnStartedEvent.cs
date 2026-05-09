namespace Core.Battle.Events
{
    public class TurnStartedEvent : IBattleEvent
    {
        public string CharacterId { get; }
        public int TurnIndex { get; }

        public TurnStartedEvent(string characterId, int turnIndex)
        {
            CharacterId = characterId;
            TurnIndex = turnIndex;
        }
    }
}
