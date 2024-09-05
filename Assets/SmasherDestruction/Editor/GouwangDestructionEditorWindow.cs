using System;
using System.Collections.Generic;
using System.IO;
using GouwangDestruction.Core;
using UnityEditor;
using UnityEngine;
using SmasherDestruction.Editor;

namespace GouwangDestruction.Editor
{
    public class GouwangDestructionEditorWindow : EditorWindow
    {
        /// <summary> 切断対象のオブジェクト </summary>
        public GameObject VictimObject;

        /// <summary> 切断平面のオブジェクト </summary>
        public Transform PlaneObject;

        /// <summary> 切断面のマテリアル </summary>
        public Material CapMaterial;

        private SerializedObject _serializedObject;
        private List<GameObject> _cuttedMeshes = new List<GameObject>();
        private Vector3 _planeAnchorPos;
        private Vector3 _planeRot;
        private string _meshName;
        private bool _makeGap;
        private int _fragModeIndex;
        private int _mode;

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
            EditorGUILayout.TextArea("SmasherDestruction : Experimental",
                SmasherDestructionConstantValues.GetGUIStyle_LabelBig());

            if (_serializedObject is not null)
            {
                EditorGUILayout.PropertyField(_serializedObject.FindProperty($"{nameof(VictimObject)}"));
                EditorGUILayout.PropertyField(_serializedObject.FindProperty($"{nameof(PlaneObject)}"));
                EditorGUILayout.PropertyField(_serializedObject.FindProperty($"{nameof(CapMaterial)}"));
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
            GUILayout.Label("Fragmentation Mode", SmasherDestructionConstantValues.GetGUIStyle_LabelSmall());

            // 編集モード を 選ぶ
            _mode = GUILayout.Toolbar(_mode,
                new GUIContent[3]
                    { new GUIContent("Ryden"), new GUIContent("ArmStrong"), new GUIContent("Smasher") });

            // 隙間を つくるか
            _makeGap = EditorGUILayout.Toggle("Make Gap", _makeGap);

            // ファイル名
            GUILayout.Label("Mesh File Name", SmasherDestructionConstantValues.GetGUIStyle_LabelSmall());
            _meshName = GUILayout.TextArea(_meshName);

            // メッシュ編集 実行ボタン
            if (GUILayout.Button(
                    _mode switch
                    {
                        0 => "Cut Mesh",
                        1 => "Frag Mesh",
                        2 => "Smash Mesh",
                        _ => ""
                    }))
            {
                switch (_mode)
                {
                    case 0:
                    {
                        TsujigiriUtility.CutTheMesh(VictimObject, _cuttedMeshes, _planeAnchorPos, PlaneObject.up,
                            CapMaterial, _makeGap);
                        break;
                    }
                    case 1:
                    {
                        GouwangDestructionCore.CutRandomly_ArmStrong(VictimObject, _cuttedMeshes, PlaneObject,
                            CapMaterial, _makeGap, _fragModeIndex);
                        break;
                    }
                    case 2:
                    {
                        break;
                    }
                }
            } // フラグ モード ボタン

            // メッシュ 保存ボタン
            if (GUILayout.Button("Save Mashes"))
            {
                CheckDirectory();

                SaveCuttedMeshes();
            }

            GUILayout.Space(10);

            // モードごとに表示するオプション
            switch (_mode)
            {
                case 1:
                case 2:
                {
                    GUILayout.Label($"Fragmentation Mode : {_fragModeIndex}");

                    _fragModeIndex = EditorGUILayout.IntSlider(_fragModeIndex, 0, 5);

                    break;
                }
            }

            // 切断面 の 回転 指定
            _planeRot = EditorGUILayout.Vector3Field("PlaneObject Rotation", _planeRot);
            // 切断面 の 回転 リセット
            if (EditorGUILayout.LinkButton("Reset Value"))
            {
                _planeRot = Vector3.zero;
            }

            // 切断面 の 位置 指定
            _planeAnchorPos = EditorGUILayout.Vector3Field("PlaneObject Anchor Pos", _planeAnchorPos);
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
            if (_cuttedMeshes.Count < 1) return;

            Gouwang.FindSaveTargetDirectory(Gouwang.CuttedMeshesFolderAbsolutePath + $"{_meshName}/");
            Gouwang.FindSaveTargetDirectory(Gouwang.CuttedMeshesPrefabFolderAbsolutePath);

            _cuttedMeshes[0].name = _meshName;

            // コンポーネントのアタッチ
            foreach (var cuttedMesh in _cuttedMeshes)
            {
                cuttedMesh.AddComponent<MeshCollider>();
                cuttedMesh.GetComponent<MeshCollider>().sharedMesh = cuttedMesh.GetComponent<MeshFilter>().sharedMesh;
                cuttedMesh.GetComponent<MeshCollider>().convex = true;
                cuttedMesh.AddComponent<Rigidbody>();
            }

            // カットしたメッシュは一つのオブジェクトにする
            for (int i = 1; i < _cuttedMeshes.Count; ++i)
            {
                _cuttedMeshes[i].transform.parent = _cuttedMeshes[0].transform;
            }

            // 保存処理
            for (int i = 0; i < _cuttedMeshes.Count; ++i)
            {
                var mesh = _cuttedMeshes[i].GetComponent<MeshFilter>().sharedMesh;

                AssetDatabase.CreateAsset(mesh,
                    Gouwang.CuttedMeshesFolderAbsolutePath + $"{_meshName}/{mesh.name}_{i}.asset");
            }

            PrefabUtility.SaveAsPrefabAsset(_cuttedMeshes[0],
                Gouwang.CuttedMeshesPrefabFolderAbsolutePath + $"{_meshName}.prefab");
        }
    }
}