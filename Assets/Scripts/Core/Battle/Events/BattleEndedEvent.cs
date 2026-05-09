namespace Core.Battle.Events
{
    public class BattleEndedEvent : IBattleEvent
    {
        public BattleResult Result { get; }

        public BattleEndedEvent(BattleResult result)
        {
            Result = result;
        }
    }
}
