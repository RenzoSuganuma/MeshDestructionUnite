using SmasherDestruction.Datas;
using System.Collections.Generic;
using UnityEngine;

namespace SmasherDestruction.Editor
{
    /// <summary>
    /// メッシュ切断機能を提供する。
    /// </summary>
    // Ver 1.0.0
    public static class Tsujigiri
    {
        private static Mesh _victimMesh;
        private static SlicedMesh _topSlicedMesh = new();
        private static SlicedMesh _bottomSlicedMesh = new();
        private static List<Vector3> _newVertices = new List<Vector3>();
        private static Plane _blade;

        /// <summary>
        /// メッシュを切断し、切断されたメッシュを返す ラッパーメソッド
        /// </summary>
        /// <param name="sourceObject">切断対象のゲームオブジェクト</param>
        /// <param name="anchorPos">切断面のアンカー位置</param>
        /// <param name="normalDir">切断面の法線</param>
        /// <param name="capMat">切断面のマテリアル</param>
        /// <returns></returns>
        public static GameObject[] CutMesh(
            GameObject sourceObject,
            Vector3 anchorPos,
            Vector3 normalDir,
            Material capMat,
            bool makeGap)
        {
            // 対象のローカル座標から平面を生成
            _blade = new Plane(
                // 切断面の法線、ローカルポジションを初期化
                sourceObject.transform.InverseTransformDirection(-normalDir),
                sourceObject.transform.InverseTransformPoint(anchorPos)
            );

            _victimMesh = sourceObject.GetComponent<MeshFilter>().sharedMesh;

            // 左右に分離したメッシュデータ、新しく追加した頂点群をクリア
            _newVertices.Clear();
            _topSlicedMesh.ClearAll();
            _bottomSlicedMesh.ClearAll();

            // 平面の左右に頂点v1,v2,v3があるかのフラグ
            bool[] sides = new bool[3];
            // サブメッシュの頂点インデックス配列
            int[] indices;
            // ３頂点
            int v1, v2, v3;

            // サブメッシュの数だけループして切断処理をする
            // ループをしないと、しっかり切れたことにならないから切っておく
            for (int submesh = 0; submesh < _victimMesh.subMeshCount; submesh++)
            {
                indices = _victimMesh.GetIndices(submesh);

                // サブメッシュ１つ分のインデックスリスト
                _topSlicedMesh.SubIndices.Add(new List<int>());
                _bottomSlicedMesh.SubIndices.Add(new List<int>());

                // サブメッシュのインデックス数分ループ
                for (int i = 0; i < indices.Length; i += 3)
                {
                    v1 = indices[i];
                    v2 = indices[i + 1];
                    v3 = indices[i + 2];

                    // 頂点が平面の面の上にあるか判定。あるなら真
                    sides[0] = _blade.GetSide(_victimMesh.vertices[v1]);
                    sides[1] = _blade.GetSide(_victimMesh.vertices[v2]);
                    sides[2] = _blade.GetSide(_victimMesh.vertices[v3]);

                    // すべての頂点が切断面の上にある場合には切断処理をしない
                    if (sides[0] == sides[1] && sides[0] == sides[2])
                    {
                        // 左右にあるかに応じ、トライアングルの追加
                        if (sides[0])
                        {
                            _topSlicedMesh.AddTriangle(v1, v2, v3, submesh, ref _victimMesh);
                        }
                        else
                        {
                            _bottomSlicedMesh.AddTriangle(v1, v2, v3, submesh, ref _victimMesh);
                        }
                    }
                    else // 切断面の上下にある場合には切断処理
                    {
                        // 切断をする
                        CutThisFace(submesh, sides, v1, v2, v3);
                    }
                }
            }

            Material[] materials = sourceObject.GetComponent<MeshRenderer>().sharedMaterials;

            if (materials[materials.Length - 1].name != capMat.name)
            {
                _topSlicedMesh.SubIndices.Add(new List<int>());
                _bottomSlicedMesh.SubIndices.Add(new List<int>());

                Material[] newMaterials = new Material[materials.Length + 1];

                materials.CopyTo(newMaterials, 0);

                newMaterials[materials.Length] = capMat;

                materials = newMaterials;
            }

            // カット処理
            Capping();

            // 左側のメッシュを生成
            Mesh leftHalfMesh = new Mesh();
            leftHalfMesh.name = "Left Splitted";
            leftHalfMesh.vertices = _topSlicedMesh.Vertices.ToArray();
            leftHalfMesh.triangles = _topSlicedMesh.Triangles.ToArray();
            leftHalfMesh.normals = _topSlicedMesh.Normals.ToArray();
            leftHalfMesh.uv = _topSlicedMesh.UVs.ToArray();

            leftHalfMesh.subMeshCount = _topSlicedMesh.SubIndices.Count;
            for (int i = 0; i < _topSlicedMesh.SubIndices.Count; i++)
            {
                leftHalfMesh.SetIndices(_topSlicedMesh.SubIndices[i].ToArray(), MeshTopology.Triangles, i);
            }

            // 右側のメッシュを生成
            Mesh rightHalfMesh = new Mesh();
            rightHalfMesh.name = "Right Splitted";
            rightHalfMesh.vertices = _bottomSlicedMesh.Vertices.ToArray();
            rightHalfMesh.triangles = _bottomSlicedMesh.Triangles.ToArray();
            rightHalfMesh.normals = _bottomSlicedMesh.Normals.ToArray();
            rightHalfMesh.uv = _bottomSlicedMesh.UVs.ToArray();

            rightHalfMesh.subMeshCount = _bottomSlicedMesh.SubIndices.Count;
            for (int i = 0; i < _bottomSlicedMesh.SubIndices.Count; i++)
            {
                rightHalfMesh.SetIndices(_bottomSlicedMesh.SubIndices[i].ToArray(), MeshTopology.Triangles, i);
            }

            // 元のオブジェクトを左側に
            sourceObject.name = "Left Side";
            sourceObject.GetComponent<MeshFilter>().mesh = leftHalfMesh;

            // 右側は生成
            GameObject leftObj = sourceObject;

            GameObject rightObj = new GameObject("Right Side", typeof(MeshFilter), typeof(MeshRenderer));
            rightObj.transform.position = sourceObject.transform.position;
            rightObj.transform.rotation = sourceObject.transform.rotation;
            rightObj.GetComponent<MeshFilter>().mesh = rightHalfMesh;

            leftObj.GetComponent<MeshRenderer>().materials = materials;
            rightObj.GetComponent<MeshRenderer>().materials = materials;

            if (makeGap)
            {
                leftObj.transform.position -= normalDir * .1f;
                rightObj.transform.position += normalDir * .1f;
            }

            return new GameObject[] { leftObj, rightObj };
        }

        /// <summary>
        /// メッシュ切断の本体
        /// </summary>
        /// <param name="subMesh"></param>
        /// <param name="sides"></param>
        /// <param name="index0"></param>
        /// <param name="index1"></param>
        /// <param name="index2"></param>
        private static void CutThisFace(int subMesh, bool[] sides, int index0, int index1, int index2)
        {
            Vector3[] leftPoints = new Vector3[2];
            Vector3[] leftNormals = new Vector3[2];
            Vector2[] leftUVs = new Vector2[2];

            Vector3[] rightPoints = new Vector3[2];
            Vector3[] rightNormals = new Vector3[2];
            Vector2[] rightUVs = new Vector2[2];

            bool settedLeft = false;
            bool settedRight = false;

            // トライアングルの頂点を配列として保持
            int[] p = new[] { index0, index1, index2 };

            for (int side = 0; side < 3; side++)
            {
                if (sides[side]) // 左側にある場合
                {
                    if (!settedLeft)
                    {
                        settedLeft = true;

                        // １，２番目の頂点共にひとまず同値で初期化
                        // 1番目のデータは正しかったとしてもここで２番目のデータが正しいと確約していない
                        leftPoints[1] = leftPoints[0] = _victimMesh.vertices[p[side]];

                        leftUVs[1] = leftUVs[0] = _victimMesh.uv[p[side]];

                        leftNormals[1] = leftNormals[0] = _victimMesh.normals[p[side]];
                    }
                    else
                    {
                        // ２番目の頂点のデータをここで正しいとしているデータで初期化
                        leftPoints[1] = _victimMesh.vertices[p[side]];
                        leftUVs[1] = _victimMesh.uv[p[side]];
                        leftNormals[1] = _victimMesh.normals[p[side]];
                    }
                }
                // 右側にある場合
                else
                {
                    if (!settedRight)
                    {
                        settedRight = true;

                        // １，２番目の頂点共にひとまず同値で初期化
                        // 1番目のデータは正しかったとしてもここで２番目のデータが正しいと確約していない
                        rightPoints[1] = rightPoints[0] = _victimMesh.vertices[p[side]];

                        rightUVs[1] = rightUVs[0] = _victimMesh.uv[p[side]];

                        rightNormals[1] = rightNormals[0] = _victimMesh.normals[p[side]];
                    }
                    else
                    {
                        // ２番目の頂点のデータをここで正しいとしているデータで初期化
                        // ２番目の頂点のデータをここで正しいとしているデータで初期化
                        rightPoints[1] = _victimMesh.vertices[p[side]];
                        rightUVs[1] = _victimMesh.uv[p[side]];
                        rightNormals[1] = _victimMesh.normals[p[side]];
                    }
                }
            }

            // 距離比率の値。頂点と平面の距離 を 辺の長さで割った値
            float normalizedDistance = 0f;
            // 距離。頂点と平面の距離
            float distance = 0f;

            // 左側
            // 【すでに指定した平面と交差する点を探索する】
            _blade.Raycast(new Ray(leftPoints[0], (rightPoints[0] - leftPoints[0]).normalized), out distance);

            // 距離比率を求める
            normalizedDistance = distance / (rightPoints[0] - leftPoints[0]).magnitude;

            // 上記で比率が出たので辺の長さに比率を掛けてあげる
            Vector3 newVertex1 = Vector3.Lerp(leftPoints[0], rightPoints[0], normalizedDistance);
            Vector2 newUv1 = Vector2.Lerp(leftUVs[0], rightUVs[0], normalizedDistance);
            Vector3 newNormal1 = Vector3.Lerp(leftNormals[0], rightNormals[0], normalizedDistance);

            // 新しく指定した頂点群に頂点を追加
            _newVertices.Add(newVertex1);

            // 右側
            _blade.Raycast(new Ray(leftPoints[1], (rightPoints[1] - leftPoints[1]).normalized), out distance);

            normalizedDistance = distance / (rightPoints[1] - leftPoints[1]).magnitude;

            Vector3 newVertex2 = Vector3.Lerp(leftPoints[1], rightPoints[1], normalizedDistance);
            Vector2 newUv2 = Vector2.Lerp(leftUVs[1], rightUVs[1], normalizedDistance);
            Vector3 newNormal2 = Vector3.Lerp(leftNormals[1], rightNormals[1], normalizedDistance);

            _newVertices.Add(newVertex2);

            // トライアングル
            // 左側
            // 【縮退三角形的に追加】
            _topSlicedMesh.AddTriangle(
                new Vector3[] { leftPoints[0], newVertex1, newVertex2 },
                new Vector3[] { leftNormals[0], newNormal1, newNormal2 },
                new Vector2[] { leftUVs[0], newUv1, newUv2 },
                newNormal1,
                subMesh
            );

            _topSlicedMesh.AddTriangle(
                new Vector3[] { leftPoints[0], leftPoints[1], newVertex2 },
                new Vector3[] { leftNormals[0], leftNormals[1], newNormal2 },
                new Vector2[] { leftUVs[0], leftUVs[1], newUv2 },
                newNormal2,
                subMesh
            );

            // 右側
            _bottomSlicedMesh.AddTriangle(
                new Vector3[] { rightPoints[0], newVertex1, newVertex2 },
                new Vector3[] { rightNormals[0], newNormal1, newNormal2 },
                new Vector2[] { rightUVs[0], newUv1, newUv2 },
                newNormal1,
                subMesh
            );

            _bottomSlicedMesh.AddTriangle(
                new Vector3[] { rightPoints[0], rightPoints[1], newVertex2 },
                new Vector3[] { rightNormals[0], rightNormals[1], newNormal2 },
                new Vector2[] { rightUVs[0], rightUVs[1], newUv2 },
                newNormal2,
                subMesh
            );
        }

        private static List<Vector3> _capVertChecked = new List<Vector3>();
        private static List<Vector3> _capVertPolygon = new List<Vector3>();

        /// <summary>
        /// 切断処理で新たに生成された頂点に基づいてカット面の生成をする
        /// </summary>
        private static void Capping()
        {
            _capVertChecked.Clear();

            for (int i = 0; i < _newVertices.Count; i++)
            {
                // 調査済みはとばす
                if (_capVertChecked.Contains(_newVertices[i]))
                {
                    continue;
                }

                _capVertPolygon.Clear();

                _capVertPolygon.Add(_newVertices[i]);
                _capVertPolygon.Add(_newVertices[i + 1]);

                _capVertChecked.Add(_newVertices[i]);
                _capVertChecked.Add(_newVertices[i + 1]);

                bool isDone = false;
                while (!isDone)
                {
                    isDone = true;

                    for (int k = 0; k < _newVertices.Count; k += 2)
                    {
                        // 【新頂点のペアを探す】
                        if (_newVertices[k] == _capVertPolygon[_capVertPolygon.Count - 1] &&
                            !_capVertChecked.Contains(_newVertices[k + 1]))
                        {
                            // ペアの頂点を見つけたらポリゴン配列へ追加、次のループを回す。
                            isDone = false;
                            _capVertPolygon.Add(_newVertices[k + 1]);
                            _capVertChecked.Add(_newVertices[k + 1]);
                        }
                        else if (_newVertices[k + 1] == _capVertPolygon[_capVertPolygon.Count - 1] &&
                                 !_capVertChecked.Contains(_newVertices[k]))
                        {
                            isDone = false;
                            _capVertPolygon.Add(_newVertices[k]);
                            _capVertChecked.Add(_newVertices[k]);
                        }
                    }
                }

                // ポリゴン形成
                FillCap(_capVertPolygon);
            }
        }

        /// <summary>
        /// 渡された頂点の配列の基づいてポリゴンの形成をする
        /// </summary>
        /// <param name="verts">ポリゴンの頂点リスト</param>
        private static void FillCap(List<Vector3> verts)
        {
            Vector3 center = Vector3.zero; // 中心と各頂点を結んで三角形を形成するのでこれを定義

            foreach (var vert in verts)
            {
                center += vert;
            }

            center /= verts.Count;

            Vector3 upward = Vector3.zero;

            // 90度回転。 平面の左側を上とする
            upward.x = _blade.normal.y;
            upward.y = -_blade.normal.x;
            upward.z = _blade.normal.z;

            Vector3 left = Vector3.Cross(_blade.normal, upward);

            Vector3 displacement = Vector3.zero;
            Vector3 newUv1 = Vector3.zero;
            Vector3 newUv2 = Vector3.zero;

            for (int i = 0; i < verts.Count; i++)
            {
                // 中心からの頂点へのベクトル
                displacement = verts[i] - center;

                // uv値をとる
                newUv1 = Vector3.zero;
                newUv1.x = .5f + Vector3.Dot(displacement, left);
                newUv1.y = .5f + Vector3.Dot(displacement, upward);
                newUv1.z = .5f + Vector3.Dot(displacement, _blade.normal);

                // 最後の頂点は最初の頂点を利用するのでインデックスを循環させる
                displacement = verts[(i + 1) % verts.Count] - center;

                newUv2 = Vector3.zero;
                newUv2.x = .5f + Vector3.Dot(displacement, left);
                newUv2.y = .5f + Vector3.Dot(displacement, upward);
                newUv2.z = .5f + Vector3.Dot(displacement, _blade.normal);

                _topSlicedMesh.AddTriangle(
                    new Vector3[]
                    {
                        verts[i],
                        verts[(i + 1) % verts.Count],
                        center
                    },
                    new Vector3[]
                    {
                        -_blade.normal,
                        -_blade.normal,
                        -_blade.normal
                    },
                    new Vector2[]
                    {
                        newUv1,
                        newUv2,
                        Vector2.one * .5f
                    },
                    -_blade.normal,
                    // カット面をサブメッシュとして登録
                    _topSlicedMesh.SubIndices.Count - 1
                );

                _bottomSlicedMesh.AddTriangle(
                    new Vector3[]
                    {
                        verts[i],
                        verts[(i + 1) % verts.Count],
                        center
                    },
                    new Vector3[]
                    {
                        _blade.normal,
                        _blade.normal,
                        _blade.normal
                    },
                    new Vector2[]
                    {
                        newUv1,
                        newUv2,
                        Vector2.one * .5f
                    },
                    _blade.normal,
                    // カット面をサブメッシュとして登録
                    _bottomSlicedMesh.SubIndices.Count - 1
                );
            }
        }
    }
}