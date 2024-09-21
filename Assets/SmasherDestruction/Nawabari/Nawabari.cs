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

    private static List<Vector3> _capVerticesChecked = new();
    private static List<Vector3> _capVerticesPolygon = new();

    /// <summary>
    /// 母点のリスト
    /// </summary>
    public static List<Vector3> Points => _points;

    /// <summary>
    /// 領域のリスト。インデックスで領域に所属する
    /// 頂点のインデックスのリストにアクセスできる
    /// </summary>
    public static List<List<int>> Sites => _sites;


    public static void CreateFragmentedMeshes(int count, Mesh mesh)
    {
        ClearAll();

        CreatePoints(mesh.bounds.extents, count);
        CreateSites(mesh.vertices);
        var ms = GetSeparatedMeshes(mesh);

        // 次に切断面を形成する処理を実行すればひとまず完成
        for (int j = 0; j < ms.Length; j++)
        {
            FindVerticesMakeNewFace(mesh, ref ms[j]);
        }

        for (int i = 0; i < ms.Length; i++)
        {
            var obj = new GameObject($"sphere{i}", new[] { typeof(MeshFilter), typeof(MeshRenderer) });
            obj.GetComponent<MeshFilter>().sharedMesh = ms[i].ToMesh();
            Debug.Log($"vertices {ms[i].Vertices.Count} border vertices {ms[i].BorderVerticesIndices.Count}");
        }
    }

    private static void ClearAll()
    {
        _points.Clear();
        _sites.Clear();
        _borders.Clear();
        _capVerticesChecked.Clear();
        _capVerticesPolygon.Clear();
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
                    if (s1 == j && !seperatedMeshes[j].BorderVerticesIndices.Contains(v1))
                    {
                        seperatedMeshes[j].BorderVerticesIndices.Add(v1);
                        seperatedMeshes[j].BorderVerticesPos.Add(mesh.vertices[v1]);
                    }

                    if (s2 == j && !seperatedMeshes[j].BorderVerticesIndices.Contains(v2))
                    {
                        seperatedMeshes[j].BorderVerticesIndices.Add(v2);
                        seperatedMeshes[j].BorderVerticesPos.Add(mesh.vertices[v2]);
                    }

                    if (s3 == j && !seperatedMeshes[j].BorderVerticesIndices.Contains(v3))
                    {
                        seperatedMeshes[j].BorderVerticesIndices.Add(v3);
                        seperatedMeshes[j].BorderVerticesPos.Add(mesh.vertices[v3]);
                    }
                }
            }
        }

        return seperatedMeshes;
    }

    /// <summary>
    /// 切断処理で新たに生成された頂点に基づいてカット面の生成をする
    /// </summary>
    private static void FindVerticesMakeNewFace(Mesh source, ref SeperatedMesh seperatedMesh)
    {
        _capVerticesChecked.Clear();

        if (seperatedMesh.BorderVerticesPos.Count % 2 != 0
            && seperatedMesh.BorderVerticesIndices.Count % 2 != 0)
        {
            var count = seperatedMesh.BorderVerticesPos.Count;
            var newpos = Vector3.Lerp(seperatedMesh.BorderVerticesPos[count - 1],
                seperatedMesh.BorderVerticesPos[count - 2], 0.5f);
            var newind = source.vertices.Length;
            
            seperatedMesh.BorderVerticesPos.Add(newpos);
        }

        for (int i = 0; i < seperatedMesh.BorderVerticesIndices.Count; i+=2)
        {
            // 調査済みはとばす
            if (_capVerticesChecked.Contains(seperatedMesh.BorderVerticesPos[i]))
            {
                continue;
            }

            _capVerticesPolygon.Clear();

            _capVerticesPolygon.Add(seperatedMesh.BorderVerticesPos[i]);
            _capVerticesPolygon.Add(seperatedMesh.BorderVerticesPos[i + 1]);

            _capVerticesChecked.Add(seperatedMesh.BorderVerticesPos[i]);
            _capVerticesChecked.Add(seperatedMesh.BorderVerticesPos[i + 1]);

            bool isDone = false;
            while (!isDone)
            {
                isDone = true;

                for (int k = 0; k < seperatedMesh.BorderVerticesPos.Count; k += 2)
                {
                    // 【新頂点のペアを探す】
                    if (seperatedMesh.BorderVerticesPos[k] == _capVerticesPolygon[_capVerticesPolygon.Count - 1] &&
                        !_capVerticesChecked.Contains(seperatedMesh.BorderVerticesPos[k + 1]))
                    {
                        // ペアの頂点を見つけたらポリゴン配列へ追加、次のループを回す。
                        isDone = false;
                        _capVerticesPolygon.Add(seperatedMesh.BorderVerticesPos[k + 1]);
                        _capVerticesChecked.Add(seperatedMesh.BorderVerticesPos[k + 1]);
                    }
                    else if (seperatedMesh.BorderVerticesPos[k + 1] ==
                             _capVerticesPolygon[_capVerticesPolygon.Count - 1] &&
                             !_capVerticesChecked.Contains(seperatedMesh.BorderVerticesPos[k]))
                    {
                        isDone = false;
                        _capVerticesPolygon.Add(seperatedMesh.BorderVerticesPos[k]);
                        _capVerticesChecked.Add(seperatedMesh.BorderVerticesPos[k]);
                    }
                }
            }

            // ポリゴン形成
            FillFaceFromVertices(_capVerticesPolygon, ref seperatedMesh);
        }
    }

    /// <summary>
    /// 渡された頂点の配列の基づいてポリゴンの形成をする
    /// </summary>
    /// <param name="vertices">ポリゴンの頂点リスト</param>
    private static void FillFaceFromVertices(List<Vector3> vertices, ref SeperatedMesh seperatedMesh)
    {
        Vector3 center = Vector3.zero; // 中心と各頂点を結んで三角形を形成するのでこれを定義

        foreach (var vertex in vertices)
        {
            var c = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            c.transform.localScale =Vector3.one* 0.05f;
            c.transform.position = vertex;
        }

        foreach (var vert in vertices)
        {
            center += vert;
        }

        center /= vertices.Count;

        Vector3 upward = Vector3.zero;
        var blade = new Plane();
        blade.Set3Points(
            vertices[0],
            vertices[vertices.Count / 2],
            vertices[vertices.Count - 1]);

        // 90度回転。 平面の左側を上とする
        upward.x = blade.normal.y;
        upward.y = -blade.normal.x;
        upward.z = blade.normal.z;

        Vector3 left = Vector3.Cross(blade.normal, upward);

        Vector3 displacement = Vector3.zero;
        Vector3 newUv1 = Vector3.zero;
        Vector3 newUv2 = Vector3.zero;

        for (int i = 0; i < vertices.Count; i++)
        {
            // 中心からの頂点へのベクトル
            displacement = vertices[i] - center;

            // uv値をとる
            newUv1 = Vector3.zero;
            newUv1.x = .5f + Vector3.Dot(displacement, left);
            newUv1.y = .5f + Vector3.Dot(displacement, upward);
            newUv1.z = .5f + Vector3.Dot(displacement, blade.normal);

            // 最後の頂点は最初の頂点を利用するのでインデックスを循環させる
            displacement = vertices[(i + 1) % vertices.Count] - center;

            newUv2 = Vector3.zero;
            newUv2.x = .5f + Vector3.Dot(displacement, left);
            newUv2.y = .5f + Vector3.Dot(displacement, upward);
            newUv2.z = .5f + Vector3.Dot(displacement, blade.normal);

            seperatedMesh.AddTriangle(
                new Vector3[]
                {
                    vertices[i],
                    vertices[(i + 1) % vertices.Count],
                    center
                },
                new Vector3[]
                {
                    -blade.normal,
                    -blade.normal,
                    -blade.normal
                },
                new Vector2[]
                {
                    newUv1,
                    newUv2,
                    Vector2.one * .5f
                },
                -blade.normal,
                // カット面をサブメッシュとして登録
                seperatedMesh.SubIndices.Count - 1
            );
        }
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