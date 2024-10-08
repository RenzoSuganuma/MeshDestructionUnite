using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LearningVoronoi_Dalanuay
{
// 参考文献
// https://qiita.com/gis/items/c1762d9683355339ee07
// https://taq.hatenadiary.jp/entry/2022/12/25/160000#%E6%AD%A3%E6%94%BB%E6%B3%95

    public struct Point2D_L
    {
        public int x, y;

        public Point2D_L(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Point2D_L(float x, float y)
        {
            this.x = (int)x;
            this.y = (int)y;
        }
    }

    public static class VoronoiUtil2D_L
    {
        public const int MAP_X = 64;
        public const int MAP_Y = 64;
        public const int VORONOI_COUNT_MAX = 1;

        // 点の座標と渡される座標の差分の二乗を返す
        public static int DistanceSqrt_D(Point2D_L point2DL, int x, int y)
        {
            int xd = x - point2DL.x;
            int yd = y - point2DL.y;
            return (xd * xd) + (yd * yd);
        }

        public static bool TriangulatePolygonSubFuncInCyrcle
            (Vector2 p1, Vector2 p2, Vector2 p3, out Vector2 center)
        {
            center = Vector2Int.one * -1;

            if (Mathf.Abs(p1.y - p2.y) < float.Epsilon
                && Mathf.Abs(p2.y - p3.y) < float.Epsilon)
            {
                return false;
            }

            float m1, m2, mx1, mx2, my1, my2, xc, yc;
            if (Mathf.Abs(p2.y - p1.y) < float.Epsilon)
            {
                m2 = -(p3.x - p2.x) / (p3.y - p3.y);
                mx2 = (p2.x + p3.x) * .5f;
                my2 = (p2.y + p3.y) * .5f;
                xc = (p2.x + p1.x) * .5f;
                yc = m2 * (xc - mx2) + my2;
            }
            else if (Mathf.Abs(p3.y - p2.y) < float.Epsilon)
            {
                m1 = -(p2.x - p1.x) / (p2.y - p1.y);
                mx1 = (p1.x + p2.x) * .5f;
                my1 = (p1.y + p2.y) * .5f;
                xc = (p3.x + p2.x) * .5f;
                yc = m1 * (xc - mx1) + my1;
            }
            else
            {
                m1 = -(p2.x - p1.x) / (p2.y - p1.y);
                m2 = -(p3.x - p2.x) / (p3.y - p1.y);
                mx1 = (p1.x + p2.x) * .5f;
                mx2 = (p2.x + p3.x) * .5f;
                my1 = (p1.y + p2.y) * .5f;
                my2 = (p2.y + p3.y) * .5f;
                xc = (m1 * mx1 - m2 * mx2 - my2 - my1) / (m1 - m2);
                yc = m1 * (xc - mx1) + my1;
            }

            float dx = p2.x - xc;
            float dy = p2.y - yc;
            float rsqr = dx * dx + dy * dy;
            float dx1 = p1.x - xc;
            float dy1 = p1.y - yc;
            double dsqr = dx1 * dx1 + dy1 * dy1;
            var cond = (dsqr <= rsqr);
            if (cond)
            {
                center = new Vector2(xc, yc);
            }

            return cond;
        }
    }

    public class Voronoi2D_L
    {
        private List<Point2D_L> _points = new();
        private List<Point2D_L> _voronoiPoints = new();
        private List<Color> _colors = new();
        private List<GameObject> _objects = new();

        public void Make(int count)
        {
            CreatePointAndColor(count);
            CreateSites();
            SetSitesPoints();
        }

        public List<GameObject> Obj => _objects;

        // 点と色を配列へ追加
        void CreatePointAndColor(int count)
        {
            int w = VoronoiUtil2D_L.MAP_X - 20;
            int h = VoronoiUtil2D_L.MAP_Y - 20;
            for (int i = 0; i < count; i++)
            {
                var pnt = new Point2D_L();
                pnt.x = Random.Range(0, w) + 10;
                pnt.y = Random.Range(0, h) + 10;
                var c = new Color(Random.Range(0f, 1.0f), Random.Range(0f, 1.0f), Random.Range(0f, 1.0f));
                _points.Add(pnt);
                _colors.Add(c);
            }
        }

        void CreateSites()
        {
            int w = VoronoiUtil2D_L.MAP_X, h = VoronoiUtil2D_L.MAP_Y, d, ind, dmin;
            for (int hh = 0; hh < h; hh++)
            {
                for (int ww = 0; ww < w; ww++)
                {
                    ind = -1;
                    dmin = Int32.MaxValue;
                    for (int it = 0; it < _points.Count; it++)
                    {
                        Point2D_L p = _points[it];
                        d = VoronoiUtil2D_L.DistanceSqrt_D(p, ww, hh);
                        if (d < dmin) // 
                        {
                            dmin = d; // 一番近い母点との距離
                            ind = it; // 一番近い母点の添え字
                        }
                    }

                    if (ind > -1)
                    {
                        var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        obj.transform.position = new Vector3(ww, hh, 0);
                        obj.GetComponent<MeshRenderer>().material.color = _colors[ind];
                        _objects.Add(obj);
                    }
                }
            }
        }

        // 母点を描写
        void SetSitesPoints()
        {
            foreach (var point in _points)
            {
                int x = point.x, y = point.y;
                var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obj.transform.position = new Vector3(x, y, 0);
                obj.GetComponent<MeshRenderer>().material.color = Color.black;
                _objects.Add(obj);
            }
        }
    }

    public class VoronoiMaker2D_L : MonoBehaviour
    {
        private Voronoi2D_L v = new Voronoi2D_L();

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            v.Make(5);
        }

        // Update is called once per frame
        void Update()
        {
        }

        private void OnDisable()
        {
            v.Obj.ForEach(o =>
            {
                Destroy(o);
                o = null;
            });
        }
    }
}