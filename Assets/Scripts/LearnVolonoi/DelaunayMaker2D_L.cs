using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LearningVoronoi_Dalanuay
{


// https://gist.github.com/miketucker/3795318

    /// <summary>
    /// ドロネー三角形
    /// </summary>
    public struct DelaunayTriangle2D_L
    {
        public int p1, p2, p3;

        public DelaunayTriangle2D_L(int p1, int p2, int p3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;
        }
    }

    /// <summary>
    /// ドロネー辺
    /// </summary>
    public sealed class DelaunayEdge2D_L
    {
        public int p1, p2;

        public DelaunayEdge2D_L() : this(0, 0)
        {
        }

        public DelaunayEdge2D_L(int p1, int p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }

        public bool Equals(DelaunayEdge2D_L other)
        {
            bool res = ((this.p1 == other.p1) && (this.p2 == other.p2)
                        || (this.p1 == other.p2) && (this.p2 == other.p1));
            return res;
        }
    }

    public sealed class DelaunayTriangulator2D_L
    {
        public bool TriangulatePolygonSubFuncInCyrcle
            (Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
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
            dx = p1.x - xc;
            dy = p1.y - yc;
            double dsqr = dx * dx + dy * dy;
            return (dsqr <= rsqr);
        }

        public void CreateInfluencePolygon(Vector2[] XZofVertices)
        {
            Vector3[] vertices = new Vector3[XZofVertices.Length];
            for (int i = 0; i < XZofVertices.Length; i++)
            {
                vertices[i] = new Vector3(XZofVertices[i].x, 0, XZofVertices[i].y);
            }

            foreach (var v in vertices)
            {
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = v;
                cube.transform.localScale = Vector3.one * .5f;
            }

            // ポリゴンとして構成できる頂点インデックス配列を返す。
            var polygon = TriangulatePolygon(XZofVertices);

            for (int i = 0; i < polygon.Length - 3; i += 3)
            {
                GameObject obj1 = new GameObject($"Mesh{i + 1}");
                GameObject obj2 = new GameObject($"Mesh{i + 2}");
                GameObject obj3 = new GameObject($"Mesh{i + 3}");
                var p1 = vertices[polygon[i]];
                var p2 = vertices[polygon[i + 1]];
                var p3 = vertices[polygon[i + 2]];

                var lr = obj1.AddComponent<LineRenderer>();
                lr.startWidth = .1f;
                lr.endWidth = .1f;
                lr.SetPositions(new[] { p1, p2 });

                lr = obj2.AddComponent<LineRenderer>();
                lr.startWidth = .1f;
                lr.endWidth = .1f;
                lr.SetPositions(new[] { p2, p3 });

                lr = obj3.AddComponent<LineRenderer>();
                lr.startWidth = .1f;
                lr.endWidth = .1f;
                lr.SetPositions(new[] { p3, p1 });
            }
        }

        public int[] TriangulatePolygon(Vector2[] XZofVertices)
        {
            int vertexCount = XZofVertices.Length;
            float xmin = XZofVertices[0].x;
            float ymin = XZofVertices[0].y;
            float xmax = xmin;
            float ymax = ymin;

            for (int i = 1; i < vertexCount; i++)
            {
                if (XZofVertices[i].x < xmin)
                {
                    xmin = XZofVertices[i].x;
                }
                else if (XZofVertices[i].x > xmax)
                {
                    xmax = XZofVertices[i].x;
                }
                else if (XZofVertices[i].y < ymin)
                {
                    ymin = XZofVertices[i].y;
                }
                else if (XZofVertices[i].y > ymax)
                {
                    ymax = XZofVertices[i].y;
                }
            }

            float dx = xmax - xmin;
            float dy = ymax - ymin;
            float dmax = (dx > dy) ? dx : dy;
            float xmid = (xmax + xmin) * .5f;
            float ymid = (ymax + ymin) * .5f;
            Vector2[] expandedXZ = new Vector2[3 + vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                expandedXZ[i] = XZofVertices[i];
            }

            expandedXZ[vertexCount] = new Vector2((xmid - 2 * dmax), (ymid - dmax));
            expandedXZ[vertexCount + 1] = new Vector2(xmid, (ymid + 2 * dmax));
            expandedXZ[vertexCount + 2] = new Vector2((xmid + 2 * dmax), (ymid - dmax));
            List<DelaunayTriangle2D_L> triangleList = new List<DelaunayTriangle2D_L>();
            triangleList.Add(
                new DelaunayTriangle2D_L(vertexCount, vertexCount + 1, vertexCount + 2));
            for (int i = 0; i < vertexCount; i++)
            {
                List<DelaunayEdge2D_L> edges = new();
                for (int j = 0; j < triangleList.Count; j++)
                {
                    if (TriangulatePolygonSubFuncInCyrcle
                        (
                            expandedXZ[i],
                            expandedXZ[triangleList[j].p1],
                            expandedXZ[triangleList[j].p2],
                            expandedXZ[triangleList[j].p3]
                        ))
                    {
                        edges.Add(new DelaunayEdge2D_L(triangleList[j].p1, triangleList[j].p2));
                        edges.Add(new DelaunayEdge2D_L(triangleList[j].p2, triangleList[j].p3));
                        edges.Add(new DelaunayEdge2D_L(triangleList[j].p3, triangleList[j].p1));
                        triangleList.RemoveAt(j);
                        j--;
                    }
                }

                if (i >= vertexCount)
                {
                    continue;
                }

                for (int j = edges.Count - 2; j >= 0; j--)
                {
                    for (int k = edges.Count - 1; k >= j + 1; k--)
                    {
                        if (edges[j].Equals(edges[k]))
                        {
                            edges.RemoveAt(k);
                            edges.RemoveAt(j);
                            k--;
                            continue;
                        }
                    }
                }

                for (int j = 0; j < edges.Count; j++)
                {
                    triangleList.Add(new DelaunayTriangle2D_L(edges[j].p1, edges[j].p2, i));
                }

                edges.Clear();
                edges = null;
            }

            for (int i = triangleList.Count - 1; i >= 0; i--)
            {
                if (triangleList[i].p1 >= vertexCount
                    || triangleList[i].p2 >= vertexCount
                    || triangleList[i].p3 >= vertexCount)
                {
                    triangleList.RemoveAt(i);
                }
            }

            triangleList.TrimExcess();
            int[] triangles = new int[3 * triangleList.Count];
            for (int i = 0; i < triangleList.Count; i++)
            {
                triangles[3 * i] = triangleList[i].p1;
                triangles[3 * i + 1] = triangleList[i].p2;
                triangles[3 * i + 2] = triangleList[i].p3;
            }

            return triangles;
        }
    }

    /// <summary>
    /// ドロネー三角形分割の機能を提供する
    /// </summary>
    public class DelaunayMaker2D_L : MonoBehaviour
    {
        [SerializeField] private List<Transform> _transforms;

        private DelaunayTriangulator2D_L _delunayTriangulator = new();
        private const int WIDTH = 32, HEIGHT = 32;

        Vector2[] MakePoints(int count)
        {
            var arr = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                var v = new Vector2(Random.Range(.0f, WIDTH), Random.Range(.0f, HEIGHT));
                arr[i] = v;
            }

            return arr;
        }

        private void Start()
        {
            _delunayTriangulator.CreateInfluencePolygon(MakePoints(16));
        }
    }
}