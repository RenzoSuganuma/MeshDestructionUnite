namespace SmasherDestruction.Kamaitachi.Voronoi
{
    /// <summary>
    /// ボロノイのユーティリティクラス
    /// </summary>
    public static class VoronoiUtility
    {
        public static int MAP_XYZ = 32;
        public static int VORONOI_COUNT_MAX = 1;

        /// <summary>
        /// 点の座標と渡される座標の差分の二乗を返す
        /// </summary>
        public static int DistanceSqrt(Point point, int x, int y, int z)
        {
            int xd = x - point.x;
            int yd = y - point.y;
            int zd = z - point.z;
            return (xd * xd) + (yd * yd) + (zd * zd);
        }
    }
}