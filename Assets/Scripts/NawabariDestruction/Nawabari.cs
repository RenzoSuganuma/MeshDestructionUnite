using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NawabariDestruction
{
    /// <summary>
    /// ３次元ボロノイ区分けの機能を提供する
    /// </summary>
    public class Nawabari
    {
        private List<NawabariPoint> _points = new();
        private List<Color> _colors = new();
        private List<GameObject> _createdObjs = new();

        public void Make(int count, Mesh mesh)
        {
            CreatePointAndColor3D(count, mesh);
            CreateSites3D(mesh);
        }

        public List<GameObject> CreatedObjs => _createdObjs;

        // 点と色を配列へ追加
        void CreatePointAndColor3D(int count, Mesh mesh)
        {
            var extents = mesh.bounds.extents;
            for (int i = 0; i < count; i++)
            {
                var pnt = new NawabariPoint();
                pnt.x = Random.Range(-extents.x, extents.x);
                pnt.y = Random.Range(-extents.y, extents.y);
                pnt.z = Random.Range(-extents.z, extents.z);
                var c = new Color(Random.Range(0f, 1.0f), Random.Range(0f, 1.0f), Random.Range(0f, 1.0f));

                _points.Add(pnt);
                _colors.Add(c);
            }
        }

        void CreateSites3D(Mesh mesh)
        {
            int ind;
            float d, dmin;
            var vertices = mesh.vertices;

            foreach (var vertex in vertices)
            {
                ind = -1;
                dmin = Int32.MaxValue;
                for (int it = 0; it < _points.Count; it++)
                {
                    NawabariPoint p = _points[it];
                    d = NawabariUtility.DistanceSqrt(p, vertex.x, vertex.y, vertex.z);
                    if (d < dmin)
                    {
                        dmin = d; // 一番近い母点との距離
                        ind = it; // 一番近い母点の添え字
                    }
                }

                if (ind > -1)
                {
                    var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    obj.transform.position = new Vector3(vertex.x, vertex.y, vertex.z);
                    obj.transform.localScale = Vector3.one * .05f;
                    obj.GetComponent<MeshRenderer>().material.color = _colors[ind];
                    _createdObjs.Add(obj);
                }
            }
        }
    }
}