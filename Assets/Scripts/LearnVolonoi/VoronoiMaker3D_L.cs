using System;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LearningVoronoi_Dalanuay
{
    // 参考文献
    // https://qiita.com/gis/items/c1762d9683355339ee07
    // https://taq.hatenadiary.jp/entry/2022/12/25/160000#%E6%AD%A3%E6%94%BB%E6%B3%95

    public struct Point3D_L
    {
        public int x, y, z;
    }

    public static class VoronoiUtil3D_L
    {
        public const int MAP_XYZ = 32;
        public const int VORONOI_COUNT_MAX = 1;

        // 点の座標と渡される座標の差分の二乗を返す
        public static int DistanceSqrt_D3D(Point3D_L point3DL, int x, int y, int z)
        {
            int xd = x - point3DL.x;
            int yd = y - point3DL.y;
            int zd = z - point3DL.z;
            return (xd * xd) + (yd * yd) + (zd * zd);
        }
    }

    public class Voronoi3D_L
    {
        private List<Point3D_L> _points = new();
        private List<Color> _colors = new();
        private List<GameObject> _createdObjs = new();

        public void Make(int count)
        {
            CreatePointAndColor3D(count);
            CreateSites3D();
            SetSitesPoints3D();
        }

        public List<GameObject> CreatedObjs => _createdObjs;

        // 点と色を配列へ追加
        void CreatePointAndColor3D(int count)
        {
            int mapX = VoronoiUtil3D_L.MAP_XYZ - 20;
            int mapY = VoronoiUtil3D_L.MAP_XYZ - 20;
            int mapZ = VoronoiUtil3D_L.MAP_XYZ - 20;
            for (int i = 0; i < count; i++)
            {
                var pnt = new Point3D_L();
                pnt.x = Random.Range(0, mapX) + 10;
                pnt.y = Random.Range(0, mapY) + 10;
                pnt.z = Random.Range(0, mapZ) + 10;
                var c = new Color(Random.Range(0f, 1.0f), Random.Range(0f, 1.0f), Random.Range(0f, 1.0f));

                _points.Add(pnt);
                _colors.Add(c);
            }
        }

        void CreateSites3D()
        {
            int mapX = VoronoiUtil3D_L.MAP_XYZ, mapY = VoronoiUtil3D_L.MAP_XYZ, mapZ = VoronoiUtil3D_L.MAP_XYZ, d, ind, dmin;
            for (int dd = 0; dd < mapZ; dd++)
            {
                for (int hh = 0; hh < mapY; hh++)
                {
                    for (int ww = 0; ww < mapX; ww++)
                    {
                        ind = -1;
                        dmin = Int32.MaxValue;
                        for (int it = 0; it < _points.Count; it++)
                        {
                            Point3D_L p = _points[it];
                            d = VoronoiUtil3D_L.DistanceSqrt_D3D(p, ww, hh, dd);
                            if (d < dmin) // 
                            {
                                dmin = d; // 一番近い母点との距離
                                ind = it; // 一番近い母点の添え字
                            }
                        }

                        if (ind > -1) // 
                        {
                            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            obj.transform.position = new Vector3(ww, hh, dd);
                            obj.GetComponent<MeshRenderer>().material.color = _colors[ind];
                            _createdObjs.Add(obj);
                        }
                    }
                }
            }
        }

        // 母点を描写
        void SetSitesPoints3D()
        {
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
        }
    }


    /// <summary>
    /// ボロノイ キューブを生成する
    /// </summary>
    public class VoronoiMaker3D_L : MonoBehaviour
    {
        private Voronoi3D_L diagram = new();

        private void Start()
        {
            diagram.Make(VoronoiUtil3D_L.MAP_XYZ);
        }

        private void OnDisable()
        {
            diagram.CreatedObjs.ForEach(o =>
            {
                Destroy(o);
                o = null;
            });
        }
    }
}