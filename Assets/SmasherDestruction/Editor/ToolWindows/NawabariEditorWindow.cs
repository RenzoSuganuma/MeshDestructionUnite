using System.Collections.Generic;
using SmasherDestruction.Editor;
using UnityEditor;
using UnityEngine;

namespace GouwangDestruction.Editor
{
    public class NawabariEditorWindow : EditorWindow
    {
        /// <summary> 切断対象のオブジェクト </summary>
        public GameObject VictimObject;

        /// <summary> 切断面のマテリアル </summary>
        public Material InsideMaterial;

        /// <summary>
        /// オブジェクトをウィンドウにアタッチできるように宣言
        /// </summary>
        private SerializedObject _serializedObject;

        private List<GameObject> _fragmentsObject = new List<GameObject>();
        private GameObject _fragmentsParent;
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
            _fragmentsObject = null;
            _fragmentsParent = null;
            _meshName = "";
            _makeGap = false;
        }

        private void Draw()
        {
            // フラグモード ラベル
            GUILayout.Label("Nawabari",
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
            if (GUILayout.Button("Frag Mesh"))
            {
                Nawabari.ExecuteFragmentation(
                    _pointCount,
                    VictimObject,
                    _fragmentsObject,
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
            if (GUILayout.Button(
                    SmasherDestructionConstantValues.SaveToStorageFragmentMeshesFileLabel
                    ))
            {
                CheckDirectory();
                SaveCuttedMeshes();
            }

            GUILayout.Space(10);

            // リセットボタン
            if (GUILayout.Button(
                    SmasherDestructionConstantValues.ResetAllOptionsLabel,
                    SmasherDestructionConstantValues.GetGUIStyle_Button()))
            {
                ResetFeilds();
            }

            GUILayout.Space(10);
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
            if (_fragmentsObject.Count < 1) return;

            SmasherDestructionEditorUtility.FindSaveTargetDirectory(
                SmasherDestructionEditorUtility.CuttedMeshesFolderAbsolutePath +
                $"{_meshName}/");
            SmasherDestructionEditorUtility.FindSaveTargetDirectory(SmasherDestructionEditorUtility
                .CuttedMeshesPrefabFolderAbsolutePath);

            // コンポーネントのアタッチ
            foreach (var cuttedMesh in _fragmentsObject)
            {
                var mc = cuttedMesh.AddComponent<MeshCollider>();
                mc.convex = true;
                cuttedMesh.GetComponent<MeshCollider>().sharedMesh = cuttedMesh.GetComponent<MeshFilter>().sharedMesh;
                var rb = cuttedMesh.AddComponent<Rigidbody>();
            }

            #region 保存処理

            // 断片化されたメッシュのアセットとしての保存処理
            for (int i = 0; i < _fragmentsObject.Count; ++i)
            {
                var mesh = _fragmentsObject[i].GetComponent<MeshFilter>().sharedMesh;

                SmasherDestructionEditorUtility.CreateAndSaveToAsset(mesh, _meshName, i);
            }

            // プレハブとして保存
            SmasherDestructionEditorUtility.SaveAsPrefab(_fragmentsParent, _meshName);

            #endregion
        }
    }
}