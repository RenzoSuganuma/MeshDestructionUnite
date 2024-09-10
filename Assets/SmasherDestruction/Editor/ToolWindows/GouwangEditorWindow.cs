﻿using System;
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
    /// 剛腕のエディタ画面
    /// </summary>
    public class GouwangEditorWindow : EditorWindow
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
            EditorGUILayout.TextArea("SmasherDestruction",
                SmasherDestructionConstantValues.GetGUIStyle_LabelTitle());

            // 破壊対象あるなら描写する
            Draw();

            // ウィンドウを閉じる ボタン
            if (GUILayout.Button("Close Window"))
            {
                ResetFeilds();

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

        private void ResetFeilds()
        {
            if (PlaneObject is not null)
            {
                PlaneObject.transform.position = Vector3.zero;
                PlaneObject.transform.rotation = Quaternion.Euler(0, 0, 0);
            }

            VictimObject = null;
            PlaneObject = null;
            InsideMaterial = null;
            _fragmentsObject = null;
            _fragmentsParent = null;
            _planeAnchorPos = Vector3.zero;
            _planeRot = Vector3.zero;
            _meshName = "";
            _makeGap = false;
        }

        private void Draw()
        {
            // フラグモード ラベル
            GUILayout.Label("Gouwang",
                SmasherDestructionConstantValues.GetGUIStyle_LabelSmall());
            GUILayout.Space(10);

            GUILayout.Space(10);

            if (_serializedObject is not null)
            {
                GUILayout.Space(10);
                EditorGUILayout.PropertyField(_serializedObject.FindProperty($"{nameof(VictimObject)}"));
                GUILayout.Space(10);
                EditorGUILayout.PropertyField(_serializedObject.FindProperty($"{nameof(PlaneObject)}"));
                GUILayout.Space(10);
                EditorGUILayout.PropertyField(_serializedObject.FindProperty($"{nameof(InsideMaterial)}"));
                GUILayout.Space(10);
            }

            if (_serializedObject.FindProperty($"{nameof(VictimObject)}").objectReferenceValue is null)
            {
                EditorGUILayout.TextArea("Attach The Destruction Target",
                    SmasherDestructionConstantValues.GetGUIStyle_LabelNotice());
                return;
            }

            // 隙間を つくるか
            _makeGap = EditorGUILayout.Toggle("Make Gap", _makeGap);
            GUILayout.Space(10);

            // ファイル名
            GUILayout.Label("Fragment File Name",
                SmasherDestructionConstantValues.GetGUIStyle_LabelSmall());

            GUILayout.Space(10);
            _meshName = GUILayout.TextArea(_meshName);
            GUILayout.Space(10);

            // メッシュ編集 実行ボタン
            if (GUILayout.Button("Frag Mesh"))
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
            }

            GUILayout.Space(10);

            // メッシュ 保存ボタン
            if (GUILayout.Button("Save Meshes"))
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

            GUILayout.Space(10);

            // 切断面 の 位置 指定
            _planeAnchorPos = EditorGUILayout.Vector3Field("PlaneObject Anchor-Position", _planeAnchorPos);
            // 切断面 の 位置 リセット
            if (EditorGUILayout.LinkButton("Reset Value"))
            {
                _planeAnchorPos = Vector3.zero;
            }

            GUILayout.Space(10);

            // リセットボタン
            if (GUILayout.Button(
                    "Reset All",
                    SmasherDestructionConstantValues.GetGUIStyle_Button()))
            {
                ResetFeilds();
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

        /// <summary>
        /// モードに応じてセーブ処理をする
        /// </summary>
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
            PrefabUtility.SaveAsPrefabAsset(_fragmentsParent,
                Gouwang.CuttedMeshesPrefabFolderAbsolutePath + $"{_meshName}.prefab");

            #endregion
        }
    }
}