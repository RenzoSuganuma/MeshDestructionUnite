using System.Collections.Generic;
using UnityEngine;

/// <summary> 切断されたメッシュ </summary>
public class CuttedMesh
{
    public List<Vector3> Vertices = new List<Vector3>();
    public List<Vector3> Normals = new List<Vector3>();
    public List<Vector2> UVs = new List<Vector2>();
    public List<int> Triangles = new List<int>();
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
    public void AddTriangle(int p0, int p1, int p2, int subMesh, ref Mesh victimMesh)
    {
        int baseIndex = Vertices.Count;

        // 対象のサブメッシュのインデックスへ追加
        SubIndices[subMesh].Add(baseIndex);
        SubIndices[subMesh].Add(baseIndex + 1);
        SubIndices[subMesh].Add(baseIndex + 2);

        // 三角形群の設定
        Triangles.Add(baseIndex);
        Triangles.Add(baseIndex + 1);
        Triangles.Add(baseIndex + 2);

        // 対象メッシュから頂点データを取得する   
        Vertices.Add(victimMesh.vertices[p0]);
        Vertices.Add(victimMesh.vertices[p1]);
        Vertices.Add(victimMesh.vertices[p2]);

        // 法線も同様に取得
        Normals.Add(victimMesh.normals[p0]);
        Normals.Add(victimMesh.normals[p1]);
        Normals.Add(victimMesh.normals[p2]);

        // UVも同様に取得
        UVs.Add(victimMesh.uv[p0]);
        UVs.Add(victimMesh.uv[p1]);
        UVs.Add(victimMesh.uv[p2]);
    }

    /// <summary>
    /// トライアングルの追加。ここではポリゴンを渡し、それを追加する
    /// </summary>
    public void AddTriangle(Vector3[] points3, Vector3[] normals3, Vector2[] uvs3, Vector3 faceNormal, int subMesh)
    {
        Vector3 normalCalculated =
            Vector3.Cross((points3[1] - points3[0]).normalized, (points3[2] - points3[0]).normalized);

        int p0, p1, p2;

        p0 = 0;
        p1 = 1;
        p2 = 2;

        // 法線とトライアングルが逆の場合には面を裏返す
        if (Vector3.Dot(normalCalculated, faceNormal) < 0)
        {
            p0 = 2;
            // p1 は真ん中のためここで初期化しなくてもよい
            p2 = 0;
        }

        int baseIndex = Vertices.Count;

        SubIndices[subMesh].Add(baseIndex);
        SubIndices[subMesh].Add(baseIndex + 1);
        SubIndices[subMesh].Add(baseIndex + 2);

        Triangles.Add(baseIndex);
        Triangles.Add(baseIndex + 1);
        Triangles.Add(baseIndex + 2);

        Vertices.Add(points3[p0]);
        Vertices.Add(points3[p1]);
        Vertices.Add(points3[p2]);

        Normals.Add(normals3[p0]);
        Normals.Add(normals3[p1]);
        Normals.Add(normals3[p2]);

        UVs.Add(uvs3[p0]);
        UVs.Add(uvs3[p1]);
        UVs.Add(uvs3[p2]);
    }
}
