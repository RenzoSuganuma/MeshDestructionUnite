using System;
using System.Collections.Generic;
using System.IO;
using GouwangDestruction.Core;
using UnityEditor;
using UnityEngine;
using SmasherDestruction.Editor;
using UnityEngine.Serialization;

namespace GouwangDestruction.Editor
{
    /// <summary>
    /// 編集モード上で実行されるツールのインターフェイスの本体
    /// </summary>
    public sealed class SmasherDestructionEditorWindow : EditorWindow
    {
        /// <summary> 切断対象のオブジェクト </summary>
        public GameObject VictimObject;

        /// <summary> 切断平面のオブジェクト </summary>
        public Transform PlaneObject;

        /// <summary> 切断面のマテリアル </summary>
        public Material InsideMaterial;

        /// <summary>
        /// オブジェクトをウィンドウにアタッチできるように宣言
        /// </summary>
        private SerializedObject _serializedObject;

        private List<GameObject> _fragmentsObject = new List<GameObject>();
        private GameObject _fragmentsParent;
        private Vector3 _planeAnchorPos;
        private Vector3 _planeRot;
        private string _meshName;
        private bool _makeGap;
        private int _fragModeIndex;

        /// <summary>
        /// ツールのモード 0 = 辻斬り 、 1 = 剛腕
        /// </summary>
        private FragmentationMode _fragmentationMode;

        private void OnEnable()
        {
            _serializedObject = new SerializedObject(this);
            _planeAnchorPos = _planeRot = Vector3.zero;
        }

        private void Update()
        {
            Repaint(); // 毎フレーム内容が更新されるようにする
        }

        private void OnGUI()
        {
            _serializedObject.Update();

            // プロパティを表示して編集可能にする
            EditorGUILayout.TextArea("<color=green>SmasherDestruction</color>",
                SmasherDestructionConstantValues.GetGUIStyle_LabelTitle());

            if (_serializedObject is not null)
            {
                EditorGUILayout.PropertyField(_serializedObject.FindProperty($"{nameof(VictimObject)}"));
                EditorGUILayout.PropertyField(_serializedObject.FindProperty($"{nameof(PlaneObject)}"));
                EditorGUILayout.PropertyField(_serializedObject.FindProperty($"{nameof(InsideMaterial)}"));
            }

            // 破壊対象あるなら描写する
            if (_serializedObject.FindProperty($"{nameof(VictimObject)}").objectReferenceValue is not null)
            {
                Draw();
            }
            else
            {
                EditorGUILayout.TextArea("Attach The Destruction Target",
                    SmasherDestructionConstantValues.GetGUIStyle_LabelSmall());
            }

            // ウィンドウを閉じる ボタン
            if (GUILayout.Button("Close Window"))
            {
                _planeRot = _planeAnchorPos = Vector3.zero;
                if (PlaneObject is not null)
                {
                    PlaneObject.transform.position = _planeAnchorPos;
                    PlaneObject.transform.rotation = Quaternion.Euler(_planeRot);
                }

                _serializedObject.Dispose();
                _serializedObject = null;
                _ = _serializedObject;
                Close();
            }

            if (_serializedObject is not null)
            {
                _serializedObject.ApplyModifiedProperties();
            }
        }

        private void Draw()
        {
            // フラグモード ラベル
            GUILayout.Label("Fragmentation Mode",
                SmasherDestructionConstantValues.GetGUIStyle_LabelSmall());

            // 編集モード を 選ぶ
            var fragModeInt = (int)_fragmentationMode;
            fragModeInt = GUILayout.Toolbar(fragModeInt,
                new GUIContent[2]
                    { new GUIContent("Tsujigiri"), new GUIContent("Gouwang") });
            _fragmentationMode = (FragmentationMode)fragModeInt;

            // 隙間を つくるか
            _makeGap = EditorGUILayout.Toggle("Make Gap", _makeGap);

            // ファイル名
            GUILayout.Label("Fragment File Name",
                SmasherDestructionConstantValues.GetGUIStyle_LabelSmall());
            _meshName = GUILayout.TextArea(_meshName);

            // メッシュ編集 実行ボタン
            if (GUILayout.Button(
                    _fragmentationMode switch // モードが辻斬り か 剛腕 かで分岐
                    {
                        FragmentationMode.Tsujigiri => "Cut Mesh",
                        FragmentationMode.Gouwang => "Frag Mesh",
                        _ => ""
                    }))
            {
                switch (_fragmentationMode)
                {
                    case FragmentationMode.Tsujigiri: // モード ＝ 辻斬り
                    {
                        TsujigiriUtility.CutTheMesh(
                            VictimObject,
                            _fragmentsObject,
                            _planeAnchorPos,
                            PlaneObject.up,
                            InsideMaterial,
                            _makeGap);
                        break;
                    }
                    case FragmentationMode.Gouwang: // モード ＝ 剛腕
                    {
                        GouwangUtility.DoFragmentation(
                            VictimObject,
                            _fragmentsObject,
                            PlaneObject,
                            InsideMaterial,
                            _makeGap);

                        // 断片のオブジェクトを１つにまとめる
                        var parentObj = new GameObject();
                        parentObj.name = _meshName;
                        _fragmentsParent = parentObj;
                        foreach (var fragment in _fragmentsObject)
                        {
                            fragment.transform.SetParent(parentObj.transform);
                        }

                        break;
                    }
                }
            }

            // メッシュ 保存ボタン
            if (GUILayout.Button("Save Mashes"))
            {
                CheckDirectory();

                SaveCuttedMeshes();
            }

            GUILayout.Space(10);

            // 切断面 の 回転 指定
            _planeRot = EditorGUILayout.Vector3Field("PlaneObject Rotation", _planeRot);
            // 切断面 の 回転 リセット
            if (EditorGUILayout.LinkButton("Reset Value"))
            {
                _planeRot = Vector3.zero;
            }

            // 切断面 の 位置 指定
            _planeAnchorPos = EditorGUILayout.Vector3Field("PlaneObject Anchor-Position", _planeAnchorPos);
            // 切断面 の 位置 リセット
            if (EditorGUILayout.LinkButton("Reset Value"))
            {
                _planeAnchorPos = Vector3.zero;
            }

            try
            {
                if (PlaneObject is not null)
                {
                    PlaneObject.transform.position = _planeAnchorPos;
                    PlaneObject.transform.rotation = Quaternion.Euler(_planeRot);
                }
            }
            catch (UnassignedReferenceException e)
            {
            }

            GUILayout.Space(10);
        }

        private void CheckDirectory()
        {
            Gouwang.FindSaveTargetDirectory(Gouwang.CuttedMeshesFolderAbsolutePath + $"{_meshName}/");
            Gouwang.FindSaveTargetDirectory(Gouwang.CuttedMeshesPrefabFolderAbsolutePath);
        }

        private void SaveCuttedMeshes() // 保存先のパスにメッシュのアセットとプレハブを保存する
        {
            if (_fragmentsObject.Count < 1) return;

            Gouwang.FindSaveTargetDirectory(Gouwang.CuttedMeshesFolderAbsolutePath + $"{_meshName}/");
            Gouwang.FindSaveTargetDirectory(Gouwang.CuttedMeshesPrefabFolderAbsolutePath);

            // コンポーネントのアタッチ
            foreach (var cuttedMesh in _fragmentsObject)
            {
                cuttedMesh.AddComponent<MeshCollider>();
                cuttedMesh.GetComponent<MeshCollider>().sharedMesh = cuttedMesh.GetComponent<MeshFilter>().sharedMesh;
                cuttedMesh.GetComponent<MeshCollider>().convex = true;
                cuttedMesh.AddComponent<Rigidbody>();
            }

            #region 保存処理

            // 断片化されたメッシュのアセットとしての保存処理
            for (int i = 0; i < _fragmentsObject.Count; ++i)
            {
                var mesh = _fragmentsObject[i].GetComponent<MeshFilter>().sharedMesh;

                AssetDatabase.CreateAsset(mesh,
                    Gouwang.CuttedMeshesFolderAbsolutePath + $"{_meshName}/{_meshName}_Mesh_{i}.asset");
            }

            // プレハブとして保存
            switch (_fragmentationMode)
            {
                case FragmentationMode.Tsujigiri:
                {
                    if (_fragmentsParent is null)
                    {
                        var p = new GameObject();
                        p.name = _meshName;
                        _fragmentsParent = p;

                        foreach (var frag in _fragmentsObject)
                        {
                            frag.transform.SetParent(_fragmentsParent.transform);
                        }
                    }
                    break;
                }

                case FragmentationMode.Gouwang:
                {
                    break;
                }
            }
            
            PrefabUtility.SaveAsPrefabAsset(_fragmentsParent,
                Gouwang.CuttedMeshesPrefabFolderAbsolutePath + $"{_meshName}.prefab");

            #endregion
        }
    }
}