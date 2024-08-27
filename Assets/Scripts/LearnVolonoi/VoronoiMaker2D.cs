using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// 参考文献
// https://qiita.com/gis/items/c1762d9683355339ee07
// https://taq.hatenadiary.jp/entry/2022/12/25/160000#%E6%AD%A3%E6%94%BB%E6%B3%95

public struct Point2D
{
    public int x, y;
}

public static class VoronoiUtil2D
{
    public const int MAP_X = 64;
    public const int MAP_Y = 64;
    public const int VORONOI_COUNT_MAX = 1;

    // 点の座標と渡される座標の差分の二乗を返す
    public static int DistanceSqrt_D(Point2D point2D, int x, int y)
    {
        int xd = x - point2D.x;
        int yd = y - point2D.y;
        return (xd * xd) + (yd * yd);
    }
}

public class Voronoi2D
{
    private List<Point2D> _points = new();
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
        int w = VoronoiUtil2D.MAP_X - 20;
        int h = VoronoiUtil2D.MAP_Y - 20;
        for (int i = 0; i < count; i++)
        {
            var pnt = new Point2D();
            pnt.x = Random.Range(0, w) + 10;
            pnt.y = Random.Range(0, h) + 10;
            var c = new Color(Random.Range(0f, 1.0f), Random.Range(0f, 1.0f), Random.Range(0f, 1.0f));
            _points.Add(pnt);
            _colors.Add(c);
        }
    }

    void CreateSites()
    {
        int w = VoronoiUtil2D.MAP_X, h = VoronoiUtil2D.MAP_Y, d, ind, dmin;
        for (int hh = 0; hh < h; hh++)
        {
            for (int ww = 0; ww < w; ww++)
            {
                ind = -1;
                dmin = Int32.MaxValue;
                for (int it = 0; it < _points.Count; it++)
                {
                    Point2D p = _points[it];
                    d = VoronoiUtil2D.DistanceSqrt_D(p, ww, hh);
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
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    var obj =  GameObject.CreatePrimitive(PrimitiveType.Cube);
                    obj.transform.position = new Vector3(x + i, y + j, 0);
                    obj.GetComponent<MeshRenderer>().material.color = Color.black;
                    _objects.Add(obj);
                }
            }
        }
    }
}

public class VoronoiMaker2D : MonoBehaviour
{
    private Voronoi2D v = new Voronoi2D();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        v.Make(25);
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