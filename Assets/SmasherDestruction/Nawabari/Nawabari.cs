using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SmasherDestruction.Datas;
using SmasherDestruction.Editor;
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

    /// <summary>
    /// 母点のリスト
    /// </summary>
    public static List<Vector3> Points => _points;

    /// <summary>
    /// 領域のリスト。インデックスで領域に所属する
    /// 頂点のインデックスのリストにアクセスできる
    /// </summary>
    public static List<List<int>> Sites => _sites;


    public static void CreateFragmentedMeshes(
        int count,
        GameObject sourceObject,
        Material insideMaterial,
        bool makeGap,
        List<GameObject> resultFragments)
    {
        ClearAll();
        var mesh = sourceObject.GetComponent<MeshFilter>().sharedMesh;
        CreatePoints(mesh.bounds.extents, count);
        CreateSites(mesh.vertices);
        ExecuteFragmentation(sourceObject, insideMaterial, makeGap,resultFragments);
    }

    private static void ExecuteFragmentation(
        GameObject sourceObject,
        Material insideMaterial,
        bool makeGap,
        List<GameObject> resultFragments)
    {
        GameObject[] copiedSource = new GameObject[_points.Count];

        for (int i = 0; i < _points.Count; i++)
        {
            copiedSource[i] = GameObject.Instantiate(sourceObject);

            resultFragments.Add(SeparateByVoronoiEdge(copiedSource[i], insideMaterial, i, makeGap));
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

    /// <summary>
    /// 渡されたオブジェクトをしてされた領域のインデックスの領域の通りに抽出
    /// </summary>
    /// <param name="sourceObject">切断対象のオブジェクト</param>
    /// <param name="insideMaterial">内側のマテリアル</param>
    /// <param name="extractTargetSitesIndex">抽出する領域のインデックス</param>
    /// <param name="makeGap">隙間を空けるか</param>
    private static GameObject SeparateByVoronoiEdge(GameObject sourceObject, Material insideMaterial,
        int extractTargetSitesIndex, bool makeGap)
    {
        // NOTE: ここで指定された領域の抽出を実行している

        GameObject fragment = sourceObject;

        // 指定された領域の境界線を求める
        for (int i = 0; i < _points.Count; i++)
        {
            if (extractTargetSitesIndex == i) continue;

            // 境界線を切断に使う平面として見立てる
            var planeNormal = (_points[i] - _points[extractTargetSitesIndex]).normalized;
            var planePos = Vector3.Lerp(_points[extractTargetSitesIndex], _points[i], 0.5f);
            if (i == 0)
            {
                var tmp = Tsujigiri.CutMesh(sourceObject, planePos, planeNormal, insideMaterial, makeGap);
                fragment = tmp[0];
                // 余分なメッシュは破棄する
                GameObject.DestroyImmediate(tmp[1]);
            }

            // ↑ と考え方は同じ
            var tmp1 = Tsujigiri.CutMesh(fragment, planePos, planeNormal, insideMaterial, makeGap);
            fragment = tmp1[0];
            GameObject.DestroyImmediate(tmp1[1]);
        }

        fragment.name = $"ExtractedMesh{extractTargetSitesIndex}";

        return fragment;
    }
}