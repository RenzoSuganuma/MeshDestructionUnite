namespace SmasherDestruction.Kamaitachi.Delanuay
{
    /// <summary>
    /// ドロネー三角形
    /// </summary>
    public struct DelanuayTriangle3D
    {
        public int p1, p2, p3;

        public DelanuayTriangle3D(int p1, int p2, int p3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }
    }
}