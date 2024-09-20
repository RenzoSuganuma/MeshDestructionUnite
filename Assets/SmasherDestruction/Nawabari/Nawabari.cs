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

        for (int i = 0; i < ms.Length; i++)
        {
            var obj = new GameObject($"sphere{i}", new[] { typeof(MeshFilter), typeof(MeshRenderer) });
            obj.GetComponent<MeshFilter>().sharedMesh = ms[i].ToMesh();
        }

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

    // $ ↓ 領域１しかまだ抽出できない ↓ $
    private static SeperatedMesh[] GetSeparatedMeshes(Mesh mesh)
    {
        var slicedMesh = new SeperatedMesh[_sites.Count];

        for (int i = 0; i < _sites.Count; i++)
        {
            slicedMesh[i] = new SeperatedMesh();
        }

        // 三角形単位でループを実行
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            var v1 = mesh.triangles[i]; // 頂点１
            var v2 = mesh.triangles[i + 1]; // 頂点２
            var v3 = mesh.triangles[i + 2]; // 頂点３

            // 境界線を構成する頂点の抽出
            // 条件 
            // v1,v2,v3 のうち少なくとも１つほかの２つと所属する領域が違うなら
            // その３つの頂点は境界線を構成する
            // パターン （条件）
            // v1->v2,v3 (1) | v2->v1,v3 (2) | v3->v1,v2 (3)
            // nether = v1 -> v2 | v1 -> v3 | v2 -> v3 ３つとも違う所属 (4)

            // 条件
            bool cond1, cond2, cond3, nether;
            // 各頂点の所属領域
            int s1 = 0, s2 = 0, s3 = 0;

            // 各頂点が所属する領域を求める
            for (int j = 0; j < _sites.Count; j++)
            {
                _borders.Add(new List<int>());
                var site = _sites[j];
                if (site.Contains(v1))
                {
                    s1 = j;
                }

                if (site.Contains(v2))
                {
                    s2 = j;
                }

                if (site.Contains(v3))
                {
                    s3 = j;
                }
            }

            // パターンを求める
            nether = (s1 != s2 && s1 != s3 && s2 != s3); // (4)
            cond1 = (s2 == s3 && s2 != s1); // (1)
            cond2 = (s1 == s3 && s1 != s2); // (2)
            cond3 = (s1 == s2 && s1 != s3); // (3)

            // NOTE: ２つの領域に存在する辺が領域の境界線を構成するはず

            for (int j = 0; j < _sites.Count; j++)
            {
                // 三角形単位で判定をする。領域内の三角形のみ追加
                if ((_sites[j].Contains(v1)
                     || _sites[j].Contains(v2)
                     || _sites[j].Contains(v3)))
                {
                    // ここで境界線の可視化
                    if (cond1 || cond2 || cond3 || nether)
                    {
                        // var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        // obj.transform.localScale = Vector3.one * 0.05f;
                        // obj.transform.localPosition = mesh.vertices[v1];
                        //
                        // var obj1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        // obj1.transform.localScale = Vector3.one * 0.05f;
                        // obj1.transform.localPosition = mesh.vertices[v2];
                        //
                        // var obj2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        // obj2.transform.localScale = Vector3.one * 0.05f;
                        // obj2.transform.localPosition = mesh.vertices[v3];

                        _borders[j].Add(v1);
                        _borders[j].Add(v2);
                        _borders[j].Add(v3);
                    }
                    // () --- ()

                    slicedMesh[j].SubIndices.Add(new List<int>());
                    slicedMesh[j].AddTriangle(v1, v2, v3, 0, ref mesh);
                    // 個々の処理自体は正しく動作しているように見えておかしな頂点の重複がある
                }
            }

            // 各領域を構成する頂点の抽出
            List<List<int>> confirmedborders = new();

            // extracted vertex site
            var evs1 = -1;
            var evs2 = -1;
            var evs3 = -1;
            for (int j = 0; j < _borders.Count; j++)
            {
                confirmedborders.Add(new List<int>());

                for (int k = 0; k < _borders[j].Count; k += 3)
                {
                    // extracted vertex
                    var ev1 = _borders[j][k];
                    var ev2 = _borders[j][k + 1];
                    var ev3 = _borders[j][k + 2];

                    // 初回の初期化
                    if (evs1 is -1 && _borders[j].Contains(ev1))
                    {
                        evs1 = j;
                    }
                    // 前回検索出来た頂点１の所属領域が今回の検索でひっかかったものと違ったら
                    else if (evs1 != j && _borders[j].Contains(ev1))
                    {
                        if (!confirmedborders[j].Contains(ev1))
                        {
                            confirmedborders[j].Add(ev1);
                        }

                        if (!confirmedborders[evs1].Contains(ev1))
                        {
                            confirmedborders[evs1].Add(ev1);
                        }
                    }

                    if (evs2 is -1 && _borders[j].Contains(ev2))
                    {
                        evs2 = j;
                    }
                    else if (evs2 != j && _borders[j].Contains(ev2))
                    {
                        if (!confirmedborders[j].Contains(ev2))
                        {
                            confirmedborders[j].Add(ev2);
                        }

                        if (!confirmedborders[evs2].Contains(ev2))
                        {
                            confirmedborders[evs2].Add(ev2);
                        }
                    }

                    if (evs3 is -1 && _borders[j].Contains(ev3))
                    {
                        evs3 = j;
                    }
                    else if (evs3 != j && _borders[j].Contains(ev3))
                    {
                        if (!confirmedborders[j].Contains(ev3))
                        {
                            confirmedborders[j].Add(ev3);
                        }

                        if (!confirmedborders[evs3].Contains(ev3))
                        {
                            confirmedborders[evs3].Add(ev3);
                        }
                    }
                }
            }

            // デバッグ
            foreach (var confirmedborder in confirmedborders)
            {
                foreach (var i1 in confirmedborder)
                {
                    var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    obj.transform.localScale = Vector3.one * 0.05f;
                    obj.transform.localPosition = mesh.vertices[i1];
                }
            }
            
            confirmedborders.Clear();
        }

        return slicedMesh;
    }
}