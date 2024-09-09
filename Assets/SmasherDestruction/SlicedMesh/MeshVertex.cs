using UnityEngine;

namespace SmasherDestruction.Datas
{
    public struct MeshVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector3 Uv;

        public MeshVertex(Vector3 position, Vector3 normal, Vector3 uv)
        {
            this.Position = position;
            this.Normal = normal;
            this.Uv = uv;
        }
    }
}