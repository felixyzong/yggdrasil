namespace Core.Grid
{
    public class GridCell
    {
        public GridPosition Position { get; }
        public bool IsWalkable { get; set; }

        public GridCell(GridPosition position, bool isWalkable = true)
        {
            Position = position;
            IsWalkable = isWalkable;
        }
    }
}
