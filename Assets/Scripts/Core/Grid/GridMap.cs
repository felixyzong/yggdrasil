using System;

namespace Core.Grid
{
    public class GridMap
    {
        private readonly GridCell[,] cells;

        public int Width { get; }
        public int Height { get; }

        public GridMap(int width, int height)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

            Width = width;
            Height = height;
            cells = new GridCell[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    cells[x, y] = new GridCell(new GridPosition(x, y));
                }
            }
        }

        public bool IsInside(GridPosition position)
        {
            return position.X >= 0 && position.X < Width && position.Y >= 0 && position.Y < Height;
        }

        public GridCell GetCell(GridPosition position)
        {
            if (!IsInside(position)) throw new ArgumentOutOfRangeException(nameof(position));
            return cells[position.X, position.Y];
        }

        public bool IsWalkable(GridPosition position)
        {
            return IsInside(position) && GetCell(position).IsWalkable;
        }
    }
}
