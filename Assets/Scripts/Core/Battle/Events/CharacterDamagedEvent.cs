namespace Core.Battle.Events
{
    public class CharacterDamagedEvent : IBattleEvent
    {
        public string SourceId { get; }
        public string TargetId { get; }
        public int Amount { get; }

        public CharacterDamagedEvent(string sourceId, string targetId, int amount)
        {
            SourceId = sourceId;
            TargetId = targetId;
            Amount = amount;
        }
    }
}
