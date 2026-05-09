using System;

namespace Core.Grid
{
    public static class GridDistance
    {
        public static int Manhattan(GridPosition a, GridPosition b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }
    }
}
