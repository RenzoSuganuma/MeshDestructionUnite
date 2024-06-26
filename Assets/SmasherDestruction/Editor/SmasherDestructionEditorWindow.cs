using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

#region MEMO

// アセット欄のデータを取得する方法
// https://note.com/ig_k/n/n907516bc958e

#endregion

namespace SmasherDestruction.Editor
{
    public class SmasherDestructionEditorWindow : EditorWindow
    {
        public GameObject VictimObject;
        public Transform PlaneObject;

        private string _meshName;
        private List<GameObject> _cuttedMeshes = new List<GameObject>();
        private SerializedObject _serializedObject;
        private Vector3 _planeAnchorPos, _planeRot;
        private Material _capMaterial;
        private int _mode;
        private int _fragModeIndex;

        private void OnEnable()
        {
            _serializedObject = new SerializedObject(this);

            _planeAnchorPos = _planeRot = Vector3.zero;

            _capMaterial = VictimObject.GetComponent<Material>();
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
            }

            GUILayout.Label("Fragmentation Mode", SmasherDestructionConstantValues.GetGUIStyle_LabelSmall());

            _mode = GUILayout.Toolbar(_mode,
                new GUIContent[3] { new GUIContent("Ryden"), new GUIContent("ArmStrong"), new GUIContent("Smasher") });


            GUILayout.Label("Mesh File Name", SmasherDestructionConstantValues.GetGUIStyle_LabelSmall());
            _meshName = GUILayout.TextArea(_meshName);

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
                        RydenHelper.CutTheMesh(VictimObject, _cuttedMeshes, _planeAnchorPos, PlaneObject.up,
                            _capMaterial);
                        break;
                    }
                    case 1:
                    case 2:
                    {
                        break;
                    }
                }
            }

            GUILayout.Space(10);

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

            _planeRot = EditorGUILayout.Vector3Field("PlaneObject Rotation", _planeRot);
            if (EditorGUILayout.LinkButton("Reset Value"))
            {
                _planeRot = Vector3.zero;
            }

            _planeAnchorPos = EditorGUILayout.Vector3Field("PlaneObject Anchor Pos", _planeAnchorPos);
            if (EditorGUILayout.LinkButton("Reset Value"))
            {
                _planeAnchorPos = Vector3.zero;
            }

            if (PlaneObject is not null)
            {
                PlaneObject.position = _planeAnchorPos;
                PlaneObject.rotation = Quaternion.Euler(_planeRot);
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Save Mesh"))
            {
                SaveMesh();
            }

            if (GUILayout.Button("Close"))
            {
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

        private void SaveMesh()
        {
            if (_cuttedMeshes.Count < 1) return;

            ArmStrong.FindSaveTargetDirectory(ArmStrong.CuttedMeshesFolderAbsolutePath + $"{_meshName}/");
            ArmStrong.FindSaveTargetDirectory(ArmStrong.CuttedMeshesPrefabFolderAbsolutePath);

            _cuttedMeshes[0].name = _meshName;

            // コンポーネントのアタッチ
            foreach (var cuttedMesh in _cuttedMeshes)
            {
                cuttedMesh.AddComponent<MeshCollider>();
                cuttedMesh.GetComponent<MeshCollider>().sharedMesh = cuttedMesh.GetComponent<MeshFilter>().mesh;
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
                var mesh = _cuttedMeshes[i].GetComponent<MeshFilter>().mesh;

                AssetDatabase.CreateAsset(mesh,
                    ArmStrong.CuttedMeshesFolderAbsolutePath + $"{_meshName}/{mesh.name}_{i}.asset");
            }

            PrefabUtility.SaveAsPrefabAsset(_cuttedMeshes[0],
                ArmStrong.CuttedMeshesPrefabFolderAbsolutePath + $"{_meshName}.prefab");
        }
    }
}