using System.Collections.Generic;
using UnityEngine;

namespace SmasherDestruction.Datas
{
    /// <summary> 切断されたメッシュ情報を一時的に保存する時に活用する </summary>
    public class SlicedMesh
    {
        /// <summary>
        /// 頂点リスト
        /// </summary>
        public List<Vector3> Vertices = new List<Vector3>();

        /// <summary>
        /// 法線リスト
        /// </summary>
        public List<Vector3> Normals = new List<Vector3>();

        /// <summary>
        /// UVリスト
        /// </summary>
        public List<Vector2> UVs = new List<Vector2>();

        /// <summary>
        /// 三角形情報
        /// </summary>
        public List<int> Triangles = new List<int>();

        /// <summary>
        /// サブメッシュのインデックスリスト
        /// </summary>
        public List<List<int>> SubIndices = new List<List<int>>();

        /// <summary>
        /// このクラスに保持しているデータをすべて消す
        /// </summary>
        public void ClearAll()
        {
            Vertices.Clear();
            Normals.Clear();
            UVs.Clear();
            Triangles.Clear();
            SubIndices.Clear();
        }

        /// <summary>
        /// トライアングルとして3頂点を追加する
        /// </summary>
        public void AddTriangle(
            int v1,
            int v2,
            int v3,
            int subMeshIndex,
            ref Mesh sourceMesh)
        {
            int baseIndex = Vertices.Count;

            // 対象のサブメッシュのインデックスへ追加
            SubIndices[subMeshIndex].Add(baseIndex);
            SubIndices[subMeshIndex].Add(baseIndex + 1);
            SubIndices[subMeshIndex].Add(baseIndex + 2);

            // 三角形群の設定
            Triangles.Add(baseIndex);
            Triangles.Add(baseIndex + 1);
            Triangles.Add(baseIndex + 2);

            // 対象メッシュから頂点データを取得する   
            Vertices.Add(sourceMesh.vertices[v1]);
            Vertices.Add(sourceMesh.vertices[v2]);
            Vertices.Add(sourceMesh.vertices[v3]);

            // 法線も同様に取得
            Normals.Add(sourceMesh.normals[v1]);
            Normals.Add(sourceMesh.normals[v2]);
            Normals.Add(sourceMesh.normals[v3]);

            // UVも同様に取得
            UVs.Add(sourceMesh.uv[v1]);
            UVs.Add(sourceMesh.uv[v2]);
            UVs.Add(sourceMesh.uv[v3]);
        }

        /// <summary>
        /// トライアングルの追加。ここではポリゴンを渡し、それを追加する
        /// </summary>
        public void AddTriangle(
            Vector3[] points3,
            Vector3[] normals3,
            Vector2[] uvs3,
            Vector3 faceNormal,
            int subMeshIndex)
        {
            Vector3 normalCalculated =
                Vector3.Cross((points3[1] - points3[0]).normalized,
                    (points3[2] - points3[0]).normalized);

            int v1, v2, v3;

            v1 = 0;
            v2 = 1;
            v3 = 2;

            // 法線と三角形の面が逆の場合には面を裏返す
            if (Vector3.Dot(normalCalculated, faceNormal) < 0)
            {
                // v2 は真ん中のためここで初期化しなくてもよい。
                // v1 -> v2 -> v3
                // v3 <- v2 <- v1
                v1 = 2;
                v3 = 0;
            }

            int baseIndex = Vertices.Count;

            // サブメッシュのインデックス情報を格納
            SubIndices[subMeshIndex].Add(baseIndex);
            SubIndices[subMeshIndex].Add(baseIndex + 1);
            SubIndices[subMeshIndex].Add(baseIndex + 2);

            // 三角形情報を格納
            Triangles.Add(baseIndex);
            Triangles.Add(baseIndex + 1);
            Triangles.Add(baseIndex + 2);

            // 頂点リストに頂点を追加
            Vertices.Add(points3[v1]);
            Vertices.Add(points3[v2]);
            Vertices.Add(points3[v3]);

            // ↑ と同様に法線を法線リスト
            Normals.Add(normals3[v1]);
            Normals.Add(normals3[v2]);
            Normals.Add(normals3[v3]);

            // ↑ と同様にUVをUVリストを追加
            UVs.Add(uvs3[v1]);
            UVs.Add(uvs3[v2]);
            UVs.Add(uvs3[v3]);
        }

        /// <summary>
        /// すでに格納されている情報からメッシュとしてデータを生成する。
        /// </summary>
        /// <returns></returns>
        public Mesh ToMesh()
        {
            Mesh halfMesh = new Mesh();
            halfMesh.name = $"SplittedMesh:{GetHashCode()}";
            halfMesh.vertices = this.Vertices.ToArray();
            halfMesh.triangles = this.Triangles.ToArray();
            halfMesh.normals = this.Normals.ToArray();
            halfMesh.uv = this.UVs.ToArray();
            halfMesh.subMeshCount = this.SubIndices.Count;
            for (int i = 0; i < this.SubIndices.Count; i++)
            {
                halfMesh.SetIndices(this.SubIndices[i].ToArray(), MeshTopology.Triangles, i);
            }

            return halfMesh;
        }
    }
}