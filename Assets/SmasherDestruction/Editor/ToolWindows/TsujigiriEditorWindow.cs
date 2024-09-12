using System.Collections.Generic;
using SmasherDestruction.Editor;
using UnityEditor;
using UnityEngine;

namespace GouwangDestruction.Editor
{
    /// <summary>
    /// 辻斬りのエディタ画面
    /// </summary>
    public class TsujigiriEditorWindow : EditorWindow
    {
        /// <summary> 切断対象のオブジェクト </summary>
        public GameObject VictimObject;

        /// <summary> 切断平面のオブジェクト </summary>
        public Transform PlaneObject;

        /// <summary> 切断面のマテリアル </summary>
        public Material InsideMaterial;

        public GameObject CutterPlane { get; set; }

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
            GUILayout.Label("Tujigiri",
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
                EditorGUILayout.TextArea(SmasherDestructionConstantValues.AssingFragmentationTargetLabel,
                    SmasherDestructionConstantValues.GetGUIStyle_LabelNotice());
                return;
            }

            // 隙間を つくるか
            _makeGap = EditorGUILayout.Toggle("Make Gap", _makeGap);
            GUILayout.Space(10);

            // ファイル名
            GUILayout.Label(SmasherDestructionConstantValues.FragmentMeshesFileNameLabel,
                SmasherDestructionConstantValues.GetGUIStyle_LabelSmall());

            GUILayout.Space(10);
            _meshName = GUILayout.TextArea(_meshName);
            GUILayout.Space(10);

            // メッシュ編集 実行ボタン
            if (GUILayout.Button("Cut Mesh",
                    SmasherDestructionConstantValues.GetGUIStyle_ExecuteButton()))
            {
                TsujigiriUtility.CutTheMesh(
                    VictimObject,
                    _fragmentsObject,
                    _planeAnchorPos,
                    PlaneObject.up,
                    InsideMaterial,
                    _makeGap);
            }

            GUILayout.Space(10);

            // メッシュ 保存ボタン
            if (GUILayout.Button(
                    SmasherDestructionConstantValues.SaveToStorageFragmentMeshesFileLabel,
                    SmasherDestructionConstantValues.GetGUIStyle_SaveButton()
                ))
            {
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
                    SmasherDestructionConstantValues.ResetAllOptionsLabel,
                    SmasherDestructionConstantValues.GetGUIStyle_ScaryButton()))
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

            CheckDirectory();

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

                SmasherDestructionEditorUtility.CreateAndSaveToAsset(mesh, _meshName, i);
            }

            // プレハブとして保存
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

            SmasherDestructionEditorUtility.SaveAsPrefab(_fragmentsParent, _meshName);

            #endregion
        }
    }
}