using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SmasherDestruction.Datas;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// ボロノイ図を利用したメッシュ切断機能を提供する
/// </summary>
public static class Nawabari
{
    /// <summary>
    /// 母点のリスト
    /// </summary>
    private static List<Vector3> _points = new();

    /// <summary>
    /// 各領域の境界線を構成する三角形の頂点のインデックスのリスト
    /// </summary>
    private static List<List<int>> _borders = new();

    /// <summary>
    /// 頂点インデックスのリストを格納しているリスト
    /// </summary>
    private static List<List<int>> _sites = new();

    public static List<Vector3> Points => _points;

    public static List<List<int>> Sites => _sites;


    public static void CreateFragmentedMeshes(int count, Mesh mesh)
    {
        ClearAll();

        CreatePoints(mesh.bounds.extents, count);
        CreateSites(mesh.vertices);
        var ms = GetSeparatedMeshes(mesh);
        
        // 次に切断面を形成する処理を実行すればひとまず完成

        for (int i = 0; i < ms.Length; i++)
        {
            var obj = new GameObject($"sphere{i}", new[] { typeof(MeshFilter), typeof(MeshRenderer) });
            obj.GetComponent<MeshFilter>().sharedMesh = ms[i].ToMesh();
            Debug.Log($"vertices {ms[i].Vertices.Count} border vertices {ms[i].BorderVertices.Count}");


            foreach (var seperatedMesh in ms)
            {
                foreach (var vertex in seperatedMesh.BorderVertices)
                {
                    var c = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    c.transform.localScale = Vector3.one * 0.05f;
                    c.transform.localPosition = mesh.vertices[vertex];
                }
            }
        }
    }

    private static void ClearAll()
    {
        _points.Clear();
        _sites.Clear();
        _borders.Clear();
    }

    /// <summary>
    /// 与えられたメッシュのサイズのなかにおさまるように母点を置く
    /// </summary>
    /// <param name="extents">メッシュのbounds.extents</param>
    /// <param name="count">母点の数</param>
    private static void CreatePoints(Vector3 extents, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var x = Random.Range(-extents.x, extents.x);
            var y = Random.Range(-extents.y, extents.y);
            var z = Random.Range(-extents.z, extents.z);

            var v = new Vector3(x, y, z);

            _points.Add(v); // 母点の追加
            _sites.Add(new List<int>()); // 領域に頂点のインデックスを追加する
        }
    }

    /// <summary>
    /// 与えられた頂点を領域分けする
    /// </summary>
    /// <param name="vertices">メッシュの頂点</param>
    private static void CreateSites(Vector3[] vertices)
    {
        int ind;
        float d, dmin;

        for (int i = 0; i < vertices.Length; i++)
        {
            var vertex = vertices[i];
            ind = -1;
            dmin = Int32.MaxValue;
            for (int it = 0; it < _points.Count; it++)
            {
                var p = _points[it];
                d = Vector3.Distance(p, vertex);
                if (d < dmin)
                {
                    dmin = d; // 一番近い母点との距離
                    ind = it; // 一番近い母点の添え字
                }
            }

            // 頂点のインデックスを追加
            // 重複は削除
            if (ind > -1)
            {
                // 先に重複を削除しておく
                for (int j = 0; j < _sites.Count; j++)
                {
                    if (j != ind)
                    {
                        if (_sites[j].Contains(i))
                        {
                            _sites[j].Remove(i);
                        }
                    }
                }

                _sites[ind].Add(i);
            }
        }
    }

    private static SeperatedMesh[] GetSeparatedMeshes(Mesh mesh)
    {
        var seperatedMeshes = new SeperatedMesh[_sites.Count];

        for (int i = 0; i < _sites.Count; i++)
        {
            seperatedMeshes[i] = new SeperatedMesh();
        }

        // 三角形単位でループを実行
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            var v1 = mesh.triangles[i]; // 頂点１
            var v2 = mesh.triangles[i + 1]; // 頂点２
            var v3 = mesh.triangles[i + 2]; // 頂点３

            _borders = new();
            for (int j = 0; j < _sites.Count; j++)
            {
                _borders.Add(new List<int>());
            }

            // NOTE: ２つの領域に存在する辺が領域の境界線を構成するはず

            for (int j = 0; j < _sites.Count; j++)
            {
                bool c1, c2, c3; // condition1,2,3
                c1 = _sites[j].Contains(v1);
                c2 = _sites[j].Contains(v2);
                c3 = _sites[j].Contains(v3);
                // 三角形単位で判定をする。領域内の三角形のみ追加
                // 三角形の頂点すべてがその両位以内なら
                if (c1 && c2 && c3)
                {
                    seperatedMeshes[j].SubIndices.Add(new List<int>());
                    seperatedMeshes[j].AddTriangle(v1, v2, v3, 0, ref mesh);
                    // 個々の処理自体は正しく動作しているように見えておかしな頂点の重複がある
                }
                // すくなくとも１つほかの領域にある場合には
                // その３つは境界線を構成することが保証される
                else
                {
                    var s1 = FindSite(v1);
                    var s2 = FindSite(v2);
                    var s3 = FindSite(v3);

                    // 現在参照している領域の所属の頂点が境界線を構成し、
                    // まだ境界線を構成する頂点インデックスのリストに登録がないなら登録を実行する
                    if (s1 == j && !seperatedMeshes[j].BorderVertices.Contains(v1))
                    {
                        seperatedMeshes[j].BorderVertices.Add(v1);
                    }

                    if (s2 == j && !seperatedMeshes[j].BorderVertices.Contains(v2))
                    {
                        seperatedMeshes[j].BorderVertices.Add(v2);
                    }

                    if (s3 == j && !seperatedMeshes[j].BorderVertices.Contains(v3))
                    {
                        seperatedMeshes[j].BorderVertices.Add(v3);
                    }
                }
            }
        }
        
        return seperatedMeshes;
    }

    /// <summary>
    /// 渡される頂点のインデックスからその頂点の所属領域のインデックスを返す
    /// </summary>
    /// <param name="vertexIndex">頂点のインデックス</param>
    /// <returns>所属する領域のインデックス</returns>
    private static int FindSite(int vertexIndex)
    {
        if (_sites.Count is 0) return -1;
        
        for (int i = 0; i < _sites.Count; i++)
        {
            if (_sites[i].Contains(vertexIndex))
            {
                return i;
            }
        }

        return -1;
    }
}