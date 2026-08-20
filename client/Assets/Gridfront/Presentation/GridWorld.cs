using Gridfront.BattleCore.Pathfinding;
using UnityEngine;

namespace Gridfront.Client.Presentation
{
    public static class GridWorld
    {
        public static Vector3 CellCenter(GridPos pos)
        {
            return new Vector3(pos.X, 0f, pos.Y);
        }

        public static Vector3 FromMilli(int xMilli, int yMilli, float height)
        {
            return new Vector3(
                xMilli / (float)GridMap.TileUnits,
                height,
                yMilli / (float)GridMap.TileUnits);
        }
    }
}
