using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

// 参考資料
// https://www.fukui-nct.ac.jp/wp/wp-content/uploads/2021/10/15-01-maegawa.pdf

public class VolonoiDiagram : MonoBehaviour
{
    // 範囲
    [SerializeField] private Vector3 _bounds;

    // 母点
    [SerializeField] private int _siteCount;

    // 二等分線の長さ
    [SerializeField] private int _bisectorLen;

    // 母点 の 座標
    private List<Vector3> _siteCoords = new List<Vector3>();

    // 母点 と 母点 の座標のリスト
    private List<Tuple<Vector3, Vector3>> _edgesVectors = new List<Tuple<Vector3, Vector3>>();

    // 二等分線とその始点
    private List<Tuple<Vector3, Vector3>> _bisectorVectorAndOrigin = new List<Tuple<Vector3, Vector3>>();

    private void Start()
    {
        CreateSites();
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 50), "Find Edges"))
        {
            FindEdges(_siteCount);
        }

        if (GUI.Button(new Rect(0, 50, 100, 50), "Find Bisector"))
        {
            FindBisector();
        }
    }

    private void CreateSites()
    {
        for (int i = 0; i < _siteCount; i++)
        {
            var pos = new Vector3(Random.Range(-_bounds.x / 2, _bounds.x / 2), 0,
                Random.Range(-_bounds.z / 2, _bounds.z / 2));

            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = pos;
            cube.transform.localScale = Vector3.one * .2f;
            _siteCoords.Add(pos);
        }
    }

    private void FindEdges(int siteCounts)
    {
        if (siteCounts <= 0) return;

        // 母点 情報のコピー
        Vector3[] copiedSites = new Vector3[_siteCount];
        _siteCoords.CopyTo(copiedSites);

        // 現在参照している母点以外の母点を取得
        var exeptedSites = copiedSites.ToList();
        exeptedSites.RemoveAt(siteCounts - 1);

        // 現在参照している母点とそれ以外すべての辺を保存する
        foreach (var site in exeptedSites)
        {
            _edgesVectors.Add(new Tuple<Vector3, Vector3>(_siteCoords[siteCounts - 1], site));
        }

        FindEdges(siteCounts - 1);
    }

    private void FindBisector()
    {
        for (int i = 0; i < _edgesVectors.Count; i++)
        {
            // 母点どうしを結ぶ 辺
            var edge_vec = (_edgesVectors[i].Item1 - _edgesVectors[i].Item2);
            // 二等分点
            var bisector_point = _edgesVectors[i].Item2 + (edge_vec / 2);
            // 二等分線
            var cross_vec_bisector_vec = Vector3.Cross(edge_vec, Vector3.down).normalized * _bisectorLen;

            _bisectorVectorAndOrigin.Add(new Tuple<Vector3, Vector3>(cross_vec_bisector_vec, bisector_point));
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawCube(transform.position, _bounds);

        foreach (var vector in _edgesVectors)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(vector.Item1, vector.Item2);
        }

        foreach (var tuple in _bisectorVectorAndOrigin)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(tuple.Item2, tuple.Item1);
            Gizmos.DrawLine(tuple.Item2, -tuple.Item1);
        }
    }
}