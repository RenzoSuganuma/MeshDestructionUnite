using System.Collections.Generic;
using SmasherDestruction.Datas;
using UnityEngine;

namespace SmasherDestruction.Editor
{
    /// <summary>
    /// メッシュ切断機能を提供する。
    /// </summary>
    // Ver 1.0.0
    public static class Tsujigiri
    {
        private static Plane _blade;
        private static Mesh _victimMesh;
        private static SlicedMesh _leftMesh = new();
        private static SlicedMesh _rightMesh = new();
        private static List<Vector3> _newVerticesPos = new();

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
            _newVerticesPos.Clear();
            _leftMesh.ClearAll();
            _rightMesh.ClearAll();

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
                _leftMesh.SubIndices.Add(new List<int>());
                _rightMesh.SubIndices.Add(new List<int>());

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
                            _leftMesh.AddTriangle(v1, v2, v3, submesh, ref _victimMesh);
                        }
                        else
                        {
                            _rightMesh.AddTriangle(v1, v2, v3, submesh, ref _victimMesh);
                        }
                    }
                    else // 切断面の上下にある場合には切断処理
                    {
                        // 切断をする va ----s-----|v|--t--> vb
                        // vのように辺vavbを分割する頂点を生成、追加する
                        CutThisFace(submesh, sides, v1, v2, v3);
                    }
                }
            }

            Material[] materials = sourceObject.GetComponent<MeshRenderer>().sharedMaterials;

            if (materials[materials.Length - 1].name != capMat.name)
            {
                _leftMesh.SubIndices.Add(new List<int>());
                _rightMesh.SubIndices.Add(new List<int>());

                Material[] newMaterials = new Material[materials.Length + 1];

                materials.CopyTo(newMaterials, 0);

                newMaterials[materials.Length] = capMat;

                materials = newMaterials;
            }

            // 処理
            FindVerticesMakeNewFace();

            // 左側のメッシュを生成
            var leftHalfMesh = _leftMesh.ToMesh();
            // 右側のメッシュを生成
            var rightHalfMesh = _rightMesh.ToMesh();

            // 元のオブジェクトを左側に
            sourceObject.name = "Left";
            sourceObject.GetComponent<MeshFilter>().mesh = leftHalfMesh;
            GameObject leftObj = sourceObject;
            leftObj.GetComponent<MeshRenderer>().materials = materials;

            // 右側は生成
            GameObject rightObj =
                new GameObject("Right",
                    typeof(MeshFilter),
                    typeof(MeshRenderer));
            rightObj.transform.position = sourceObject.transform.position;
            rightObj.transform.rotation = sourceObject.transform.rotation;
            rightObj.GetComponent<MeshFilter>().mesh = rightHalfMesh;
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
        /// <param name="subMeshIndex"></param>
        /// <param name="sides"></param>
        /// <param name="index1"></param>
        /// <param name="index2"></param>
        /// <param name="index3"></param>
        private static void CutThisFace(
            int subMeshIndex,
            bool[] sides,
            int index1,
            int index2,
            int index3)
        {
            // 平面の上を左、下を右 とする
            // position , normal , uv をまとめたいため、
            // それらをまとめている構造体の配列を実装。
            MeshVertex[] leftVertices = new MeshVertex[2];
            MeshVertex[] rightVertices = new MeshVertex[2];

            bool settedLeft = false;
            bool settedRight = false;

            // トライアングルの頂点を配列として保持
            int[] indices = new[] { index1, index2, index3 };

            for (int side = 0; side < 3; side++)
            {
                if (sides[side]) // 左側にある場合
                {
                    if (!settedLeft)
                    {
                        settedLeft = true;

                        // １，２番目の頂点共にひとまず同値で初期化
                        // 1番目のデータは正しかったとしてもここで２番目のデータが正しいと確約していない
                        leftVertices[1].Position = leftVertices[0].Position = _victimMesh.vertices[indices[side]];
                        leftVertices[1].Normal = leftVertices[0].Normal = _victimMesh.normals[indices[side]];
                        leftVertices[1].Uv = leftVertices[0].Uv = _victimMesh.uv[indices[side]];
                    }
                    else
                    {
                        // ２番目の頂点のデータをここで正しいとしているデータで初期化
                        leftVertices[1].Position = _victimMesh.vertices[indices[side]];
                        leftVertices[1].Normal = _victimMesh.normals[indices[side]];
                        leftVertices[1].Uv = _victimMesh.uv[indices[side]];
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
                        rightVertices[1].Position = rightVertices[0].Position = _victimMesh.vertices[indices[side]];
                        rightVertices[1].Normal = rightVertices[0].Normal = _victimMesh.normals[indices[side]];
                    }
                    else
                    {
                        // ２番目の頂点のデータをここで正しいとしているデータで初期化
                        rightVertices[1].Position = _victimMesh.vertices[indices[side]];
                        rightVertices[1].Normal = _victimMesh.normals[indices[side]];
                        rightVertices[1].Uv = _victimMesh.uv[indices[side]];
                    }
                }
            }

            // 距離比率の値。頂点と平面の距離 を 辺の長さで割った値
            float normalizedDistance = 0f;
            // 距離。頂点と平面の距離
            float distance = 0f;

            #region 左側の断片用に新しく頂点を求めて追加する

            // 左側
            // 【すでに指定した平面と交差する点を探索する】
            _blade.Raycast(
                new Ray(leftVertices[0].Position,
                    (rightVertices[0].Position - leftVertices[0].Position).normalized),
                out distance);
            // 距離比率を求める。
            // va ----s---|-t-> vb
            // のように比率を求めて線形補間して平面の上と下にva,vbをセパレートする頂点を求める
            normalizedDistance =
                distance
                /
                (rightVertices[0].Position - leftVertices[0].Position).magnitude;
            // 上記で比率が出たので辺の長さに比率を掛けてあげる
            Vector3 newVertex1 = Vector3.Lerp(leftVertices[0].Position, rightVertices[0].Position, normalizedDistance);
            Vector3 newNormal1 = Vector3.Lerp(leftVertices[0].Normal, rightVertices[0].Normal, normalizedDistance);
            Vector2 newUv1 = Vector2.Lerp(leftVertices[0].Uv, rightVertices[0].Uv, normalizedDistance);
            // 新しく指定した頂点群に頂点を追加
            _newVerticesPos.Add(newVertex1);

            #endregion

            #region 右側の断片用に新しく頂点を求めて追加する

            // 右側
            _blade.Raycast(
                new Ray(leftVertices[1].Position
                    , (rightVertices[1].Position - leftVertices[1].Position).normalized)
                , out distance);
            // 距離比率を求める。
            // va ----s---|-t-> vb
            // のように比率を求めて線形補間して平面の上と下にva,vbをセパレートする頂点を求める
            normalizedDistance =
                distance
                /
                (rightVertices[1].Position - leftVertices[1].Position).magnitude;
            Vector3 newVertex2 = Vector3.Lerp(leftVertices[1].Position, rightVertices[1].Position, normalizedDistance);
            Vector3 newNormal2 = Vector3.Lerp(leftVertices[1].Normal, rightVertices[1].Normal, normalizedDistance);
            Vector2 newUv2 = Vector2.Lerp(leftVertices[1].Uv, rightVertices[1].Uv, normalizedDistance);
            // 新しく指定した頂点群に頂点を追加
            _newVerticesPos.Add(newVertex2);

            #endregion

            // トライアングルを追加。

            #region 左側三角形

            // 左側
            // 【縮退三角形的に追加】
            _leftMesh.AddTriangle(
                new Vector3[] { leftVertices[0].Position, newVertex1, newVertex2 },
                new Vector3[] { leftVertices[0].Normal, newNormal1, newNormal2 },
                new Vector2[] { leftVertices[0].Uv, newUv1, newUv2 },
                newNormal1,
                subMeshIndex
            );
            _leftMesh.AddTriangle(
                new Vector3[] { leftVertices[0].Position, leftVertices[1].Position, newVertex2 },
                new Vector3[] { leftVertices[0].Normal, leftVertices[1].Normal, newNormal2 },
                new Vector2[] { leftVertices[0].Uv, leftVertices[1].Uv, newUv2 },
                newNormal2,
                subMeshIndex
            );

            #endregion

            #region 右側三角形

            // 右側
            _rightMesh.AddTriangle(
                new Vector3[] { rightVertices[0].Position, newVertex1, newVertex2 },
                new Vector3[] { rightVertices[0].Normal, newNormal1, newNormal2 },
                new Vector2[] { rightVertices[0].Uv, newUv1, newUv2 },
                newNormal1,
                subMeshIndex
            );
            _rightMesh.AddTriangle(
                new Vector3[] { rightVertices[0].Position, rightVertices[1].Position, newVertex2 },
                new Vector3[] { rightVertices[0].Normal, rightVertices[1].Normal, newNormal2 },
                new Vector2[] { rightVertices[0].Uv, rightVertices[1].Uv, newUv2 },
                newNormal2,
                subMeshIndex
            );

            #endregion
        }

        private static List<Vector3> _capVerticesChecked = new List<Vector3>();
        private static List<Vector3> _capVerticesPolygon = new List<Vector3>();

        /// <summary>
        /// 切断処理で新たに生成された頂点に基づいてカット面の生成をする
        /// </summary>
        private static void FindVerticesMakeNewFace()
        {
            _capVerticesChecked.Clear();

            for (int i = 0; i < _newVerticesPos.Count; i += 2) // ;i++ → i+= 2
            {
                // 調査済みはとばす
                if (_capVerticesChecked.Contains(_newVerticesPos[i]))
                {
                    continue;
                }

                _capVerticesPolygon.Clear();

                _capVerticesPolygon.Add(_newVerticesPos[i]);
                _capVerticesPolygon.Add(_newVerticesPos[i + 1]);

                _capVerticesChecked.Add(_newVerticesPos[i]);
                _capVerticesChecked.Add(_newVerticesPos[i + 1]);

                bool isDone = false;
                while (!isDone)
                {
                    isDone = true;

                    for (int k = 0; k < _newVerticesPos.Count; k += 2)
                    {
                        // 【新頂点のペアを探す】
                        if (_newVerticesPos[k] == _capVerticesPolygon[_capVerticesPolygon.Count - 1] &&
                            !_capVerticesChecked.Contains(_newVerticesPos[k + 1]))
                        {
                            // ペアの頂点を見つけたらポリゴン配列へ追加、次のループを回す。
                            isDone = false;
                            _capVerticesPolygon.Add(_newVerticesPos[k + 1]);
                            _capVerticesChecked.Add(_newVerticesPos[k + 1]);
                        }
                        else if (_newVerticesPos[k + 1] == _capVerticesPolygon[_capVerticesPolygon.Count - 1] &&
                                 !_capVerticesChecked.Contains(_newVerticesPos[k]))
                        {
                            isDone = false;
                            _capVerticesPolygon.Add(_newVerticesPos[k]);
                            _capVerticesChecked.Add(_newVerticesPos[k]);
                        }
                    }
                }

                // ポリゴン形成
                FillFaceFromVertices(_capVerticesPolygon);
            }
        }

        /// <summary>
        /// 渡された頂点の配列の基づいてポリゴンの形成をする
        /// </summary>
        /// <param name="vertices">ポリゴンの頂点リスト</param>
        private static void FillFaceFromVertices(List<Vector3> vertices)
        {
            Vector3 center = Vector3.zero; // 中心と各頂点を結んで三角形を形成するのでこれを定義

            foreach (var vert in vertices)
            {
                center += vert;
            }

            center /= vertices.Count;

            Vector3 upward = Vector3.zero;
            // 90度回転。 平面の左側を上とする
            upward.x = _blade.normal.y;
            upward.y = -_blade.normal.x;
            upward.z = _blade.normal.z;

            Vector3 left = Vector3.Cross(_blade.normal, upward);

            Vector3 displacement = Vector3.zero;
            Vector3 newUv1 = Vector3.zero;
            Vector3 newUv2 = Vector3.zero;

            for (int i = 0; i < vertices.Count; i++)
            {
                // 中心からの頂点へのベクトル
                displacement = vertices[i] - center;

                // uv値をとる
                newUv1 = Vector3.zero;
                newUv1.x = .5f + Vector3.Dot(displacement, left);
                newUv1.y = .5f + Vector3.Dot(displacement, upward);
                newUv1.z = .5f + Vector3.Dot(displacement, _blade.normal);

                // 最後の頂点は最初の頂点を利用するのでインデックスを循環させる
                displacement = vertices[(i + 1) % vertices.Count] - center;

                newUv2 = Vector3.zero;
                newUv2.x = .5f + Vector3.Dot(displacement, left);
                newUv2.y = .5f + Vector3.Dot(displacement, upward);
                newUv2.z = .5f + Vector3.Dot(displacement, _blade.normal);

                _leftMesh.AddTriangle(
                    new Vector3[]
                    {
                        vertices[i],
                        vertices[(i + 1) % vertices.Count],
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
                    _leftMesh.SubIndices.Count - 1
                );

                _rightMesh.AddTriangle(
                    new Vector3[]
                    {
                        vertices[i],
                        vertices[(i + 1) % vertices.Count],
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
                    _rightMesh.SubIndices.Count - 1
                );
            }
        }
    }
}