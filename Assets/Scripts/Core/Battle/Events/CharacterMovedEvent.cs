using Core.Grid;

namespace Core.Battle.Events
{
    public class CharacterMovedEvent : IBattleEvent
    {
        public string CharacterId { get; }
        public GridPosition From { get; }
        public GridPosition To { get; }

        public CharacterMovedEvent(string characterId, GridPosition from, GridPosition to)
        {
            CharacterId = characterId;
            From = from;
            To = to;
        }
    }
}
