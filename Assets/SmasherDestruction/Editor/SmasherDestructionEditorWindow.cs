using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace SmasherDestruction.Editor
{
    public class SmasherDestructionEditorWindow : EditorWindow
    {
        public static ObjectToMakeMeshes Objects { get; private set; }

        [System.Serializable]
        public class ObjectToMakeMeshes
        {
            public GameObject TrackingObject;
        }

        [SerializeField] private ObjectToMakeMeshes _objects = null;

        private SerializedObject _serializedObject = null;

        [MenuItem("Tools/MyDebugWindow")]
        private static void Open()
        {
            GetWindow<SmasherDestructionEditorWindow>();
        }

        private void OnEnable()
        {
            Objects = _objects;
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
            EditorGUILayout.PropertyField(_serializedObject.FindProperty($"{nameof(_objects)}"));

            _serializedObject.ApplyModifiedProperties();
        }
    }
}