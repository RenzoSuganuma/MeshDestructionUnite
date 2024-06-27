using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace SmasherDestruction.Editor
{
    public class SmasherDestructionEditor : EditorWindow
    {
        [MenuItem("Window/SmasherDestruction")]
        public static void ShowSmasherWindow()
        {
            var window = GetWindow<SmasherDestructionEditorWindow>();
            window.titleContent = new GUIContent("SmasherDestructionEditor");
        }
    }
}