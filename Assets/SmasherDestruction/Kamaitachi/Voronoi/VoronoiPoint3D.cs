namespace SmasherDestruction.Kamaitachi.Voronoi
{
    /// <summary>
    /// 母点の構造体
    /// </summary>
    public struct VoronoiPoint3D
    {
        public int x, y, z;
        
        public VoronoiPoint3D(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}