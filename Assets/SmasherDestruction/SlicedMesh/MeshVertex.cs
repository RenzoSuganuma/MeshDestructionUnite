using UnityEngine;

namespace SmasherDestruction.Datas
{
    /// <summary>
    /// 頂点の位置、法線、UVをまとめた構造体
    /// </summary>
    public struct MeshVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 Uv;

        public MeshVertex(Vector3 position, Vector3 normal, Vector2 uv)
        {
            this.Position = position;
            this.Normal = normal;
            this.Uv = uv;
        }
    }
}