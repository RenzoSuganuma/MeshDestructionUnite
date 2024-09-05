namespace SmasherDestruction.Kamaitachi.Voronoi
{
    /// <summary>
    /// ボロノイのユーティリティクラス
    /// </summary>
    public static class VoronoiUtility3D
    {
        public static int MAP_XYZ = 32;
        public static int VORONOI_COUNT_MAX = 1;

        /// <summary>
        /// 点の座標と渡される座標の差分の二乗を返す
        /// </summary>
        public static int DistanceSqrt(VoronoiPoint3D voronoiPoint3D, int x, int y, int z)
        {
            int xd = x - voronoiPoint3D.x;
            int yd = y - voronoiPoint3D.y;
            int zd = z - voronoiPoint3D.z;
            return (xd * xd) + (yd * yd) + (zd * zd);
        }
    }
}