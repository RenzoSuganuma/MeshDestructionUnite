namespace SmasherDestruction.Nurikabe.Delanuay
{
    /// <summary>
    /// ドロネー三角形
    /// </summary>
    public struct DelanuayTriangle
    {
        public int p1, p2, p3;

        public DelanuayTriangle(int p1, int p2, int p3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }
    }
}