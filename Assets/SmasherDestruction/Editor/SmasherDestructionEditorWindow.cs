using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SmasherDestruction.Editor
{
    public class SmasherDestructionEditorWindow : EditorWindow
    {
        private void OnEnable()
        {
            var elem = Resources.Load<VisualTreeAsset>("SmasherDestruction/UIDocuments/SmasherDestructionUIDocuments");
            elem.CloneTree(rootVisualElement);
        }

        private void OnGUI()
        {
        }

        private void OnDisable()
        {
        }
    }
}