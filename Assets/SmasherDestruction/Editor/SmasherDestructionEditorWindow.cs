using System;
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
        public GameObject Victim;

        private SerializedObject _serializedObject;
        private int _mode;
        
        private void OnEnable()
        {
            _serializedObject = new SerializedObject(this);
        }

        private void Update()
        {
            Repaint(); // 毎フレーム内容が更新されるようにする
        }

        [DrawGizmo(GizmoType.NonSelected)]
        private void OnGUI()
        {
            _serializedObject.Update();

            // プロパティを表示して編集可能にする
            EditorGUILayout.TextArea("SmasherDestruction : Experimental",
                SmasherDestructionConstantValues.GetGUIStyle_LabelBig());

            if (_serializedObject is not null)
            {
                EditorGUILayout.PropertyField(_serializedObject.FindProperty($"{nameof(Victim)}"));
            }

            GUILayout.Label("Fragmentation Mode", SmasherDestructionConstantValues.GetGUIStyle_LabelSmall());

            _mode = GUILayout.Toolbar(_mode,
                new GUIContent[3] { new GUIContent("Ryden"), new GUIContent("ArmStrong"), new GUIContent("Smasher") });

            if (GUILayout.Button(
                    _mode switch
                    {
                        0 => "Cut Mesh",
                        1 => "Frag Mesh",
                        2 => "Smash Mesh",
                        _ => ""
                    }))
            {
            }

            if (GUILayout.Button("Save Mesh"))
            {
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
    }
}