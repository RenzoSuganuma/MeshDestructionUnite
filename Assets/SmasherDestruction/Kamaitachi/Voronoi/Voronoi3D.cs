using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.Layouts;
using Random = UnityEngine.Random;

namespace SmasherDestruction.Kamaitachi.Voronoi
{
    /// <summary>
    /// ボロノイのクラス
    /// </summary>
    public sealed class Voronoi3D
    {
        /// <summary>
        /// ローカル座標の各成分の値
        /// </summary>
        private List<VoronoiPoint3Df> _points = new();

        /// <summary>
        /// [母点のインデックス]{頂点のインデックスの配列}
        /// 領域
        /// </summary>
        private List<int>[] _sites = new List<int>[] { };

        /// <summary>
        /// [母点のインデックス]{それに対応した色}
        /// </summary>
        private List<Color> _colors = new();

        /// <summary>
        /// [母点のインデックス]{頂点番号の配列}
        /// </summary>
        private List<int>[] _borderVertices = new List<int>[] { };

        public List<int>[] BorderVertices => _borderVertices;

        public List<int>[] Sites => _sites;

        public List<Color> Colors => _colors;

        /// <summary>
        /// ３次元ボロノイを生成する...?
        /// </summary>
        public void CreateVoronoi(int count)
        {
            CreatePointAndColor(count);
            CreateSites();
            DrawSitesPoints();
        }

        /// <summary>
        /// ３次元ボロノイを生成する...?
        /// </summary>
        public void CreateVoronoi(int count, Mesh mesh)
        {
            CreatePointAndColor(count, mesh);
            CreateSites(mesh);
            EraseDuplicatedVertices();
            FindBorderVertices(mesh);
        }

        private void FindBorderVertices(Mesh mesh)
        {
            // 境い目の頂点を格納しておく配列の初期化はここでやる。領域の配列の初期化の後に必ず実行。:_sitesのプロパティを参照しているから
            _borderVertices = new List<int>[_sites.Length];
            for (int i = 0; i < _sites.Length; i++)
            {
                _borderVertices[i] = new List<int>();
            }

            // 同じ母点に属している頂点が構成する三角形以外の三角形が境目の三角形
            for (int i = 0; i < mesh.triangles.Length; i += 3)
            {
            }
        }

        /// <summary>
        /// 領域のインデックスを返す。見つからなかったら -1を返す。
        /// </summary>
        private int FindSite(int vertexIndex)
        {
            int ret = -1;

            for (int i = _sites.Length - 1; i >= 0; i--)
            {
                if (_sites[i].Contains(vertexIndex))
                {
                    ret = i;
                }
            }

            return ret;
        }

        private void EraseDuplicatedVertices()
        {
            // １頂点１領域になるように排他処理
            for (int i = _sites.Length - 1; i >= 0; i--)
            {
                // 上塗りを繰り返すような処理をしているので最後に上塗りをしたもの
                // を先に塗られたものから排除して。。。を繰り返す 
                var excludeIndices = _sites[i];
                for (int j = i - 1; j >= 0; j--)
                {
                    foreach (var index in excludeIndices)
                    {
                        _sites[j].Remove(index);
                    }
                }
            }
        }

        /// <summary>
        /// 点と色を配列へ追加
        /// </summary>
        void CreatePointAndColor(int count)
        {
            int mapX = VoronoiUtility3D.MAP_XYZ - 20;
            int mapY = VoronoiUtility3D.MAP_XYZ - 20;
            int mapZ = VoronoiUtility3D.MAP_XYZ - 20;

            for (int i = 0; i < count; i++)
            {
                var pnt = new VoronoiPoint3Df();
                pnt.x = Random.Range(0, mapX) + 10;
                pnt.y = Random.Range(0, mapY) + 10;
                pnt.z = Random.Range(0, mapZ) + 10;
                var c = new Color(Random.Range(0f, 1.0f), Random.Range(0f, 1.0f), Random.Range(0f, 1.0f));

                _points.Add(pnt);
                _colors.Add(c);
            }
        }

        /// <summary>
        /// 点と色を配列へ追加
        /// </summary>
        void CreatePointAndColor(int count, Mesh mesh)
        {
            var extents = mesh.bounds.extents;

            float offsetX = extents.x * .2f;
            float offsetY = extents.y * .2f;
            float offsetZ = extents.z * .2f;

            float mapXhalf = extents.x + offsetX;
            float mapYhalf = extents.y + offsetY;
            float mapZhalf = extents.z + offsetZ;

            for (int i = 0; i < count; i++)
            {
                var pnt = new VoronoiPoint3Df();
                pnt.x = Random.Range(-mapXhalf, mapXhalf) + offsetX / 2;
                pnt.y = Random.Range(-mapYhalf, mapYhalf) + offsetY / 2;
                pnt.z = Random.Range(-mapZhalf, mapZhalf) + offsetZ / 2;
                var c = new Color(Random.Range(0f, 1.0f), Random.Range(0f, 1.0f), Random.Range(0f, 1.0f));

                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.localPosition = new Vector3(pnt.x, pnt.y, pnt.z);
                cube.transform.localScale = Vector3.one * .1f;

                _points.Add(pnt);
                _colors.Add(c);
            }
        }

        /// <summary>
        /// 領域を生成する
        /// </summary>
        void CreateSites()
        {
            int mapX = VoronoiUtility3D.MAP_XYZ,
                mapY = VoronoiUtility3D.MAP_XYZ,
                mapZ = VoronoiUtility3D.MAP_XYZ,
                dis,
                ind,
                dmin;
            for (int d = 0; d < mapZ; d++) // depth
            {
                for (int h = 0; h < mapY; h++) // height
                {
                    for (int w = 0; w < mapX; w++) // width
                    {
                        ind = -1;
                        dmin = Int32.MaxValue;
                        for (int i = 0; i < _points.Count; i++)
                        {
                            VoronoiPoint3Df p = _points[i];
                            VoronoiPoint3D pntI = new VoronoiPoint3D((int)p.x, (int)p.y, (int)p.z);
                            dis = VoronoiUtility3D.DistanceSqrt(pntI, w, h, d);
                            if (dis < dmin)
                            {
                                dmin = dis; // 一番近い母点との距離
                                ind = i; // 一番近い母点の添え字
                            }
                        }

                        // 一番近い母点があるならば
                        if (ind > -1)
                        {
                            // ↓ ここで母点のある領域内にあるピクセルの描写をしている
                            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            obj.transform.position = new Vector3(w, h, d);
                            obj.GetComponent<MeshRenderer>().material.color = _colors[ind];
                            // ↑ ここで母点のある領域内にあるピクセルの描写をしている
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 渡されたメッシュから領域を生成する
        /// </summary>
        void CreateSites(Mesh mesh)
        {
            var extents = mesh.bounds.extents;
            float mapXhalf = extents.x;
            float mapYhalf = extents.y;
            float mapZhalf = extents.z;
            float dmin; // 母点との最短距離
            float dis; // 母点との距離
            int ind;

            var vertices = mesh.vertices;

            if (_sites.Length < _points.Count)
            {
                Array.Resize(ref _sites, _points.Count);
            }

            for (int i = 0; i < _sites.Length; i++)
            {
                _sites[i] = new List<int>();
            }

            for (int i = 0; i < _sites.Length; i++)
            {
                var ind_vert = i % _sites.Length;
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i];
                ind = -1;
                dmin = Int32.MaxValue;

                // 各母点に対してループ
                for (int k = 0; k < _points.Count; k++)
                {
                    VoronoiPoint3Df p = _points[k];
                    float x = vertex.x;
                    float y = vertex.y;
                    float z = vertex.z;
                    dis = VoronoiUtility3D.DistanceSqrt(p, x, y, z);

                    if (dis < dmin)
                    {
                        dmin = dis; // 一番近い母点との距離
                        ind = k; // 一番近い母点の添え字
                    }

                    // 一番近い母点があるならば
                    if (ind > -1)
                    {
                        if (_sites[ind] is not null && !_sites[ind].Contains(i))
                        {
                            _sites[ind].Add(i);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 母点を描写
        /// </summary>
        void DrawSitesPoints()
        {
            // ↓ 最終的にはいらないデバッグ機能
            foreach (var point in _points)
            {
                int x = (int)point.x, y = (int)point.y, z = (int)point.z;

                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        for (int k = -1; k < 2; k++)
                        {
                            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            obj.transform.position = new Vector3(x + i, y + j, z + k);
                            obj.GetComponent<MeshRenderer>().material.color = Color.black;
                        }
                    }
                }
            }
            // ↑ 最終的にはいらないデバッグ機能
        }
    }
}