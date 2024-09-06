using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SmasherDestruction.Nurikabe.Delanuay
{
    /// <summary>
    /// ドロネー三角形を形成する機能を提供するクラス
    /// </summary>
    public sealed class DelanuayTriangulator2D
    {
        /// <summary>
        /// 円上の頂点で三角形を構成する
        /// </summary>
        /// おそらく 「円周角の定理(共円条件)の逆」を利用している
        public bool PointsIsInCircle
            (Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            // 渡された三角形を構成する辺のうち２つの長さが0に近いときは三角形が構成できない
            if (Mathf.Abs(p1.y - p2.y) < float.Epsilon
                && Mathf.Abs(p2.y - p3.y) < float.Epsilon)
            {
                return false;
            }

            // m1 , m2 = 垂直二等分線の傾き
            // mx1,mx2,my1,mx2 = 中点
            // xc,yc = 外周円の中点
            float m1, m2, mx1, mx2, my1, my2, xc, yc;

            // 三角形のうち１つの辺の長さが0なら
            if (Mathf.Abs(p2.y - p1.y) < float.Epsilon)
            {
                m2 = -(p3.x - p2.x) / (p3.y - p2.y); // 垂直二等分線の傾き = -1 * 辺の傾き
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
            } // どの編も長さがあるなら
            else
            {
                // 垂直二等分線の傾き = -1 * 辺の傾き
                m1 = -(p2.x - p1.x) / (p2.y - p1.y);
                m2 = -(p3.x - p2.x) / (p3.y - p1.y);
                // 辺の中点を求めている
                mx1 = (p1.x + p2.x) * .5f;
                mx2 = (p2.x + p3.x) * .5f;
                my1 = (p1.y + p2.y) * .5f;
                my2 = (p2.y + p3.y) * .5f;
                // NOTE:
                // 垂直二等分線の方程式
                // [ y = ax + b ] より、
                //  [ y = m1(x - mx1) + my1 ] [ y = m2(x - mx2) + my2 ]
                // ２式を連立させると
                // m1(x - mx1) + my1 = m2(x - mx2) + my2 
                // これをxについて解くと、
                // x = m1 * mx1 - m2 * mx2 - my2 - my1 / m1 - m2
                // x = xc [ xc = 円の中心のx座標 ]
                xc = (m1 * mx1 - m2 * mx2 - my2 - my1) / (m1 - m2);
                // y = a * x + b から
                yc = m1 * (xc - mx1) + my1;
            }

            // NOTE:
            // xc = 外接円の中点のX成分
            // xy = 外接円の中点のY成分

            float dx = p2.x - xc;
            float dy = p2.y - yc;
            float rsqr = dx * dx + dy * dy;
            dx = p1.x - xc;
            dy = p1.y - yc;
            double dsqr = dx * dx + dy * dy;

            // p1,p2が同じ外接円の円周上にあるならばその円の中点との距離は p1,p2ともに等しい値である
            return (dsqr <= rsqr);
        }

        /// <summary>
        /// 頂点群【X,Z成分を抽出したもの】から三角形分割をしたメッシュを生成する
        /// </summary>
        public Mesh CreateMesh(Vector2[] XZofVertices)
        {
            Vector3[] vertices = new Vector3[XZofVertices.Length];
            for (int i = 0; i < XZofVertices.Length; i++)
            {
                vertices[i] = new Vector3(XZofVertices[i].x, 0, XZofVertices[i].y);
            }

            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.uv = XZofVertices;
            mesh.triangles = TriangulatePolygon(XZofVertices);
            mesh.RecalculateNormals();

            return mesh;
        }

        /// <summary>
        /// 渡される点群をベースのメッシュへ追加して三角形分割をしたメッシュを生成する
        /// </summary>
        public Mesh CreateTriangulatedMesh(Vector2[] XZofVertices, Mesh baseMesh)
        {
            var baseMeshUV = baseMesh.uv;
            var applyingUVPos = new Vector2[baseMeshUV.Length + XZofVertices.Length];

            var mesh = new Mesh();

            #region ApplyingVertices

            Vector3[] additionalVertices = new Vector3[XZofVertices.Length];
            for (int i = 0; i < XZofVertices.Length; i++)
            {
                additionalVertices[i] = new Vector3(XZofVertices[i].x, 0, XZofVertices[i].y);
            }

            Vector3[] applyingVertices =
                new Vector3[baseMesh.vertices.Length + additionalVertices.Length];
            for (int i = 0; i < baseMesh.vertices.Length; i++)
            {
                // ベースメッシュの頂点群
                applyingVertices[i] = baseMesh.vertices[i];
            }

            for (int i = 0; i < additionalVertices.Length; i++)
            {
                // 追加の頂点群
                applyingVertices[(baseMesh.vertices.Length - 1) + i] = additionalVertices[i];
            }

            #endregion

            #region ApplyingUV

            for (int i = 0; i < baseMeshUV.Length; i++)
            {
                // ベースメッシュのUV
                applyingUVPos[i] = baseMeshUV[i];
            }

            for (int i = 0; i < XZofVertices.Length; i++)
            {
                // 追加のUV
                applyingUVPos[(baseMeshUV.Length - 1) + i] = XZofVertices[i];
            }

            #endregion

            mesh.vertices = applyingVertices;
            mesh.uv = applyingUVPos;
            mesh.triangles = TriangulatePolygon(applyingUVPos);
            mesh.RecalculateNormals();

            return mesh;
        }

        /// <summary>
        /// 点群から三角形分割をする
        /// </summary>
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
            List<DelanuayTriangle2D> triangleList = new();
            triangleList.Add(
                new DelanuayTriangle2D(vertexCount, vertexCount + 1, vertexCount + 2));
            for (int i = 0; i < vertexCount; i++)
            {
                List<DelanuayEdge2D> edges = new();
                for (int j = 0; j < triangleList.Count; j++)
                {
                    if (PointsIsInCircle
                        (
                            expandedXZ[i],
                            expandedXZ[triangleList[j].p1],
                            expandedXZ[triangleList[j].p2],
                            expandedXZ[triangleList[j].p3]
                        ))
                    {
                        edges.Add(new DelanuayEdge2D(triangleList[j].p1, triangleList[j].p2));
                        edges.Add(new DelanuayEdge2D(triangleList[j].p2, triangleList[j].p3));
                        edges.Add(new DelanuayEdge2D(triangleList[j].p3, triangleList[j].p1));
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
                    triangleList.Add(new DelanuayTriangle2D(edges[j].p1, edges[j].p2, i));
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
}