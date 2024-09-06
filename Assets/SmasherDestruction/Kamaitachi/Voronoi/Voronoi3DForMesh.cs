using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SmasherDestruction.Kamaitachi.Voronoi
{
    /// <summary>
    /// ボロノイのクラス
    /// </summary>
    public sealed class Voronoi3DForMesh
    {
        private List<VoronoiPoint3D> _points = new();
        private List<Color> _colors = new();

        // ↓ 生成したキューブを格納しておく配列【実際（ツールの機能）にはいらない変数】
        private List<GameObject> _createdObjs = new();
        // ↑ 生成したキューブを格納しておく配列【実際（ツールの機能）にはいらない変数】

        // 母点のインデックスに対応した頂点群の配列
        private Dictionary<int, List<Vector3>> _verticesNearSite = new();

        /// <summary>
        /// ３次元ボロノイを生成する...?
        /// </summary>
        public void CreateVoronoi(int count)
        {
            CreatePointAndColor(count);
            CreateSites();
            DrawSitesPoints();
        }

        public void CreateVoronoi(int count, Mesh mesh)
        {
            CreatePointAndColor(count, mesh);
            CreateSites(mesh);
            DrawSitesPoints();
        }

        public List<GameObject> CreatedObjs => _createdObjs;

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
                var pnt = new VoronoiPoint3D();
                pnt.x = Random.Range(0, mapX) + 10;
                pnt.y = Random.Range(0, mapY) + 10;
                pnt.z = Random.Range(0, mapZ) + 10;
                var c = new Color(Random.Range(0f, 1.0f), Random.Range(0f, 1.0f), Random.Range(0f, 1.0f));

                _points.Add(pnt);
                _colors.Add(c);
            }
        }

        void CreatePointAndColor(int count, Mesh mesh)
        {
            int mapX = (int)mesh.bounds.size.x;
            int mapY = (int)mesh.bounds.size.y;
            int mapZ = (int)mesh.bounds.size.z;
            for (int i = 0; i < count; i++)
            {
                var pnt = new VoronoiPoint3D();
                pnt.x = Random.Range(0, mapX);
                pnt.y = Random.Range(0, mapY);
                pnt.z = Random.Range(0, mapZ);
                var c = new Color(Random.Range(0f, 1.0f), Random.Range(0f, 1.0f), Random.Range(0f, 1.0f));

                _points.Add(pnt);
                _colors.Add(c);
            }
        }

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
                            VoronoiPoint3D p = _points[i];
                            dis = VoronoiUtility3D.DistanceSqrt(p, w, h, d);
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
                            _createdObjs.Add(obj);
                            // ↑ ここで母点のある領域内にあるピクセルの描写をしている
                        }
                    }
                }
            }
        }

        public void CreateSites(Mesh mesh)
        {
            int mapX = (int)mesh.bounds.size.x,
                mapY = (int)mesh.bounds.size.y,
                mapZ = (int)mesh.bounds.size.z,
                dis,
                ind,
                dmin;

            for (int d = -mapZ; d < mapZ; d++) // depth
            {
                for (int h = -mapY; h < mapY; h++) // height
                {
                    for (int w = -mapX; w < mapX; w++) // width
                    {
                        ind = -1;
                        dmin = Int32.MaxValue;
                        for (int i = 0; i < _points.Count; i++)
                        {
                            VoronoiPoint3D p = _points[i];
                            dis = VoronoiUtility3D.DistanceSqrt(p, w, h, d);
                            if (dis < dmin)
                            {
                                dmin = dis; // 一番近い母点との距離
                                ind = i; // 一番近い母点の添え字
                            }
                        }

                        // 一番近い母点があるならば
                        if (ind > -1)
                        {
                            var v = new Vector3(w, h, d);
                            try
                            {
                                if (_verticesNearSite.ContainsKey(ind))
                                {
                                    _verticesNearSite[ind].Add(v);
                                }
                                else
                                {
                                    _verticesNearSite.Add(ind, new List<Vector3>() { v });
                                }
                            }
                            catch (ArgumentOutOfRangeException e)
                            {
                                Debug.Log($"OutOfRange,index{ind},len{_verticesNearSite.Count}");
                            }
                        }
                    }
                }
            }

            var obj = new GameObject();
            var vertices = _verticesNearSite[0];
            List<int> vertsIndices = new();
            foreach (var v in vertices)
            {
                var l = mesh.vertices.ToList();
                var i = l.IndexOf(v);
                vertsIndices.Add(i);
            }

            var triangles =
                mesh.triangles.ToList().Where(i => vertsIndices.Contains(i)).ToArray();

            Mesh m = new();
            m.vertices = vertices.ToArray();
            m.triangles = triangles;

            var mf = obj.AddComponent<MeshFilter>();
            mf.sharedMesh = m;
        }

        /// <summary>
        /// 母点を描写
        /// </summary>
        void DrawSitesPoints()
        {
            return;
            // ↓ 最終的にはいらないデバッグ機能
            foreach (var point in _points)
            {
                int x = point.x, y = point.y, z = point.z;

                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        for (int k = -1; k < 2; k++)
                        {
                            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            obj.transform.position = new Vector3(x + i, y + j, z + k);
                            obj.GetComponent<MeshRenderer>().material.color = Color.black;
                            _createdObjs.Add(obj);
                        }
                    }
                }
            }
            // ↑ 最終的にはいらないデバッグ機能
        }
    }
}