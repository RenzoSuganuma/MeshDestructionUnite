using Random = UnityEngine.Random;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace SmasherDestruction.Editor
{
    /// <summary>
    /// ３次元ボロノイ区分けの機能を提供する
    /// </summary>
    public static class NawabariModoki
    {
        private static List<Vector3> _points = new();
        private static List<Color> _colors = new();
        private static List<List<int>> _sites = new();

        /// <summary>
        /// 断片化を実行
        /// </summary>
        /// <param name="pointCount"></param>
        /// <param name="victim"></param>
        /// <param name="insideMaterial"></param>
        /// <param name="mesh"></param>
        /// <param name="fragments"></param>
        public static void ExecuteFragmentation(
            int pointCount,
            GameObject victim,
            List<GameObject> fragments,
            Material insideMaterial,
            bool makeGap)
        {
            var mesh = victim.GetComponent<MeshFilter>().sharedMesh;
            CreatePointAndColor3D(pointCount, mesh);
            CreateSites3D(mesh);

            var duplicated = GameObject.Instantiate(victim);

            int ind = Random.Range(0, _points.Count);

            var p = _points[ind];

            // p 以外の母点のリストを求める
            Vector3[] othersArray = new Vector3[_points.Count];
            _points.CopyTo(othersArray);
            // p 以外の母点のリスト
            var otherList = othersArray.ToList();
            otherList.RemoveAt(ind); // 点pが残っているのでそれを除外

            // 最終的に残る断片
            // 切断前のオブジェクトを複製
            for (int k = 0; k < otherList.Count; k++)
            {
                var planeUp = (otherList[k] - p).normalized; // 抽出対象 領域 p は 法線の逆側。 
                var planePos = ((otherList[k] - p) + p) * .5f; // 平面を垂直二等分線として見立てる
                TsujigiriUtility.CutTheMesh(
                    victim,
                    fragments,
                    planePos,
                    planeUp,
                    insideMaterial,
                    makeGap);
            }


            // ここでリストを空っぽにしないと分割回数が増えるバグが発生する。
            _points.Clear();
            _colors.Clear();
            _sites.Clear();
        }

        // 点と色を配列へ追加
        private static void CreatePointAndColor3D(int pointCount, Mesh mesh)
        {
            var extents = mesh.bounds.extents;
            for (int i = 0; i < pointCount; i++)
            {
                var pnt = new Vector3();
                pnt.x = Random.Range(-extents.x, extents.x);
                pnt.y = Random.Range(-extents.y, extents.y);
                pnt.z = Random.Range(-extents.z, extents.z);
                var c =
                    new Color(Random.Range(0f, 1.0f),
                        Random.Range(0f, 1.0f),
                        Random.Range(0f, 1.0f));

                if (_points.Count > 5)
                {
                    var d = _points[_points.Count - 1] - pnt; // 最後に追加した母点との距離
                    var s = mesh.bounds.max - mesh.bounds.min; // メッシュのサイズのベクトル
                    if (d.magnitude <= (s / 2f).magnitude) // 距離がメッシュのサイズの 1/3以下の時にはオフセットをかける
                    {
                        if (Vector3.Dot(_points[_points.Count - 1], pnt) > 0)
                        {
                            if (Vector3.Dot(d, s) > 0)
                            {
                                pnt.x += Random.Range(0, extents.x);
                                pnt.y += Random.Range(0, extents.y);
                                pnt.z += Random.Range(0, extents.z);
                            }
                            else
                            {
                                pnt.x -= Random.Range(0, extents.x);
                                pnt.y -= Random.Range(0, extents.y);
                                pnt.z -= Random.Range(0, extents.z);
                            }
                        }
                        else
                        {
                            if (Vector3.Dot(d, s) > 0)
                            {
                                pnt.x -= Random.Range(0, extents.x);
                                pnt.y -= Random.Range(0, extents.y);
                                pnt.z -= Random.Range(0, extents.z);
                            }
                        }
                    }
                }

                _points.Add(pnt);
                _colors.Add(c);
                _sites.Add(new List<int>());
            }
        }

        private static void CreateSites3D(Mesh mesh)
        {
            int ind;
            float d, dmin;
            var vertices = mesh.vertices;

            for (int i = 0; i < mesh.vertices.Length; i++)
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
                    _sites[ind].Add(i);
                }
            }
        }
    }
}