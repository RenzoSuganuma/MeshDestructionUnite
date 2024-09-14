using System;
using System.Collections.Generic;
using System.Linq;
using SmasherDestruction.Editor;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GouwangDestruction.Editor
{
    public class NawabariModokiEditorWindow : EditorWindow
    {
        /// <summary> 切断対象のオブジェクト </summary>
        public GameObject VictimObject;

        /// <summary> 切断面のマテリアル </summary>
        public Material InsideMaterial;

        /// <summary>
        /// オブジェクトをウィンドウにアタッチできるように宣言
        /// </summary>
        private SerializedObject _serializedObject;

        private List<GameObject> _fragmentsObjects = new List<GameObject>();
        private GameObject _fragmentsParent;
        private GameObject _copiedParent;
        private string _meshName;
        private int _pointCount;
        private bool _makeGap;

        private void OnEnable()
        {
            _serializedObject = new SerializedObject(this);
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
            if (GUILayout.Button(
                    SmasherDestructionConstantValues.CloseWindowLabel
                ))
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
            VictimObject = null;
            InsideMaterial = null;
            _fragmentsObjects = null;
            _fragmentsParent = null;
            _meshName = "";
            _makeGap = false;
            _pointCount = 0;

            DestroyTemporaryObject();
        }

        private void Draw()
        {
            // フラグモード ラベル
            GUILayout.Label("NawabariModoki",
                SmasherDestructionConstantValues.GetGUIStyle_LabelSmall());
            GUILayout.Space(10);

            GUILayout.Space(10);

            if (_serializedObject is not null)
            {
                GUILayout.Space(10);
                EditorGUILayout.PropertyField(_serializedObject.FindProperty($"{nameof(VictimObject)}"));
                GUILayout.Space(10);
                EditorGUILayout.PropertyField(_serializedObject.FindProperty($"{nameof(InsideMaterial)}"));
                GUILayout.Space(10);
            }

            if (_serializedObject.FindProperty($"{nameof(VictimObject)}").objectReferenceValue is null)
            {
                EditorGUILayout.TextArea(SmasherDestructionConstantValues.AssingFragmentationTargetLabel,
                    SmasherDestructionConstantValues.GetGUIStyle_LabelNotice());
                return;
            }

            // 隙間を つくるか
            _makeGap = EditorGUILayout.Toggle("Make Gap", _makeGap);
            GUILayout.Space(10);

            _pointCount = EditorGUILayout.IntField("Point Count", _pointCount);

            // ファイル名
            GUILayout.Label(SmasherDestructionConstantValues.FragmentMeshesFileNameLabel,
                SmasherDestructionConstantValues.GetGUIStyle_LabelSmall());

            GUILayout.Space(10);
            _meshName = GUILayout.TextArea(_meshName);
            GUILayout.Space(10);

            // メッシュ編集 実行ボタン
            if (GUILayout.Button("Frag Mesh", SmasherDestructionConstantValues.GetGUIStyle_ExecuteButton()))
            {
                NawabariModoki.ExecuteFragmentation(
                    _pointCount,
                    VictimObject,
                    _fragmentsObjects,
                    InsideMaterial,
                    _makeGap);

                // 断片のオブジェクトを１つにまとめる
                var parentObj = new GameObject();
                parentObj.name = _meshName;
                _fragmentsParent = parentObj;
                foreach (var fragment in _fragmentsObjects)
                {
                    fragment.transform.SetParent(parentObj.transform);
                }

                SmasherDestructionEditorUtility.PaintFragments(_fragmentsObjects.Count, out _copiedParent,
                    _fragmentsParent, InsideMaterial);
            }

            GUILayout.Space(10);

            // メッシュ 保存ボタン
            if (GUILayout.Button(
                    SmasherDestructionConstantValues.SaveToStorageFragmentMeshesFileLabel,
                    SmasherDestructionConstantValues.GetGUIStyle_SaveButton()
                ))
            {
                CheckDirectory();
                DestroyTemporaryObject();
                SaveCuttedMeshes();
            }

            GUILayout.Space(10);

            // リセットボタン
            if (GUILayout.Button(
                    SmasherDestructionConstantValues.ResetAllOptionsLabel,
                    SmasherDestructionConstantValues.GetGUIStyle_ScaryButton()))
            {
                ResetFeilds();
            }

            GUILayout.Space(10);
        }

        private void DestroyTemporaryObject()
        {
            if (_fragmentsParent is not null)
            {
                _fragmentsParent.SetActive(true);
            }

            // 強調用のオブジェクトを破棄
            if (_copiedParent is not null)
            {
                GameObject.DestroyImmediate(_copiedParent);
                _copiedParent = null;
            }
        }

        private void CheckDirectory()
        {
            SmasherDestructionEditorUtility.FindSaveTargetDirectory(
                SmasherDestructionEditorUtility.CuttedMeshesFolderAbsolutePath +
                $"{_meshName}/");
            SmasherDestructionEditorUtility.FindSaveTargetDirectory(SmasherDestructionEditorUtility
                .CuttedMeshesPrefabFolderAbsolutePath);
        }

        /// <summary>
        /// モードに応じてセーブ処理をする
        /// </summary>
        private void SaveCuttedMeshes() // 保存先のパスにメッシュのアセットとプレハブを保存する
        {
            if (_fragmentsObjects.Count < 1) return;

            SmasherDestructionEditorUtility.FindSaveTargetDirectory(
                SmasherDestructionEditorUtility.CuttedMeshesFolderAbsolutePath +
                $"{_meshName}/");
            SmasherDestructionEditorUtility.FindSaveTargetDirectory(SmasherDestructionEditorUtility
                .CuttedMeshesPrefabFolderAbsolutePath);

            // コンポーネントのアタッチ
            foreach (var cuttedMesh in _fragmentsObjects)
            {
                var mc = cuttedMesh.AddComponent<MeshCollider>();
                mc.convex = true;
                cuttedMesh.GetComponent<MeshCollider>().sharedMesh = cuttedMesh.GetComponent<MeshFilter>().sharedMesh;
                var rb = cuttedMesh.AddComponent<Rigidbody>();
            }

            #region 保存処理

            // 断片化されたメッシュのアセットとしての保存処理
            for (int i = 0; i < _fragmentsObjects.Count; ++i)
            {
                var mesh = _fragmentsObjects[i].GetComponent<MeshFilter>().sharedMesh;

                SmasherDestructionEditorUtility.CreateAndSaveToAsset(mesh, _meshName, i);
            }

            // プレハブとして保存
            SmasherDestructionEditorUtility.SaveAsPrefab(_fragmentsParent, _meshName);

            #endregion
        }
    }
}