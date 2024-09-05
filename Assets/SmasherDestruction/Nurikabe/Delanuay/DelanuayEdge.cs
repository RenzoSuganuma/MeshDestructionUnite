namespace SmasherDestruction.Nurikabe.Delanuay
{
    /// <summary>
    /// ドロネー辺
    /// </summary>
    public sealed class DelanuayEdge
    {
        public int p1, p2;

        public DelanuayEdge() : this(0, 0)
        {
        }

        public DelanuayEdge(int p1, int p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }

        /// <summary>
        /// 辺が等しいか判定する
        /// </summary>
        public bool Equals(DelanuayEdge other)
        {
            bool res = ((this.p1 == other.p1) && (this.p2 == other.p2)
                        || (this.p1 == other.p2) && (this.p2 == other.p1));
            return res;
        }
    }
}