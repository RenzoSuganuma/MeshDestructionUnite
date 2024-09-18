using System.Collections.Generic;

namespace SmasherDestruction.Datas
{
    /// <summary>
    /// 基底クラスのフィールドに境界線を構成する頂点群のリストを追加
    /// </summary>
    public class SeperatedMesh : SlicedMesh
    {
        /// <summary>
        /// 境界線を構成する頂点群
        /// </summary>
        public List<int> BorderVertices = new();
    }
}