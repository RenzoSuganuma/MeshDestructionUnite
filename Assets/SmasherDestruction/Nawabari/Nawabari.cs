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

        var obj = new GameObject("sphere", new[] { typeof(MeshFilter), typeof(MeshRenderer) });
        obj.GetComponent<MeshFilter>().sharedMesh = ms;

        return;
        // 領域分けした頂点の可視化
        // foreach (var site in _sites)
        // {
        //     var c = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
        //     foreach (var i in site)
        //     {
        //         var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //         obj.transform.localScale = Vector3.one * 0.05f;
        //         obj.transform.localPosition = mesh.vertices[i];
        //         obj.GetComponent<MeshRenderer>().material.color = c;
        //     }
        // }
    }

    private static void ClearAll()
    {
        _points.Clear();
        _sites.Clear();
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

            if (ind > -1)
            {
                // 頂点のインデックスを追加
                // 重複は削除
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

    // $ ↓ バグってます ↓ $
    private static Mesh GetSeparatedMeshes(Mesh mesh)
    {
        SlicedMesh slicedMesh = new SlicedMesh();
        Mesh m = new Mesh();
        List<Vector3> vertices = new();
        List<Vector3> normals = new();
        List<Vector2> uvs = new();
        List<int> triangles = new();

        // 領域１
        foreach (var i in _sites[0])
        {
            vertices.Add(mesh.vertices[i]);
            uvs.Add(mesh.uv[i]);
            normals.Add(mesh.normals[i]);
        }

        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            var v1 = mesh.triangles[i];
            var v2 = mesh.triangles[i + 1];
            var v3 = mesh.triangles[i + 2];

            // 三角形単位で判定をする。領域内の三角形のみ追加
            if ((_sites[0].Contains(v1)
                 || _sites[0].Contains(v2)
                 || _sites[0].Contains(v3)))
            {
                triangles.Add(v1);
                triangles.Add(v2);
                triangles.Add(v3);
                slicedMesh.SubIndices.Add(new List<int>());
                slicedMesh.AddTriangle(v1, v2, v3, 0, ref mesh);
                // 個々の処理自体は正しく動作しているように見えておかしな頂点の重複がある
            }
        }


        // for (int i = 0; i < triangles.Count; i++) // 領域内の三角形を可視化
        // {
        //     var c = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
        //
        //     var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //     obj.transform.localScale = Vector3.one * 0.05f;
        //     obj.transform.localPosition = mesh.vertices[triangles[i]];
        //     obj.GetComponent<MeshRenderer>().material.color = c;
        //
        //     var obj1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //     obj1.transform.localScale = Vector3.one * 0.05f;
        //     obj1.transform.localPosition = mesh.vertices[triangles[i + 1]];
        //     obj1.GetComponent<MeshRenderer>().material.color = c;
        //
        //     var obj2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //     obj2.transform.localScale = Vector3.one * 0.05f;
        //     obj2.transform.localPosition = mesh.vertices[triangles[i + 2]];
        //     obj2.GetComponent<MeshRenderer>().material.color = c;
        // }

        return slicedMesh.ToMesh();
    }
}